#include "RecastClient.h"

static float frand()
{
	//	return ((float)(rand() & 0xffff)/(float)0xffff);
	return (float)rand() / (float)RAND_MAX;
}

RecastWrapper::RecastClient::RecastClient() :
	m_pmesh(0),
	m_dmesh(0),
	m_navMesh(0),
	m_navQuery(0),
	m_crowd(0),
	m_ctx(0)
{
	m_vod = dtAllocObstacleAvoidanceDebugData();
	m_vod->init(2048);

	memset(&m_agentDebug, 0, sizeof(m_agentDebug));
	m_agentDebug.idx = -1;
	m_agentDebug.vod = m_vod;

	m_navQuery = dtAllocNavMeshQuery();
	m_crowd = dtAllocCrowd();
}

RecastWrapper::RecastClient::~RecastClient()
{
	cleanup();
}

void RecastWrapper::RecastClient::cleanup()
{
	rcFreePolyMesh(m_pmesh);
	rcFreePolyMeshDetail(m_dmesh);
	dtFreeNavMesh(m_navMesh);
	dtFreeNavMeshQuery(m_navQuery);
	dtFreeCrowd(m_crowd);
	dtFreeObstacleAvoidanceDebugData(m_vod);

	if (m_ctx)
	{
		delete m_ctx;
	}
}

bool RecastWrapper::RecastClient::prepareBuild()
{	
	return false;
}

bool RecastWrapper::RecastClient::doBuild()
{	
	return false;
}

dtStatus RecastWrapper::RecastClient::findRandomPointAroundCircle(dtPolyRef startRefLocal, const float* center, float maxRadius, dtPolyRef* randomRefLocal, float* point)
{
	const dtQueryFilter* filter = m_crowd->getFilter(0);
	return m_navQuery->findRandomPointAroundCircle(startRefLocal, center, maxRadius, filter, frand, randomRefLocal, point);
}

dtStatus RecastWrapper::RecastClient::findPath(dtPolyRef startRef, dtPolyRef endRef, const float* startPos, const float* endPos, dtPolyRef* path, int* pathCount)
{
	const dtQueryFilter* filter = m_crowd->getFilter(0);
	return m_navQuery->findPath(startRef, endRef, startPos, endPos, filter, path, pathCount, MAX_PATH_POLYS);
}

inline bool findPathInRange(const float* v1, const float* v2, const float r, const float h)
{
	const float dx = v2[0] - v1[0];
	const float dy = v2[1] - v1[1];
	const float dz = v2[2] - v1[2];
	return (dx*dx + dz*dz) < r*r && fabsf(dy) < h;
}


static int findPathFixupCorridor(dtPolyRef* path, const int npath, const int maxPath,
	const dtPolyRef* visited, const int nvisited)
{
	int furthestPath = -1;
	int furthestVisited = -1;

	// Find furthest common polygon.
	for (int i = npath - 1; i >= 0; --i)
	{
		bool found = false;
		for (int j = nvisited - 1; j >= 0; --j)
		{
			if (path[i] == visited[j])
			{
				furthestPath = i;
				furthestVisited = j;
				found = true;
			}
		}
		if (found)
			break;
	}

	// If no intersection found just return current path. 
	if (furthestPath == -1 || furthestVisited == -1)
		return npath;

	// Concatenate paths.	

	// Adjust beginning of the buffer to include the visited.
	const int req = nvisited - furthestVisited;
	const int orig = rcMin(furthestPath + 1, npath);
	int size = rcMax(0, npath - orig);
	if (req + size > maxPath)
		size = maxPath - req;
	if (size)
		memmove(path + req, path + orig, size * sizeof(dtPolyRef));

	// Store visited
	for (int i = 0; i < req; ++i)
		path[i] = visited[(nvisited - 1) - i];

	return req + size;
}

// This function checks if the path has a small U-turn, that is,
// a polygon further in the path is adjacent to the first polygon
// in the path. If that happens, a shortcut is taken.
// This can happen if the target (T) location is at tile boundary,
// and we're (S) approaching it parallel to the tile edge.
// The choice at the vertex can be arbitrary, 
//  +---+---+
//  |:::|:::|
//  +-S-+-T-+
//  |:::|   | <-- the step can end up in here, resulting U-turn path.
//  +---+---+
static int findPathFixupShortcuts(dtPolyRef* path, int npath, dtNavMeshQuery* navQuery)
{
	if (npath < 3)
		return npath;

	// Get connected polygons
	static const int maxNeis = 16;
	dtPolyRef neis[maxNeis];
	int nneis = 0;

	const dtMeshTile* tile = 0;
	const dtPoly* poly = 0;
	if (dtStatusFailed(navQuery->getAttachedNavMesh()->getTileAndPolyByRef(path[0], &tile, &poly)))
		return npath;

	for (unsigned int k = poly->firstLink; k != DT_NULL_LINK; k = tile->links[k].next)
	{
		const dtLink* link = &tile->links[k];
		if (link->ref != 0)
		{
			if (nneis < maxNeis)
				neis[nneis++] = link->ref;
		}
	}

	// If any of the neighbour polygons is within the next few polygons
	// in the path, short cut to that polygon directly.
	static const int maxLookAhead = 6;
	int cut = 0;
	for (int i = dtMin(maxLookAhead, npath) - 1; i > 1 && cut == 0; i--) {
		for (int j = 0; j < nneis; j++)
		{
			if (path[i] == neis[j]) {
				cut = i;
				break;
			}
		}
	}
	if (cut > 1)
	{
		int offset = cut - 1;
		npath -= offset;
		for (int i = 1; i < npath; i++)
			path[i] = path[i + offset];
	}

	return npath;
}

static bool getSteerTarget(dtNavMeshQuery* navQuery, const float* startPos, const float* endPos,
	const float minTargetDist,
	const dtPolyRef* path, const int pathSize,
	float* steerPos, unsigned char& steerPosFlag, dtPolyRef& steerPosRef,
	float* outPoints = 0, int* outPointCount = 0)
{
	// Find steer target.
	static const int MAX_STEER_POINTS = 3;
	float steerPath[MAX_STEER_POINTS * 3];
	unsigned char steerPathFlags[MAX_STEER_POINTS];
	dtPolyRef steerPathPolys[MAX_STEER_POINTS];
	int nsteerPath = 0;
	navQuery->findStraightPath(startPos, endPos, path, pathSize,
		steerPath, steerPathFlags, steerPathPolys, &nsteerPath, MAX_STEER_POINTS);
	if (!nsteerPath)
		return false;

	if (outPoints && outPointCount)
	{
		*outPointCount = nsteerPath;
		for (int i = 0; i < nsteerPath; ++i)
			dtVcopy(&outPoints[i * 3], &steerPath[i * 3]);
	}


	// Find vertex far enough to steer to.
	int ns = 0;
	while (ns < nsteerPath)
	{
		// Stop at Off-Mesh link or when point is further than slop away.
		if ((steerPathFlags[ns] & DT_STRAIGHTPATH_OFFMESH_CONNECTION) ||
			!findPathInRange(&steerPath[ns * 3], startPos, minTargetDist, 1000.0f))
			break;
		ns++;
	}
	// Failed to find good point to steer to.
	if (ns >= nsteerPath)
		return false;

	dtVcopy(steerPos, &steerPath[ns * 3]);
	steerPos[1] = startPos[1];
	steerPosFlag = steerPathFlags[ns];
	steerPosRef = steerPathPolys[ns];

	return true;
}

dtStatus RecastWrapper::RecastClient::getSmoothPath(dtPolyRef startRef, const float* startPos, const float* endPos, dtPolyRef* path, int pathCount, float* pathPoints, int* smoothPathPoints)
{
	// Iterate over the path to find smooth path on the detail mesh surface.
	dtPolyRef polys[MAX_PATH_POLYS];
	memcpy(polys, path, sizeof(dtPolyRef)*pathCount);
	int npolys = pathCount;

	float iterPos[3], targetPos[3];
	m_navQuery->closestPointOnPoly(startRef, startPos, iterPos, 0);
	m_navQuery->closestPointOnPoly(polys[npolys - 1], endPos, targetPos, 0);

	static const float STEP_SIZE = 2;
	static const float SLOP = 0.0001;

	int smoothPathCount = 0;
	dtVcopy(&pathPoints[smoothPathCount * 3], iterPos);
	smoothPathCount++;

	// Move towards target a small advancement at a time until target reached or
	// when ran out of memory to store the path.
	while (npolys && smoothPathCount < MAX_PATH_SMOOTH)
	{
		// Find location to steer towards.
		float steerPos[3];
		unsigned char steerPosFlag;
		dtPolyRef steerPosRef;

		if (!getSteerTarget(m_navQuery, iterPos, targetPos, SLOP,
			polys, npolys, steerPos, steerPosFlag, steerPosRef))
			break;

		bool endOfPath = (steerPosFlag & DT_STRAIGHTPATH_END) ? true : false;
		bool offMeshConnection = (steerPosFlag & DT_STRAIGHTPATH_OFFMESH_CONNECTION) ? true : false;

		// Find movement delta.
		float delta[3], len;
		dtVsub(delta, steerPos, iterPos);
		len = dtMathSqrtf(dtVdot(delta, delta));

		// If the steer target is end of path or off-mesh link, do not move past the location.
		if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
			len = 1;
		else
			len = STEP_SIZE / len;
		float moveTgt[3];
		dtVmad(moveTgt, iterPos, delta, len);

		// Move
		float result[3];
		dtPolyRef visited[16];
		int nvisited = 0;
		m_navQuery->moveAlongSurface(polys[0], iterPos, moveTgt, m_crowd->getFilter(0),
			result, visited, &nvisited, 16);

		npolys = findPathFixupCorridor(polys, npolys, MAX_PATH_POLYS, visited, nvisited);
		npolys = findPathFixupShortcuts(polys, npolys, m_navQuery);

		float h = 0;
		m_navQuery->getPolyHeight(polys[0], result, &h);
		result[1] = h;
		dtVcopy(iterPos, result);

		// Handle end of path and off-mesh links when close enough.
		if (endOfPath && findPathInRange(iterPos, steerPos, SLOP, 1.0f))
		{
			// Reached end of path.
			dtVcopy(iterPos, targetPos);
			if (smoothPathCount < MAX_PATH_SMOOTH)
			{
				dtVcopy(&pathPoints[smoothPathCount * 3], iterPos);
				smoothPathCount++;
			}
			break;
		}
		else if (offMeshConnection && findPathInRange(iterPos, steerPos, SLOP, 1.0f))
		{
			// Reached off-mesh connection.
			float startPos[3], endPos[3];

			// Advance the path up to and over the off-mesh connection.
			dtPolyRef prevRef = 0, polyRef = polys[0];
			int npos = 0;
			while (npos < npolys && polyRef != steerPosRef)
			{
				prevRef = polyRef;
				polyRef = polys[npos];
				npos++;
			}
			for (int i = npos; i < npolys; ++i)
				polys[i - npos] = polys[i];
			npolys -= npos;

			// Handle the connection.
			dtStatus status = m_navMesh->getOffMeshConnectionPolyEndPoints(prevRef, polyRef, startPos, endPos);
			if (dtStatusSucceed(status))
			{
				if (smoothPathCount < MAX_PATH_SMOOTH)
				{
					dtVcopy(&pathPoints[smoothPathCount * 3], startPos);
					smoothPathCount++;
					// Hack to make the dotted path not visible during off-mesh connection.
					if (smoothPathCount & 1)
					{
						dtVcopy(&pathPoints[smoothPathCount * 3], startPos);
						smoothPathCount++;
					}
				}
				// Move position at the other side of the off-mesh link.
				dtVcopy(iterPos, endPos);
				float eh = 0.0f;
				m_navQuery->getPolyHeight(polys[0], iterPos, &eh);
				iterPos[1] = eh;
			}
		}

		// Store results.
		if (smoothPathCount < MAX_PATH_SMOOTH)
		{
			dtVcopy(&pathPoints[smoothPathCount * 3], iterPos);
			smoothPathCount++;
		}
	}

	*smoothPathPoints = smoothPathCount;

	return DT_SUCCESS;
}

dtStatus RecastWrapper::RecastClient::addObstacle(const float* pos, float radius, float height, dtObstacleRef* ref)
{
	return DT_FAILURE;
}

dtStatus RecastWrapper::RecastClient::addObstacleBox(const float* bmin, const float* bmax, dtObstacleRef* ref)
{
	return DT_FAILURE;
}

dtStatus RecastWrapper::RecastClient::removeObstacle(dtObstacleRef ref)
{
	return DT_FAILURE;
}

void RecastWrapper::RecastClient::clearObstacles()
{
}

bool RecastWrapper::RecastClient::configure(GDX::AI::ProtoRecastSettings* proto)
{	
	for (int i = 0; i < 3; i++) {
		m_worldBoundsMin[i] = proto->world_bounds_min(i);
		m_worldBoundsMax[i] = proto->world_bounds_max(i);
	}
	
	m_cellSize = proto->cell_size();
	m_cellHeight = proto->cell_height();
	m_agentMaxSlope = proto->agent_max_slope();
	m_agentHeight = proto->agent_height();
	m_agentMaxClimb = proto->agent_max_climb();
	m_agentRadius = proto->agent_radius();
	m_edgeMaxLen = proto->edge_max_len();
	m_edgeMaxError = proto->edge_max_error();
	m_regionMinSize = proto->region_min_size();
	m_regionMergeSize = proto->region_merge_size();
	m_detailSampleDist = proto->detail_sample_dist();
	m_detailSampleMaxError = proto->detail_sample_max_error();

	m_filterLowHangingObstacles = proto->filter_low_hanging_obstacles();
	m_filterLedgeSpans = proto->filter_ledge_spans();
	m_filterWalkableLowHeightSpans = proto->filter_walkable_low_height_spans();

	m_maxAgents = proto->max_agents();

	m_partitionType = (RecastWrapper::PartitionType)proto->partition_type();

	return true;
}
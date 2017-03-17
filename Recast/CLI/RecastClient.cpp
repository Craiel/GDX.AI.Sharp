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
	rcFreePolyMesh(m_pmesh);
	rcFreePolyMeshDetail(m_dmesh);
	dtFreeNavMesh(m_navMesh);
	dtFreeNavMeshQuery(m_navQuery);
	dtFreeCrowd(m_crowd);
	dtFreeObstacleAvoidanceDebugData(m_vod);

	if(m_ctx)
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

bool RecastWrapper::RecastClient::getDebugNavMesh(const unsigned short polyFlags, GDX::AI::ProtoRecastDebugNavMesh* proto)
{
	if(!m_navMesh)
	{
		return false;
	}
	
	const dtNavMesh& mesh = *m_navMesh;
	for (int i = 0; i < mesh.getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = mesh.getTile(i);
		if (!tile->header) continue;
		dtPolyRef base = m_navMesh->getPolyRefBase(tile);

		GDX::AI::ProtoRecastDebugNavMeshTile* protoTile = proto->add_tiles();
		int tileVerts = tile->header->vertCount;
		for (int i = 0; i < tileVerts; i++)
		{
			float* vert = &tile->verts[i*3];
			GDX::AI::ProtoNavMeshVector* vertex = protoTile->add_vertices();
			vertex->set_x(vert[0]);
			vertex->set_y(vert[1]);
			vertex->set_z(vert[2]);
		}

		for (int j = 0; j < tile->header->polyCount; ++j)
		{
			const dtPoly* p = &tile->polys[j];
			if ((p->flags & polyFlags) != 0) continue;
			
			const dtMeshTile* tile = 0;
			const dtPoly* poly = 0;
			if (dtStatusFailed(m_navMesh->getTileAndPolyByRef(base | (dtPolyRef)j, &tile, &poly)))
			{
				return false;
			}

			const unsigned int ip = (unsigned int)(poly - tile->polys);

			if (poly->getType() == DT_POLYTYPE_OFFMESH_CONNECTION)
			{
				dtOffMeshConnection* con = &tile->offMeshCons[ip - tile->header->offMeshBase];

				GDX::AI::ProtoRecastDebugNavMeshOffMeshConnection* connection = proto->add_off_mesh_connections();
				
				connection->set_flags(con->flags);

				GDX::AI::ProtoNavMeshVector* point_0 = new GDX::AI::ProtoNavMeshVector();
				point_0->set_x(con->pos[0]);
				point_0->set_y(con->pos[1]);
				point_0->set_z(con->pos[2]);
				connection->set_allocated_p_0(point_0);

				GDX::AI::ProtoNavMeshVector* point_1 = new GDX::AI::ProtoNavMeshVector();
				point_1->set_x(con->pos[3]);
				point_1->set_y(con->pos[4]);
				point_1->set_z(con->pos[5]);
				connection->set_allocated_p_1(point_1);
			} else
			{
				const dtPolyDetail* pd = &tile->detailMeshes[ip];
				for (int i = 0; i < pd->triCount; ++i)
				{
					const unsigned char* t = &tile->detailTris[(pd->triBase + i) * 4];
					//GDX::AI::ProtoRecastDebugNavMeshTriangle* triangle = proto->add_triangles();
					unsigned short tri[3];
					for (int j = 0; j < 3; ++j)
					{
						if (t[j] < poly->vertCount)
							tri[j] = poly->verts[t[j]];
						else
							tri[j] = (pd->vertBase + t[j] - poly->vertCount) * 3;
					}

					GDX::AI::ProtoNavMeshTriangle* triangle = protoTile->add_triangles();
					triangle->set_x(tri[0]);
					triangle->set_y(tri[1]);
					triangle->set_z(tri[2]);
				}
			}
		}
	}

	return true;
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
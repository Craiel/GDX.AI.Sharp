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

bool RecastWrapper::RecastClient::build(std::string geom_path)
{
	InputGeom* geom = new InputGeom();
	geom->load(m_ctx, geom_path);
	bool result = build(geom);
	delete geom;
	return result;
}

bool RecastWrapper::RecastClient::build(class InputGeom* geom)
{
	m_geom = geom;
	m_ctx = new BuildContext();

	const float* bmin = m_geom->getNavMeshBoundsMin();
	const float* bmax = m_geom->getNavMeshBoundsMax();
	const float* verts = m_geom->getMesh()->getVerts();
	const int nverts = m_geom->getMesh()->getVertCount();
	const int* tris = m_geom->getMesh()->getTris();
	const int ntris = m_geom->getMesh()->getTriCount();

	prepareBuild(geom);

	// Reset build times gathering.
	m_ctx->resetTimers();

	// Start the build process.	
	m_ctx->startTimer(RC_TIMER_TOTAL);

	m_ctx->log(RC_LOG_PROGRESS, "Building navigation:");
	m_ctx->log(RC_LOG_PROGRESS, " - %d x %d cells", m_cfg.width, m_cfg.height);
	m_ctx->log(RC_LOG_PROGRESS, " - %.1fK verts, %.1fK tris", nverts / 1000.0f, ntris / 1000.0f);

	doBuild();
	
	m_ctx->stopTimer(RC_TIMER_TOTAL);
	
	return true;
}

bool RecastWrapper::RecastClient::prepareBuild(class InputGeom* geom)
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
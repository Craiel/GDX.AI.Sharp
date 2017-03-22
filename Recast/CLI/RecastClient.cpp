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
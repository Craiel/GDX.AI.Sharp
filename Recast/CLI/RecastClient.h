#pragma once

#include <Recast.h>
#include <DetourNavMesh.h>
#include <DetourNavMeshQuery.h>
#include <DetourNavMeshBuilder.h>
#include <DetourCrowd.h>
#include <NavMesh.pb.h>

#include "BuildContext.h"
#include "InputGeom.h"

namespace RecastWrapper {
	enum PartitionType
	{
		PARTITION_WATERSHED,
		PARTITION_MONOTONE,
		PARTITION_LAYERS,
	};

	enum PolyAreas
	{
		POLYAREA_GROUND,
		POLYAREA_WATER,
		POLYAREA_ROAD,
		POLYAREA_DOOR,
		POLYAREA_GRASS,
		POLYAREA_JUMP,
	};

	enum PolyFlags
	{
		POLYFLAGS_WALK = 0x01,		// Ability to walk (ground, grass, road)
		POLYFLAGS_SWIM = 0x02,		// Ability to swim (water).
		POLYFLAGS_DOOR = 0x04,		// Ability to move through doors.
		POLYFLAGS_JUMP = 0x08,		// Ability to jump.
		POLYFLAGS_DISABLED = 0x10,		// Disabled polygon
		POLYFLAGS_ALL = 0xffff	// All abilities.
	};

	class RecastClient
	{
	protected:
		rcConfig m_cfg;

		dtCrowdAgentDebugInfo m_agentDebug;
		dtObstacleAvoidanceDebugData* m_vod;

		rcPolyMesh* m_pmesh;
		rcPolyMeshDetail* m_dmesh;

		rcHeightfield* m_solid;
		rcCompactHeightfield* m_chf;
		rcContourSet* m_cset;

		class BuildContext* m_ctx;

		class dtNavMesh* m_navMesh;
		class dtNavMeshQuery* m_navQuery;
		class dtCrowd* m_crowd;

		class InputGeom* m_geom;
		
	protected:
		float m_cellSize = 0.3f;
		float m_cellHeight = 0.2f;
		float m_agentMaxSlope = 45.0f;
		float m_agentHeight = 2.0f;
		float m_agentMaxClimb = 0.9f;
		float m_agentRadius = 0.6f;
		float m_edgeMaxLen = 12.0f;
		float m_edgeMaxError = 1.3f;
		float m_regionMinSize = 8;
		float m_regionMergeSize = 20;
		float m_detailSampleDist = 6.0f;
		float m_detailSampleMaxError = 1.0f;

		bool m_filterLowHangingObstacles = true;
		bool m_filterLedgeSpans = true;
		bool m_filterWalkableLowHeightSpans = true;

		int m_maxAgents = 1000;

		PartitionType m_partitionType = PARTITION_WATERSHED;

	public:
		RecastClient();
		~RecastClient();

		bool build(std::string geom_path);
		bool build(class InputGeom* geom);

		BuildContext* getContext() { return m_ctx; }

		void update(float delta) { m_crowd->update(delta, &m_agentDebug); }

		int addAgent(const float* pos, const dtCrowdAgentParams* params) { return m_crowd->addAgent(pos, params); }

		const dtCrowdAgent* getAgent(int index) { return m_crowd->getAgent(index); }

		dtStatus findNearestPoly(const float* center, const float* extents, dtPolyRef* nearestRef, float* nearestPoint)
		{
			const dtQueryFilter* filter = m_crowd->getFilter(0);
			return m_navQuery->findNearestPoly(center, extents, filter, nearestRef, nearestPoint);
		}

		dtStatus findRandomPointAroundCircle(dtPolyRef startRefLocal, const float* center_start, float maxRadius, dtPolyRef* randomRefLocal, float* point);

		bool requestMoveTarget(int index, dtPolyRef targetRef, const float* pos) { return m_crowd->requestMoveTarget(index, targetRef, pos); }

		bool resetMoveTarget(int index) { return m_crowd->resetMoveTarget(index); }

		bool getDebugNavMesh(const unsigned short polyFlags, GDX::AI::ProtoRecastDebugNavMesh* proto);

	protected:
		virtual bool prepareBuild(class InputGeom* geom);
		virtual bool doBuild();
	};
}
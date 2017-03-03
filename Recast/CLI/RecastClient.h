#pragma once

#include <Recast.h>
#include <DetourNavMesh.h>
#include <DetourNavMeshQuery.h>
#include <DetourNavMeshBuilder.h>
#include <DetourCrowd.h>

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
	private:
		rcConfig m_cfg;

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
		
	public:
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

		PartitionType m_partitionType = PARTITION_WATERSHED;

	public:
		RecastClient();
		~RecastClient();

		bool build(std::string geom_path);
		bool build(class InputGeom* geom);

		BuildContext* getContext() { return m_ctx; }

	private:
		void buildStep1InitConfig();
		bool buildStep2Rasterize();
		void buildStep3FilterWalkable();
		bool buildStep4PartitionWalkableSurface();
		bool buildStep5TraceAndSimplify();
		bool buildStep6BuildPolygons();
		bool buildStep7CreateDetailMesh();
		bool buildStep8CreateContourData();
	};
}
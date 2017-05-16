#pragma once

namespace RecastWrapper
{
	public ref class ManagedRecastSettings
	{
	public:
		float* WorldBoundsMin;
		float* WorldBoundsMax;
		float CellSize = 0.3f;
		float CellHeight = 0.2f;
		float AgentMaxSlope = 45.0f;
		float AgentHeight = 2.0f;
		float AgentMaxClimb = 0.9f;
		float AgentRadius = 0.6f;
		float EdgeMaxLen = 12.0f;
		float EdgeMaxError = 1.3f;
		float RegionMinSize = 8;
		float RegionMergeSize = 20;
		float DetailSampleDist = 6.0f;
		float DetailSampleMaxError = 1.0f;

		bool FilterLowHangingObstacles = true;
		bool FilterLedgeSpans = true;
		bool FilterWalkableLowHeightSpans = true;

		int MaxAgents = 1000;

		int PartitionType = 0; // Watershed

		void Apply(RecastClient* client)
		{
			client->m_worldBoundsMin = WorldBoundsMin;
			client->m_worldBoundsMax = WorldBoundsMax;
			client->m_cellSize = CellSize;
			client->m_cellHeight = CellHeight;
			client->m_agentMaxSlope = AgentMaxSlope;
			client->m_agentHeight = AgentHeight;
			client->m_agentMaxClimb = AgentMaxClimb;
			client->m_agentRadius = AgentRadius;
			client->m_edgeMaxLen = EdgeMaxLen;
			client->m_edgeMaxError = EdgeMaxError;
			client->m_regionMinSize = RegionMinSize;
			client->m_regionMergeSize = RegionMergeSize;
			client->m_detailSampleDist = DetailSampleDist;
			client->m_detailSampleMaxError = DetailSampleMaxError;

			client->m_filterLowHangingObstacles = FilterLowHangingObstacles;
			client->m_filterLedgeSpans = FilterLedgeSpans;
			client->m_filterWalkableLowHeightSpans = FilterWalkableLowHeightSpans;

			client->m_maxAgents = MaxAgents;

			client->m_partitionType = (RecastWrapper::PartitionType)PartitionType;
		}

		void SetWorldBounds(array<float>^ min, array<float>^ max)
		{
			pin_ptr<float> min_start = &min[0];
			pin_ptr<float> max_start = &max[0];
			this->WorldBoundsMin = &min_start[0];
			this->WorldBoundsMax = &max_start[0];
		}
	};
}
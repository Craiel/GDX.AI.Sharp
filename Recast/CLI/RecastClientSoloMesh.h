#pragma once

#include <DetourTileCache.h>
#include "RecastClient.h"

namespace RecastWrapper {
	class RecastClientSoloMesh : public RecastClient
	{
	protected:
		virtual bool prepareBuild();
		virtual bool doBuild();

	public:
		RecastClientSoloMesh();
		~RecastClientSoloMesh();

		bool build(std::string geom_path);

		virtual dtStatus addObstacle(const float* pos, float radius, float height, dtObstacleRef* ref);
		virtual dtStatus addObstacleBox(const float* bmin, const float* bmax, dtObstacleRef* ref);
		virtual dtStatus removeObstacle(dtObstacleRef ref);
		virtual void clearObstacles();

	private:
		void buildStep1InitConfig();
		bool buildStep2Rasterize();
		void buildStep3FilterWalkable();
		bool buildStep4PartitionWalkableSurface();
		bool buildStep5TraceAndSimplify();
		bool buildStep6BuildPolygons();
		bool buildStep7CreateDetailMesh();
		bool buildStep8CreateDetourData();
	};
}

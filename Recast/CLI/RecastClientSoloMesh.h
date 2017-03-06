#pragma once

#include "RecastClient.h"

namespace RecastWrapper {
	class RecastClientSoloMesh : public RecastClient
	{
	protected:
		virtual bool prepareBuild(class InputGeom* geom);
		virtual bool doBuild();

	public:
		RecastClientSoloMesh();
		~RecastClientSoloMesh();

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

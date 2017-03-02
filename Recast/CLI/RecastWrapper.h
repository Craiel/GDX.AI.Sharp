#pragma once

#include "ManagedInputGeom.h"
#include "Singleton.h"
#include <Recast.h>

using namespace System::Runtime::InteropServices;

/*namespace RecastWrapper
{
	public enum class PartitionType
	{
		Watershed,
		Monotone,
		Layers
	};

	public ref class RecastWrapper
	{
	private:
		RecastWrapper() {}
		RecastWrapper(const RecastWrapper%) { throw gcnew System::InvalidOperationException("singleton cannot be copy-constructed"); }
		static RecastWrapper m_instance;

	public:
		static property RecastWrapper^ Instance { RecastWrapper^ get() { return %m_instance; } }
		static property int RC_WALKABLE_AREA_W { int get() { return RC_WALKABLE_AREA; }}

	public:
		void rcCalcGridSizeWrapped(array<float>^ bmin, array<float>^ bmax, float cs, [Out] int% width, [Out] int% height)
		{
			int w;
			int h;
			pin_ptr<float> bmin_start = &bmin[0];
			pin_ptr<float> bmax_start = &bmax[0];
			rcCalcGridSize(bmin_start, bmax_start, cs, &w, &h);
			width = w;
			height = h;
		}

		ManagedRcHeightfield^ rcAllocHeightFieldWrapped()
		{
			rcHeightfield* field = rcAllocHeightfield();
			return gcnew ManagedRcHeightfield(field);
		}

		void rcMarkWalkableTrianglesWrapped(ManagedRcContext^ context, float walkableSlopeAngle, ManagedInputGeom^ inputGeom, array<unsigned char>^ areas)
		{
			InputGeom* geom = inputGeom->GetUnmanaged();
			const float* verts = geom->getMesh()->getVerts();
			const int nverts = geom->getMesh()->getVertCount();
			const int* tris = geom->getMesh()->getTris();
			const int ntris = geom->getMesh()->getTriCount();

			unsigned char* triangle_areas = new unsigned char[ntris];
			rcMarkWalkableTriangles(context->GetUnmanaged(), walkableSlopeAngle, verts, nverts, tris, ntris, triangle_areas);
			for (int i = 0; i < ntris; i++)
			{
				areas[i] = triangle_areas[i];
			}
		}

		void rcMarkWalkableTrianglesWrapped(ManagedRcContext^ context, float walkableSlopeAngle, array<float>^ vertices, int vertexCount, array<int>^ triangles, int triangleCount, array<unsigned char>^ areas)
		{
			pin_ptr<float> vertex_start = &vertices[0];
			pin_ptr<int> triangle_start = &triangles[0];
			unsigned char* triangle_areas = new unsigned char[triangleCount];
			rcMarkWalkableTriangles(context->GetUnmanaged(), walkableSlopeAngle, vertex_start, vertexCount, triangle_start, triangleCount, triangle_areas);
			for(int i = 0; i < triangleCount; i++)
			{
				areas[i] = triangle_areas[i];
			}
		}
		
		bool rcRasterizeTrianglesWrapped(ManagedRcContext^ context, ManagedInputGeom^ inputGeom, array<unsigned char>^ areas, ManagedRcHeightfield^ heightfield, int walkableClimb)
		{
			InputGeom* geom = inputGeom->GetUnmanaged();
			const float* verts = geom->getMesh()->getVerts();
			const int nverts = geom->getMesh()->getVertCount();
			const int* tris = geom->getMesh()->getTris();
			const int ntris = geom->getMesh()->getTriCount();

			pin_ptr<unsigned char> areas_start = &areas[0];

			// rcContext* ctx, const float* verts, const int nv, const int* tris, const unsigned char* areas, const int nt, rcHeightfield& solid, const int flagMergeThr = 1
			return rcRasterizeTriangles(context->GetUnmanaged(), verts, nverts, tris, areas_start, ntris, *heightfield->GetUnmanaged(), walkableClimb);
		}

		bool rcRasterizeTrianglesWrapped(ManagedRcContext^ context, array<float>^ vertices, int vertexCount, array<int>^ triangles, array<unsigned char>^ areas, int triangleCount, ManagedRcHeightfield^ heightfield, int walkableClimb)
		{
			pin_ptr<float> vertex_start = &vertices[0];
			pin_ptr<int> triangle_start = &triangles[0];
			pin_ptr<unsigned char> areas_start = &areas[0];

			// rcContext* ctx, const float* verts, const int nv, const int* tris, const unsigned char* areas, const int nt, rcHeightfield& solid, const int flagMergeThr = 1
			return rcRasterizeTriangles(context->GetUnmanaged(), vertex_start, vertexCount, triangle_start, areas_start, triangleCount, *heightfield->GetUnmanaged(), walkableClimb);
		}

		void rcFilterLowHangingWalkableObstaclesWrapped(ManagedRcContext^ context, int walkableClimb, ManagedRcHeightfield^ heightfield)
		{
			rcFilterLowHangingWalkableObstacles(context->GetUnmanaged(), walkableClimb, *heightfield->GetUnmanaged());
		}

		void rcFilterLedgeSpansWrapped(ManagedRcContext^ context, int walkableHeight, int walkableClimb, ManagedRcHeightfield^ heightfield)
		{
			rcFilterLedgeSpans(context->GetUnmanaged(), walkableHeight, walkableClimb, *heightfield->GetUnmanaged());
		}

		void rcFilterWalkableLowHeightSpansWrapped(ManagedRcContext^ context, int walkableHeight, ManagedRcHeightfield^ heightfield)
		{
			rcFilterWalkableLowHeightSpans(context->GetUnmanaged(), walkableHeight, *heightfield->GetUnmanaged());
		}

		ManagedRcCompactHeightfield^ rcAllocCompactHeightfieldWrapped()
		{
			rcCompactHeightfield* instance = rcAllocCompactHeightfield();
			return gcnew ManagedRcCompactHeightfield(instance);
		}

		bool rcCreateHeightfieldWrapped(ManagedRcContext^ context, ManagedRcHeightfield^ heightfield, int width, int height, array<float>^ bmin, array<float>^ bmax, float cs, float ch)
		{
			pin_ptr<float> bmin_start = &bmin[0];
			pin_ptr<float> bmax_start = &bmax[0];
			return rcCreateHeightfield(context->GetUnmanaged(), *heightfield->GetUnmanaged(), width, height, bmin_start, bmax_start, cs, ch);
		}

		bool rcBuildCompactHeightfieldWrapped(ManagedRcContext^ context, int walkableHeight, int walkableClimb, ManagedRcHeightfield^ heightfield, ManagedRcCompactHeightfield^ compactHeightfield)
		{
			return rcBuildCompactHeightfield(context->GetUnmanaged(), walkableHeight, walkableClimb, *heightfield->GetUnmanaged(), *compactHeightfield->GetUnmanaged());
		}

		bool rcErodeWalkableAreaWrapped(ManagedRcContext^ context, int walkableRadius, ManagedRcCompactHeightfield^ compactHeightfield)
		{
			return rcErodeWalkableArea(context->GetUnmanaged(), walkableRadius, *compactHeightfield->GetUnmanaged());
		}

		void rcMarkAllConvexPolyArea(ManagedRcContext^ context, ManagedInputGeom^ inputGeom, ManagedRcCompactHeightfield^ compactHeightfield)
		{
			const ConvexVolume* vols = inputGeom->GetUnmanaged()->getConvexVolumes();
			for (int i = 0; i < inputGeom->GetUnmanaged()->getConvexVolumeCount(); ++i)
			{
				rcMarkConvexPolyAreaWrapped(context, vols[i], compactHeightfield);
			}
		}

		void rcMarkConvexPolyAreaWrapped(ManagedRcContext^ context, ConvexVolume vol, ManagedRcCompactHeightfield^ compactHeightfield)
		{
			rcMarkConvexPolyArea(context->GetUnmanaged(), vol.verts, vol.nverts, vol.hmin, vol.hmax, (unsigned char)vol.area, *compactHeightfield->GetUnmanaged());
		}

		bool rcBuildDistanceFieldWrapped(ManagedRcContext^ context, ManagedRcCompactHeightfield^ compactHeightfield)
		{
			return rcBuildDistanceField(context->GetUnmanaged(), *compactHeightfield->GetUnmanaged());
		}

		bool rcBuildRegionsWrapped(ManagedRcContext^ context, ManagedRcCompactHeightfield^ compactHeightfield, int borderSize, int minRegionArea, int mergeRegionArea)
		{
			return rcBuildRegions(context->GetUnmanaged(), *compactHeightfield->GetUnmanaged(), borderSize, minRegionArea, mergeRegionArea);
		}

		bool rcBuildRegionsMonotoneWrapped(ManagedRcContext^ context, ManagedRcCompactHeightfield^ compactHeightfield, int borderSize, int minRegionArea, int mergeRegionArea)
		{
			return rcBuildRegionsMonotone(context->GetUnmanaged(), *compactHeightfield->GetUnmanaged(), borderSize, minRegionArea, mergeRegionArea);
		}

		bool rcBuildLayerRegionsWrapped(ManagedRcContext^ context, ManagedRcCompactHeightfield^ compactHeightfield, int borderSize, int minRegionArea)
		{
			return rcBuildLayerRegions(context->GetUnmanaged(), *compactHeightfield->GetUnmanaged(), borderSize, minRegionArea);
		}

		ManagedRcContourSet^ rcAllocContourSetWrapped()
		{
			rcContourSet* instance = rcAllocContourSet();
			return gcnew ManagedRcContourSet(instance);
		}

		bool rcBuildContoursWrapped(ManagedRcContext^ context, ManagedRcCompactHeightfield^ compactHeightfield, float maxSimplicationError, int maxEdgeLength, ManagedRcContourSet^ contourSet)
		{
			return rcBuildContours(context->GetUnmanaged(), *compactHeightfield->GetUnmanaged(), maxSimplicationError, maxEdgeLength, *contourSet->GetUnmanaged());
		}

		ManagedRcPolyMesh^ rcAllocPolyMeshWrapped()
		{
			rcPolyMesh* instance = rcAllocPolyMesh();
			return gcnew ManagedRcPolyMesh(instance);
		}

		bool rcBuildPolyMeshWrapped(ManagedRcContext^ context, ManagedRcContourSet^ contourSet, int maxVertsPerPoly, ManagedRcPolyMesh^ polyMesh)
		{
			return rcBuildPolyMesh(context->GetUnmanaged(), *contourSet->GetUnmanaged(), maxVertsPerPoly, *polyMesh->GetUnmanaged());
		}

		ManagedRcPolyMeshDetail^ rcAllocPolyMeshDetailWrapped()
		{
			rcPolyMeshDetail* instance = rcAllocPolyMeshDetail();
			return gcnew ManagedRcPolyMeshDetail(instance);
		}

		bool rcBuildPolyMeshDetailWrapped(ManagedRcContext^ context, ManagedRcPolyMesh^ polyMesh, ManagedRcCompactHeightfield^ compactHeightfield, float detailSampleDistance, float detailSampleMaxError, ManagedRcPolyMeshDetail^ polyMeshDetail)
		{
			return rcBuildPolyMeshDetail(context->GetUnmanaged(), *polyMesh->GetUnmanaged(), *compactHeightfield->GetUnmanaged(), detailSampleDistance, detailSampleMaxError, *polyMeshDetail->GetUnmanaged());
		}

		void rcFreeHeightFieldWrapped(ManagedRcHeightfield^ heightfield)
		{
			rcFreeHeightField(heightfield->GetUnmanaged());
		}

		void rcFreeCompactHeightfieldWrapped(ManagedRcCompactHeightfield^ compactHeightfield)
		{
			rcFreeCompactHeightfield(compactHeightfield->GetUnmanaged());
		}

		void rcFreeContourSetWrapped(ManagedRcContourSet^ contourSet)
		{
			rcFreeContourSet(contourSet->GetUnmanaged());
		}
	};
}*/
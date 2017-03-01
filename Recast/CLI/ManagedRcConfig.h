#pragma once

#include <Recast.h>

namespace RecastWrapper {

	public ref class ManagedRcConfig
	{
	public:

		int width;
		int height;

		int tileSize;
		int borderSize;

		float cs;
		float ch;

		array<float>^ bmin;
		array<float>^ bmax;

		float walkableSlopeAngle;
		int walkableHeight;
		int walkableClimb;
		int walkableRadius;

		int maxEdgeLen;
		float maxSimplificationError;

		int minRegionArea;
		int mergeRegionArea;

		int maxVertsPerPoly;
		float detailSampleDist;
		float detailSampleMaxError;

	public:
		ManagedRcConfig() { bmin = gcnew array<float>(3); bmax = gcnew array<float>(3); }
		~ManagedRcConfig() { this->!ManagedRcConfig(); }
		!ManagedRcConfig() { }

		rcConfig ToConfig()
		{
			rcConfig result;
			result.width = width;
			result.height = height;
			result.tileSize = tileSize;
			result.borderSize = borderSize;
			result.cs = cs;
			result.ch = ch;
			result.bmin[0] = bmin[0];
			result.bmin[1] = bmin[1];
			result.bmin[2] = bmin[2];
			result.bmax[0] = bmax[0];
			result.bmax[1] = bmax[1];
			result.bmax[2] = bmax[2];
			result.walkableSlopeAngle = walkableSlopeAngle;
			result.walkableHeight = walkableHeight;
			result.walkableClimb = walkableClimb;
			result.walkableRadius = walkableRadius;
			result.maxEdgeLen = maxEdgeLen;
			result.maxSimplificationError = maxSimplificationError;
			result.minRegionArea = minRegionArea;
			result.mergeRegionArea = mergeRegionArea;
			result.maxVertsPerPoly = maxVertsPerPoly;
			result.detailSampleDist = detailSampleDist;
			result.detailSampleMaxError = detailSampleMaxError;

			return result;
		}
	};

}
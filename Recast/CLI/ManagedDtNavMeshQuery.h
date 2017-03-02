#pragma once

#include "ManagedDtCrowd.h"
#include "ManagedDtQueryFilter.h"
#include <DetourNavMeshQuery.h>

#include <cstdlib>

using namespace System::Runtime::InteropServices;

static float frand()
{
	//	return ((float)(rand() & 0xffff)/(float)0xffff);
	return (float)rand() / (float)RAND_MAX;
}

namespace RecastWrapper
{
	public ref class ManagedDtNavMeshQuery
	{
	private:
		dtNavMeshQuery* unmanaged;
	internal:
		ManagedDtNavMeshQuery(dtNavMeshQuery* instance) { unmanaged = instance; }
		dtNavMeshQuery* GetUnmanaged() { return unmanaged; }
				
	public:
		unsigned int Init(ManagedDtNavMesh^ navMesh, int maxNodes)
		{
			return unmanaged->init(navMesh->GetUnmanaged(), maxNodes);
		}

		unsigned int FindRandomPointAroundCircle(unsigned int% startRef, array<float>^ centerPosition, float maxRadius, ManagedDtCrowd^ crowd, [Out] unsigned int% randomRef, [Out] array<float>^% randomPoint)
		{
			float rn1 = frand();
			float rn2 = frand();

			dtPolyRef startRefLocal = startRef;
			dtPolyRef randomRefLocal;
			float pt[3];
			pin_ptr<float> center_start = &centerPosition[0];
			const dtQueryFilter* filter = crowd->GetUnmanaged()->getFilter(0);
			dtStatus status = unmanaged->findRandomPointAroundCircle(startRefLocal, center_start, maxRadius, filter, frand, &randomRefLocal, pt);
			startRef = startRefLocal;
			randomRef = randomRefLocal;
			randomPoint = gcnew array<float>(3);
			randomPoint[0] = pt[0];
			randomPoint[1] = pt[1];
			randomPoint[2] = pt[2];

			return status;
		}

		unsigned int FindNearestPoly(array<float>^ centerPosition, array<float>^ extends, ManagedDtCrowd^ crowd, [Out] unsigned int% nearestRef, [Out] array<float>^% nearestPoint)
		{
			pin_ptr<float> center_start = &centerPosition[0];
			pin_ptr<float> extends_start = &extends[0];
			dtPolyRef refLocal;
			float pt[3];
			const dtQueryFilter* filter = crowd->GetUnmanaged()->getFilter(0);
			dtStatus status = unmanaged->findNearestPoly(center_start, extends_start, filter, &refLocal, pt);
			if (refLocal != 0) {
				nearestRef = refLocal;
				nearestPoint = gcnew array<float>(3);
				nearestPoint[0] = pt[0];
				nearestPoint[1] = pt[1];
				nearestPoint[2] = pt[2];
			}

			return status;
		}

	};
}
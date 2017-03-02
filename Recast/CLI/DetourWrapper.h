#pragma once

#include "ManagedDtNavMesh.h"
#include "ManagedDtNavMeshCreateParams.h"
#include "ManagedDtNavMeshQuery.h"
#include "ManagedDtCrowd.h"
#include "DetourCrowd.h"
#include "DetourNavMesh.h"
#include "DetourNavMeshQuery.h"
#include "DetourNavMeshBuilder.h"

using namespace System::Runtime::InteropServices;
using namespace System::Collections;
using namespace System::Collections::Generic;

namespace RecastWrapper
{
	public ref class DetourWrapper
	{
	private:
		DetourWrapper() {}
		DetourWrapper(const DetourWrapper%) { throw gcnew System::InvalidOperationException("singleton cannot be copy-constructed"); }
		static DetourWrapper m_instance;

	public:
		static property DetourWrapper^ Instance { DetourWrapper^ get() { return %m_instance; } }

	public:
		bool dtCreateNavMeshDataWrapped(ManagedDtNavMeshCreateParams^ params, [Out] array<unsigned char>^% navData)
		{
			int size;
			unsigned char* data = 0;
			bool result = dtCreateNavMeshData(&params->GetUnmanaged(), &data, &size);
			navData = gcnew array<unsigned char>(size);
			for (int i = 0; i < size; i++)
			{
				navData[i] = data[i];
			}

			return result;
		}

		ManagedDtNavMesh^ dtAllocNavMeshWrapped()
		{
			dtNavMesh* instance = dtAllocNavMesh();
			return gcnew ManagedDtNavMesh(instance);
		}

		void dtFreeWrapped(ManagedDtNavMesh^ navMesh)
		{
			dtFree(navMesh->GetUnmanaged());
		}

		bool dtStatusFailedWrapped(unsigned int status)
		{
			return dtStatusFailed(status);
		}

		ManagedDtNavMeshQuery^ dtAllocNavMeshQueryWrapped()
		{
			dtNavMeshQuery* instance = dtAllocNavMeshQuery();
			return gcnew ManagedDtNavMeshQuery(instance);
		}

		ManagedDtCrowd^ dtAllocCrowdWrapped()
		{
			dtCrowd* instance = dtAllocCrowd();
			return gcnew ManagedDtCrowd(instance);
		}

		List<array<float>^>^ GetNavMeshDebugData(ManagedDtNavMesh^ managed, const unsigned short polyFlags)
		{
			List<array<float>^>^ result = gcnew List<array<float>^>();

			const dtNavMesh& mesh = *managed->GetUnmanaged();

			for (int i = 0; i < mesh.getMaxTiles(); ++i)
			{
				const dtMeshTile* tile = mesh.getTile(i);
				if (!tile->header) continue;
				dtPolyRef base = mesh.getPolyRefBase(tile);

				for (int j = 0; j < tile->header->polyCount; ++j)
				{
					const dtPoly* p = &tile->polys[j];
					if ((p->flags & polyFlags) == 0) continue;
					GetNavMeshDebugPolyData(result, mesh, base | (dtPolyRef)j);
				}
			}

			return result;
		}

		void GetNavMeshDebugPolyData(List<array<float>^>^ target, const dtNavMesh& mesh, dtPolyRef ref)
		{
			const dtMeshTile* tile = 0;
			const dtPoly* poly = 0;
			if (dtStatusFailed(mesh.getTileAndPolyByRef(ref, &tile, &poly)))
				return;

			const unsigned int ip = (unsigned int)(poly - tile->polys);
			if (poly->getType() == DT_POLYTYPE_OFFMESH_CONNECTION)
			{
				// TODO
				return;
			}

			const dtPolyDetail* pd = &tile->detailMeshes[ip];

			array<array<float>^>^ result = gcnew array<array<float>^>(pd->triCount);
			for (int i = 0; i < pd->triCount; ++i)
			{
				const unsigned char* t = &tile->detailTris[(pd->triBase + i) * 4];

				for (int j = 0; j < 3; ++j)
				{
					const float* pos;
					if (t[j] < poly->vertCount)
					{
						pos = &tile->verts[poly->verts[t[j]] * 3];
					}
					else
					{
						pos = &tile->detailVerts[(pd->vertBase + t[j] - poly->vertCount) * 3];
					}

					array<float>^ vert = gcnew array<float>(3);
					vert[0] = pos[0];
					vert[1] = pos[1];
					vert[2] = pos[2];
					target->Add(vert);
				}
			}
		}
	};
}
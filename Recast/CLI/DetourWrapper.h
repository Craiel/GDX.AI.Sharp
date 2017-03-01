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
	};
}
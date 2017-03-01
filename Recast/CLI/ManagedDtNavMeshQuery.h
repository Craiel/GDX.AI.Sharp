#pragma once

#include <DetourNavMeshQuery.h>

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
	};
}
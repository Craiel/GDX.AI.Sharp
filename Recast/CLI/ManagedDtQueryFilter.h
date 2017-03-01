#pragma once

#include <DetourNavMeshQuery.h>

namespace RecastWrapper
{
	public ref class ManagedDtQueryFilter
	{
	private:
		dtQueryFilter* unmanaged;
	internal:
		ManagedDtQueryFilter(dtQueryFilter* instance) { unmanaged = instance; }
		dtQueryFilter* GetUnmanaged() { return unmanaged; }
	};
}

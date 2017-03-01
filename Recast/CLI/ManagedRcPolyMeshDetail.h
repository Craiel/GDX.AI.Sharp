#pragma once

#include <Recast.h>

namespace RecastWrapper
{
	public ref class ManagedRcPolyMeshDetail
	{
	private:
		rcPolyMeshDetail* unmanaged;

	internal:
		ManagedRcPolyMeshDetail(rcPolyMeshDetail* instance) { unmanaged = instance; }
		rcPolyMeshDetail* GetUnmanaged() { return unmanaged; }
	};
}
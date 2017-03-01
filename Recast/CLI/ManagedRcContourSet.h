#pragma once

#include <Recast.h>

namespace RecastWrapper
{
	public ref class ManagedRcContourSet
	{
	private:
		rcContourSet* unmanaged;
	internal:
		ManagedRcContourSet(rcContourSet* instance) { unmanaged = instance; }
		rcContourSet* GetUnmanaged() { return unmanaged; }
	};
}
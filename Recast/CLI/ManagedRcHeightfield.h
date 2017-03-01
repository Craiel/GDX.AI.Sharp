#pragma once

#include <Recast.h>

namespace RecastWrapper
{
	public ref class ManagedRcHeightfield
	{
	private:
		rcHeightfield* unmanaged;

	internal:
		ManagedRcHeightfield(rcHeightfield* instance) { unmanaged = instance; }
		rcHeightfield* GetUnmanaged() { return unmanaged; }
	};
}
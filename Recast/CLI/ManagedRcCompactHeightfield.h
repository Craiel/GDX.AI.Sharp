#pragma once

#include <Recast.h>

namespace RecastWrapper
{
	public ref class ManagedRcCompactHeightfield
	{
	private:
		rcCompactHeightfield* unmanaged;
	internal:
		ManagedRcCompactHeightfield(rcCompactHeightfield* instance) { unmanaged = instance; }
		rcCompactHeightfield* GetUnmanaged() { return unmanaged; }
	};
}
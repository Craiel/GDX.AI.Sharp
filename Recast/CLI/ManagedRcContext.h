#pragma once

#include <Recast.h>

namespace RecastWrapper
{
	public ref class ManagedRcContext
	{
	protected:
		rcContext* unmanaged;
	public:
		ManagedRcContext() { unmanaged = new rcContext(); }
		ManagedRcContext(rcContext* instance) { unmanaged = instance; }
	internal:
		rcContext* GetUnmanaged() { return unmanaged; }
	};
}

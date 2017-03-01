#pragma once

#include <DetourCrowd.h>

namespace RecastWrapper
{
	public ref class ManagedDtCrowd
	{
	private:
		dtCrowd* unmanaged;
	internal:
		ManagedDtCrowd(dtCrowd* instance) { unmanaged = instance; }
		dtCrowd* GetUnmanaged() { return unmanaged; }
	};
}
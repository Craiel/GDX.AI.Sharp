#pragma once

#include <DetourNavMesh.h>

namespace RecastWrapper
{
	public ref class ManagedDtNavMesh
	{
	private:
		dtNavMesh* unmanaged;
	internal:
		ManagedDtNavMesh(dtNavMesh* instance) { unmanaged = instance; }
		dtNavMesh* GetUnmanaged() { return unmanaged; }

	public:
		unsigned int Init(array<unsigned char>^ navData, int navDataSize, int flags)
		{
			dtStatus status;
			pin_ptr<unsigned char> navData_start = &navData[0];
			status = unmanaged->init(navData_start, navDataSize, flags);
			return status;
		}
	};
}
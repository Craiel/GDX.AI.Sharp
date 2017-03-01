#pragma once

#include <DetourCrowd.h>

namespace RecastWrapper
{
	public ref class ManagedDtCrowdAgent
	{
	private:
		const dtCrowdAgent* unmanaged;
	internal:
		ManagedDtCrowdAgent(const dtCrowdAgent* instance) { unmanaged = instance; }
		const dtCrowdAgent* GetUnmanaged() { return unmanaged; }
	public:
		array<float>^ GetPosition()
		{
			array<float>^ result = gcnew array<float>(3);
			result[0] = unmanaged->npos[0];
			result[1] = unmanaged->npos[1];
			result[2] = unmanaged->npos[2];
			return result;
		}

		property unsigned char TargetState{ unsigned char get() { return unmanaged->targetState; } }
	};
}

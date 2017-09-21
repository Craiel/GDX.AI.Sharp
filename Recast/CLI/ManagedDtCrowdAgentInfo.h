/*#pragma once

#include <DetourCrowd.h>

namespace RecastWrapper {

	public ref class ManagedDtCrowdAgentInfo
	{
	public:
		array<float>^ npos;
		array<float>^ vel;

		unsigned char targetState;
		array<float>^ targetPos;

	public:
		ManagedDtCrowdAgentInfo()
		{
			npos = gcnew array<float>(3); 
			vel = gcnew array<float>(3); 
			targetPos = gcnew array<float>(3);
		}

		~ManagedDtCrowdAgentInfo() { this->!ManagedDtCrowdAgentInfo(); }
		!ManagedDtCrowdAgentInfo() { }
	};

}*/
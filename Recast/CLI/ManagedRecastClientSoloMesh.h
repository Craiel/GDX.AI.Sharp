#pragma once

#include <msclr\marshal_cppstd.h>

#include "RecastClientTiled.h"
#include "RecastClientSoloMesh.h"
#include "ManagedRecastSettings.h"
#include "ManagedDtCrowdAgentInfo.h"
#include "ManagedDtCrowdAgentParams.h"
#include "ManagedRecastClient.h"

using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

namespace RecastWrapper
{
	public ref class ManagedRecastClientSoloMesh : ManagedRecastClient
	{
	private:
		RecastClientSoloMesh* unmanagedTyped;

	public:
		ManagedRecastClientSoloMesh(ManagedRecastSettings^ settings)
		{
			unmanagedTyped = new RecastClientSoloMesh();
			unmanaged = unmanagedTyped;
			settings->Apply(unmanaged);
		}

		~ManagedRecastClientSoloMesh() { }
	public:
		bool LoadObj(String^ path) {
			std::string unmanagedPath = msclr::interop::marshal_as<std::string>(path);
			return unmanagedTyped->build(unmanagedPath);
		}
	};
}
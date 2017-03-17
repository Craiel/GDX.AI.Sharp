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
	public ref class ManagedRecastClientTiled : ManagedRecastClient
	{
	private:
		RecastClientTiled* unmanagedTyped;

	public:
		ManagedRecastClientTiled(ManagedRecastSettings^ settings)
		{
			unmanagedTyped = new RecastClientTiled();
			unmanaged = unmanagedTyped;
			settings->Apply(unmanaged);
		}

		~ManagedRecastClientTiled() { }
	public:
		bool Load(array<byte>^ data)
		{
			GOOGLE_PROTOBUF_VERIFY_VERSION;

			pin_ptr<byte> data_array_start = &data[0];

			GDX::AI::ProtoRecastTiledNavMesh* proto = new GDX::AI::ProtoRecastTiledNavMesh();
			if (!proto->ParseFromArray(data_array_start, data->Length))
			{
				return false;
			}

			return unmanagedTyped->Load(proto);
		}

		bool Save([Out] array<byte>^% data)
		{
			GOOGLE_PROTOBUF_VERIFY_VERSION;

			GDX::AI::ProtoRecastTiledNavMesh* proto = new GDX::AI::ProtoRecastTiledNavMesh();
			if (!unmanagedTyped->Save(proto))
			{
				delete proto;
				return false;
			}

			int size = proto->ByteSize();
			void *buffer = malloc(size);
			proto->SerializeToArray(buffer, size);

			data = gcnew array<byte>(size);
			pin_ptr<byte> data_array_start = &data[0];
			memcpy(data_array_start, buffer, size);

			// clean up
			delete proto;
			free(buffer);
			google::protobuf::ShutdownProtobufLibrary();

			return true;
		}
	};
}
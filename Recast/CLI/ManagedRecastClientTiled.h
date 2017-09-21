/*#pragma once

#include <msclr\marshal_cppstd.h>

#include "RecastClientTiled.h"
#include "RecastClientSoloMesh.h"
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
		ManagedRecastClientTiled()
		{
			unmanagedTyped = new RecastClientTiled();
			unmanaged = unmanagedTyped;
		}

		~ManagedRecastClientTiled() { }
	public:
		bool GetDebugNavMesh([Out] array<byte>^% data)
		{
			GDX::AI::ProtoRecastDebugNavMesh* proto = new GDX::AI::ProtoRecastDebugNavMesh();
			if (!unmanagedTyped->getDebugNavMesh(POLYFLAGS_DISABLED, proto))
			{
				delete proto;
				return false;
			}

			data = gcnew array<byte>(proto->ByteSize());
			pin_ptr<byte> data_array_start = &data[0];
			proto->SerializeToArray(data_array_start, proto->ByteSize());

			// clean up
			delete proto;
			google::protobuf::ShutdownProtobufLibrary();

			return true;
		}

		bool Generate(String^ path)
		{
			std::string unmanagedPath = msclr::interop::marshal_as<std::string>(path);
			return unmanagedTyped->generate(unmanagedPath);
		}

		bool Load(array<byte>^ data)
		{
			GOOGLE_PROTOBUF_VERIFY_VERSION;

			pin_ptr<byte> data_array_start = &data[0];

			GDX::AI::ProtoRecastTiledNavMesh* proto = new GDX::AI::ProtoRecastTiledNavMesh();
			if (!proto->ParseFromArray(data_array_start, data->Length))
			{
				return false;
			}

			return unmanagedTyped->load(proto);
		}

		bool Save([Out] array<byte>^% data)
		{
			GOOGLE_PROTOBUF_VERIFY_VERSION;

			GDX::AI::ProtoRecastTiledNavMesh* proto = new GDX::AI::ProtoRecastTiledNavMesh();
			if (!unmanagedTyped->save(proto))
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
}*/
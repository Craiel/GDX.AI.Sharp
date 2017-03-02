#pragma once

#include "ManagedRcContext.h"
#include "InputGeom.h"
#include <string>
#include <msclr\marshal_cppstd.h>

using System::String;

namespace RecastWrapper
{
	public ref class ManagedInputGeom
	{
	private:
		InputGeom* unmanaged;
	public:
		ManagedInputGeom(InputGeom* instance) { unmanaged = instance; }
		ManagedInputGeom(ManagedRcContext^ context, String^ path)
		{
			unmanaged = new InputGeom();

			std::string unmanagedPath = msclr::interop::marshal_as<std::string>(path);
			unmanaged->load(context->GetUnmanaged(), unmanagedPath);
		}
	internal:
		InputGeom* GetUnmanaged() { return unmanaged; }

	public:
		array<float>^ GetNavMeshBoundsMin()
		{
			const float* min = unmanaged->getNavMeshBoundsMin();
			array<float>^ result = gcnew array<float>(3);
			result[0] = min[0];
			result[1] = min[1];
			result[2] = min[2];
			return result;
		}

		array<float>^ GetNavMeshBoundsMax()
		{
			const float* max = unmanaged->getNavMeshBoundsMax();
			array<float>^ result = gcnew array<float>(3);
			result[0] = max[0];
			result[1] = max[1];
			result[2] = max[2];
			return result;
		}

		array<array<float>^>^ GetVertices()
		{
			const float* verts = unmanaged->getMesh()->getVerts();
			int vertCount = unmanaged->getMesh()->getVertCount();
			array<array<float>^>^ result = gcnew array<array<float>^>(vertCount);
			int index = 0;
			for(int i = 0; i < vertCount; i++)
			{
				array<float>^ vert = gcnew array<float>(3);
				vert[0] = verts[index++];
				vert[1] = verts[index++];
				vert[2] = verts[index++];
				result[i] = vert;
			}

			return result;
		}

		array<array<int>^>^ GetTris()
		{
			const int* tris = unmanaged->getMesh()->getTris();
			int triCount = unmanaged->getMesh()->getTriCount();
			array<array<int>^>^ result = gcnew array<array<int>^>(triCount);
			int index = 0;
			for (int i = 0; i < triCount; i++)
			{
				array<int>^ tri = gcnew array<int>(3);
				tri[0] = tris[index++];
				tri[1] = tris[index++];
				tri[2] = tris[index++];
				result[i] = tri;
			}

			return result;
		}

		int GetTriCount()
		{
			return unmanaged->getMesh()->getTriCount();
		}

		int GetVertCount()
		{
			return unmanaged->getMesh()->getVertCount();
		}
	};
}
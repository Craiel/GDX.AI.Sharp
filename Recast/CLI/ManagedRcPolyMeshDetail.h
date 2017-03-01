#pragma once

#include <Recast.h>

namespace RecastWrapper
{
	public ref class ManagedRcPolyMeshDetail
	{
	private:
		rcPolyMeshDetail* unmanaged;

	internal:
		ManagedRcPolyMeshDetail(rcPolyMeshDetail* instance) { unmanaged = instance; }
		rcPolyMeshDetail* GetUnmanaged() { return unmanaged; }
	public:
		array<array<float>^>^ GetVertices()
		{
			array<array<float>^>^ result = gcnew array<array<float>^>(unmanaged->nverts);
			int index = 0;
			for(int i = 0; i < unmanaged->nverts; i++)
			{
				array<float>^ vert = gcnew array<float>(3);
				vert[0] = unmanaged->verts[index++];
				vert[1] = unmanaged->verts[index++];
				vert[2] = unmanaged->verts[index++];
				result[i] = vert;
			}
			
			return result;
		}

		array<array<unsigned char>^>^ GetTriangles()
		{
			array<array<unsigned char>^>^ result = gcnew array<array<unsigned char>^>(unmanaged->ntris);
			int index = 0;
			for (int i = 0; i < unmanaged->ntris; i++)
			{
				array<unsigned char>^ tri = gcnew array<unsigned char>(3);
				tri[0] = unmanaged->tris[index++];
				tri[1] = unmanaged->tris[index++];
				tri[2] = unmanaged->tris[index++];
				result[i] = tri;
			}

			return result;
		}
	};
}
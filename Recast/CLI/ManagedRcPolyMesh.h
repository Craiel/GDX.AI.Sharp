#pragma once

#include <Recast.h>

namespace RecastWrapper
{
	public ref class ManagedRcPolyMesh
	{
	private:
		rcPolyMesh* unmanaged;

	internal:
		ManagedRcPolyMesh(rcPolyMesh* instance) { unmanaged = instance; }
		rcPolyMesh* GetUnmanaged() { return unmanaged; }
		
	public:
		property int VertexCount { int get() { return unmanaged->nverts; } }

		property int PolygonCount {	int get() { return unmanaged->npolys; } }
	};
}
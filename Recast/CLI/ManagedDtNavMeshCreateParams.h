#pragma once

#include "ManagedRcPolyMesh.h"
#include "ManagedRcPolyMeshDetail.h"
#include "ManagedInputGeom.h"
#include <DetourNavMeshBuilder.h>
#include <string.h>

namespace RecastWrapper
{
	public ref class ManagedDtNavMeshCreateParams
	{
	public:
		ManagedRcPolyMesh^ Poly;
		ManagedRcPolyMeshDetail^ PolyDetail;
		ManagedInputGeom^ Geom;

		unsigned int userId;	///< The user defined id of the tile.
		int tileX;				///< The tile's x-grid location within the multi-tile destination mesh. (Along the x-axis.)
		int tileY;				///< The tile's y-grid location within the multi-tile desitation mesh. (Along the z-axis.)
		int tileLayer;			///< The tile's layer within the layered destination mesh. [Limit: >= 0] (Along the y-axis.)
		
		float walkableHeight;	///< The agent height. [Unit: wu]
		float walkableRadius;	///< The agent radius. [Unit: wu]
		float walkableClimb;	///< The agent maximum traversable ledge. (Up/Down) [Unit: wu]
		float cs;				///< The xz-plane cell size of the polygon mesh. [Limit: > 0] [Unit: wu]
		float ch;				///< The y-axis cell height of the polygon mesh. [Limit: > 0] [Unit: wu]

								/// True if a bounding volume tree should be built for the tile.
								/// @note The BVTree is not normally needed for layered navigation meshes.
		bool buildBvTree;

	public:
		ManagedDtNavMeshCreateParams() { }

	public:
		dtNavMeshCreateParams GetUnmanaged()
		{
			dtNavMeshCreateParams params;
			memset(&params, 0, sizeof(params));

			rcPolyMesh* polyMesh = Poly->GetUnmanaged();
			params.verts = polyMesh->verts;
			params.vertCount = polyMesh->nverts;

			params.polys = polyMesh->polys;
			params.polyFlags = polyMesh->flags;
			params.polyAreas = polyMesh->areas;
			params.polyCount = polyMesh->npolys;

			params.nvp = polyMesh->nvp;

			rcPolyMeshDetail* polyDetailMesh = PolyDetail->GetUnmanaged();
			params.detailMeshes = polyDetailMesh->meshes;
			params.detailVerts = polyDetailMesh->verts;
			params.detailVertsCount = polyDetailMesh->nverts;
			params.detailTris = polyDetailMesh->tris;
			params.detailTriCount = polyDetailMesh->ntris;

			InputGeom* geom = Geom->GetUnmanaged();
			params.offMeshConVerts = geom->getOffMeshConnectionVerts();
			params.offMeshConRad = geom->getOffMeshConnectionRads();
			params.offMeshConFlags = geom->getOffMeshConnectionFlags();
			params.offMeshConAreas = geom->getOffMeshConnectionAreas();
			params.offMeshConDir = geom->getOffMeshConnectionDirs();
			params.offMeshConUserID = geom->getOffMeshConnectionId();
			params.offMeshConCount = geom->getOffMeshConnectionCount();

			params.userId = userId;
			params.tileX = tileX;
			params.tileY = tileY;
			params.tileLayer = tileLayer;

			rcVcopy(params.bmin, polyMesh->bmin);
			rcVcopy(params.bmax, polyMesh->bmax);

			params.walkableHeight = walkableHeight;
			params.walkableRadius = walkableRadius;
			params.walkableClimb = walkableClimb;
			params.cs = cs;
			params.ch = ch;
			params.buildBvTree = buildBvTree;

			return params;
		}
	};
}

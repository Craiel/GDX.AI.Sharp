namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using Enums;

    using RecastWrapper;

    public static class DetourNavMeshBuilding
    {
        public static void Build(DetourNavMeshBuildContext context, NavMeshBuildContext navMeshContext)
        {
            PrepareContext(context, navMeshContext);

            // Step 1
            CreateWrappedMeshData(context);

            // Step 2
            CreateNavMesh(context);

            // Step 3
            CreateNavMeshQuery(context);

            // Step 4
            CreateNavMeshCrowd(context);
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private static void PrepareContext(DetourNavMeshBuildContext context, NavMeshBuildContext navMeshContext)
        {
            context.Params.SetNavMesh(navMeshContext.PolyMesh, navMeshContext.PolyMeshDetail);

            // Copy over some other configuration options
            context.Params.walkableHeight = navMeshContext.Config.walkableHeight;
            context.Params.walkableRadius = navMeshContext.Config.walkableRadius;
            context.Params.walkableClimb = navMeshContext.Config.walkableClimb;
            context.Params.cs = navMeshContext.Config.cs;
            context.Params.ch = navMeshContext.Config.ch;
            context.Params.buildBvTree = true;
        }

        private static void CreateWrappedMeshData(DetourNavMeshBuildContext context)
        {
            byte[] navData;
            if (!DetourWrapper.Instance.dtCreateNavMeshDataWrapped(context.Params, out navData))
            {
                throw new NavMeshBuildException("Failed to create Detour NavMesh");
            }

            context.NavMeshData = navData;
        }

        private static void CreateNavMesh(DetourNavMeshBuildContext context)
        {
            context.NavMesh = DetourWrapper.Instance.dtAllocNavMeshWrapped();
            uint status = context.NavMesh.Init(context.NavMeshData, context.NavMeshData.Length, (int)dtTileFlagsWrapped.DT_TILE_FREE_DATA);
            if (DetourWrapper.Instance.dtStatusFailedWrapped(status))
            {
                DetourWrapper.Instance.dtFreeWrapped(context.NavMesh);
                context.NavMesh = null;
                throw new NavMeshBuildException("Failed to init Detour NavMesh");
            }
        }

        private static void CreateNavMeshQuery(DetourNavMeshBuildContext context)
        {
            context.Query = DetourWrapper.Instance.dtAllocNavMeshQueryWrapped();
            uint status = context.Query.Init(context.NavMesh, 2048);
            if (DetourWrapper.Instance.dtStatusFailedWrapped(status))
            {
                DetourWrapper.Instance.dtFreeWrapped(context.NavMesh);
                context.NavMesh = null;
                throw new NavMeshBuildException("Failed to init Detour navmesh query");
            }
        }

        private static void CreateNavMeshCrowd(DetourNavMeshBuildContext context)
        {
            context.Crowd = DetourWrapper.Instance.dtAllocCrowdWrapped();
        }
    }
}

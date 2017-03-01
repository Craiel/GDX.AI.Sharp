namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using CarbonCore.Utils.IO;

    using Logic;

    public static class Recast
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static RecastRuntime BuildSoloMesh(CarbonFile file, NavMeshBuildContext buildContext = null, DetourNavMeshBuildContext detourContext = null)
        {
            buildContext = buildContext ?? new NavMeshBuildContext();
            NavMeshBuilding.Build(buildContext, file);

            detourContext = detourContext ?? new DetourNavMeshBuildContext();
            DetourNavMeshBuilding.Build(detourContext, buildContext);

            var runtime = new RecastRuntime();
            runtime.NavMesh = new RecastNavMesh(runtime, buildContext.PolyMesh, buildContext.PolyMeshDetail);
            runtime.DetourNavMesh = new DetourNavMesh(runtime, detourContext.NavMesh);
            runtime.Query = new DetourQuery(runtime, detourContext.Query);
            runtime.Crowd = new DetourCrowd(runtime, detourContext.Crowd);

            return runtime;
        }
    }
}

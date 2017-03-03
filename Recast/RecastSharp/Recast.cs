namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using CarbonCore.Utils.Diagnostics;
    using CarbonCore.Utils.IO;

   /* using Logic;

    using RecastWrapper;

    public static class Recast
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static RecastRuntime BuildSoloMesh(CarbonFile file, NavMeshBuildContext buildContext = null, DetourNavMeshBuildContext detourContext = null)
        {
            var runtime = new RecastRuntime();

            buildContext = buildContext ?? new NavMeshBuildContext();
            NavMeshBuilding.Build(buildContext, file);

            // Wrap and assign the result
            runtime.NavMesh = new RecastNavMesh(runtime, buildContext.PolyMesh, buildContext.PolyMeshDetail);

            // TODO: we might have to allow customization of the flags
            runtime.NavMesh.SetDefaultPolyFlags();

            detourContext = detourContext ?? new DetourNavMeshBuildContext();
            DetourNavMeshBuilding.Build(detourContext, buildContext);

            runtime.DetourNavMesh = new DetourNavMesh(runtime, detourContext.NavMesh);
            runtime.Query = new DetourQuery(runtime, detourContext.Query);
            runtime.Crowd = new DetourCrowd(runtime, detourContext.Crowd);

#if DEBUG
            buildContext.ManagedContext.StopTimer();
            buildContext.ManagedContext.LogBuildTimes();

            Diagnostic.Info("Printing Context Log:");
            for (var i = 0; i < buildContext.ManagedContext.getLogCount(); i++)
            {
                string msg = buildContext.ManagedContext.getLogText(i);
                Diagnostic.Info(msg);
            }

            Diagnostic.Info(">> Polymesh {0} vertices, {1} polygons", buildContext.PolyMesh.VertexCount, buildContext.PolyMesh.PolygonCount);
#endif
            
            return runtime;
        }
    }*/
}

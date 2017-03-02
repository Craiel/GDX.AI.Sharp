namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using System;

    using CarbonCore.Utils.Diagnostics;
    using CarbonCore.Utils.IO;

    using RecastWrapper;

    public static class NavMeshBuilding
    {
        public static void Build(NavMeshBuildContext context, CarbonFile sourceFile)
        {
            // Step 0+1
            LoadModel(context, sourceFile);
            InitializeBoundsAndConfig(context);

            context.ManagedContext.ResetTimers();
            context.ManagedContext.StartTimer();

#if DEBUG
            Diagnostic.Info("Building navigation:");
            Diagnostic.Info($" - {context.Config.width} x {context.Config.height} cells");
            Diagnostic.Info($" - {Math.Round(context.InputGeom.GetVertCount() / 100.0f) / 10}K verts, {Math.Round(context.InputGeom.GetTriCount() / 100.0f) / 10}K tris");
#endif

            // Step 2
            RasterizeInputPolygons(context);

            // Step 3
            FilterWalkableSurface(context);

            // Step 4
            PartitionSurface(context);

            // Step 5
            TraceAndSimplifyContours(context);

            // Step 6
            BuildPolyMesh(context);

            // Step 7 - Create detail mesh which allows to access approximate height on each polygon
            BuildPolyDetailMesh(context);
        }

        private static void LoadModel(NavMeshBuildContext context, CarbonFile sourceFile)
        {
            if (sourceFile == null || !sourceFile.Exists)
            {
                throw new NavMeshBuildException("Source file not defined or not found");
            }

            context.InputGeom = new ManagedInputGeom(context.ManagedContext, sourceFile.GetPath());
        }

        private static void InitializeBoundsAndConfig(NavMeshBuildContext context)
        {
            context.Config.bmin = context.InputGeom.GetNavMeshBoundsMin();
            context.Config.bmax = context.InputGeom.GetNavMeshBoundsMax();
            
            int width;
            int height;
            RecastWrapper.Instance.rcCalcGridSizeWrapped(context.Config.bmin, context.Config.bmax, context.Config.cs, out width, out height);
            context.Config.width = width;
            context.Config.height = height;
        }

        private static void RasterizeInputPolygons(NavMeshBuildContext context)
        {
            context.Heightfield = RecastWrapper.Instance.rcAllocHeightFieldWrapped();
            if (
                !RecastWrapper.Instance.rcCreateHeightfieldWrapped(
                    context.ManagedContext,
                    context.Heightfield,
                    context.Config.width,
                    context.Config.height,
                    context.Config.bmin,
                    context.Config.bmax,
                    context.Config.cs,
                    context.Config.ch))
            {
                throw new NavMeshBuildException("Could not create heightfield");
            }
            
            // Find triangles which are walkable based on their slope and rasterize them.
            // If your input data is multiple meshes, you can transform them here, calculate
            // the are type for each of the meshes and rasterize them.
            context.Areas = new byte[context.InputGeom.GetTriCount()];
            RecastWrapper.Instance.rcMarkWalkableTrianglesWrapped(
                context.ManagedContext,
                context.Config.walkableSlopeAngle,
                context.InputGeom,
                context.Areas);
            if (
                !RecastWrapper.Instance.rcRasterizeTrianglesWrapped(
                    context.ManagedContext,
                    context.InputGeom,
                    context.Areas,
                    context.Heightfield,
                    context.Config.walkableClimb))
            {
                throw new NavMeshBuildException("Could not rasterize triangles");
            }

            if (!context.KeepIntermediateResults)
            {
                context.Areas = null;
            }
        }

        private static void FilterWalkableSurface(NavMeshBuildContext context)
        {
            // Once all geoemtry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            if (context.FilterLowHangingObstacles)
            {
                RecastWrapper.Instance.rcFilterLowHangingWalkableObstaclesWrapped(context.ManagedContext, context.Config.walkableClimb, context.Heightfield);
            }

            if (context.FilterLedgeSpans)
            {
                RecastWrapper.Instance.rcFilterLedgeSpansWrapped(context.ManagedContext, context.Config.walkableHeight, context.Config.walkableClimb, context.Heightfield);
            }

            if (context.FilterWalkableLowHeightSpans)
            {
                RecastWrapper.Instance.rcFilterWalkableLowHeightSpansWrapped(context.ManagedContext, context.Config.walkableHeight, context.Heightfield);
            }
        }

        private static void PartitionSurface(NavMeshBuildContext context)
        {
            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            context.CompactHeightfield = RecastWrapper.Instance.rcAllocCompactHeightfieldWrapped();
            if (
                !RecastWrapper.Instance.rcBuildCompactHeightfieldWrapped(
                    context.ManagedContext,
                    context.Config.walkableHeight,
                    context.Config.walkableClimb,
                    context.Heightfield,
                    context.CompactHeightfield))
            {
                throw new NavMeshBuildException("Could not build compact heightfield");
            }

            if (!context.KeepIntermediateResults)
            {
                RecastWrapper.Instance.rcFreeHeightFieldWrapped(context.Heightfield);
                context.Heightfield = null;
            }

            // Erode the walkable area by agent radius.
            if (!RecastWrapper.Instance.rcErodeWalkableAreaWrapped(context.ManagedContext, context.Config.walkableRadius, context.CompactHeightfield))
            {
                throw new NavMeshBuildException("Could not erode walkable area");
            }

            // (Optional) Mark areas.
            RecastWrapper.Instance.rcMarkAllConvexPolyArea(context.ManagedContext, context.InputGeom, context.CompactHeightfield);

            switch (context.PartitionType)
            {
                case PartitionType.Watershed:
                    {
                        if (!RecastWrapper.Instance.rcBuildDistanceFieldWrapped(context.ManagedContext, context.CompactHeightfield))
                        {
                            Diagnostic.Error("could not build distance field");
                            return;
                        }

                        if (
                            !RecastWrapper.Instance.rcBuildRegionsWrapped(
                                context.ManagedContext,
                                context.CompactHeightfield,
                                0,
                                context.Config.minRegionArea,
                                context.Config.mergeRegionArea))
                        {
                            throw new NavMeshBuildException("could not build watershed regions");
                        }

                        break;
                    }

                case PartitionType.Monotone:
                    {
                        if (
                            !RecastWrapper.Instance.rcBuildRegionsMonotoneWrapped(
                                context.ManagedContext,
                                context.CompactHeightfield,
                                0,
                                context.Config.minRegionArea,
                                context.Config.mergeRegionArea))
                        {
                            throw new NavMeshBuildException("could not build monotone regions");
                        }

                        break;
                    }

                case PartitionType.Layers:
                    {
                        if (
                            !RecastWrapper.Instance.rcBuildLayerRegionsWrapped(
                                context.ManagedContext,
                                context.CompactHeightfield,
                                0,
                                context.Config.minRegionArea))
                        {
                            throw new NavMeshBuildException("could not build layer regions");
                        }

                        break;
                    }
            }
        }

        private static void TraceAndSimplifyContours(NavMeshBuildContext context)
        {
            context.ContourSet = RecastWrapper.Instance.rcAllocContourSetWrapped();
            if (
                !RecastWrapper.Instance.rcBuildContoursWrapped(
                    context.ManagedContext,
                    context.CompactHeightfield,
                    context.Config.maxSimplificationError,
                    context.Config.maxEdgeLen,
                    context.ContourSet))
            {
                throw new NavMeshBuildException("cout not create contours");
            }
        }

        private static void BuildPolyMesh(NavMeshBuildContext context)
        {
            context.PolyMesh = RecastWrapper.Instance.rcAllocPolyMeshWrapped();
            if (!RecastWrapper.Instance.rcBuildPolyMeshWrapped(context.ManagedContext, context.ContourSet, context.Config.maxVertsPerPoly, context.PolyMesh))
            {
                throw new NavMeshBuildException("could not triangulate contours");
            }
        }

        private static void BuildPolyDetailMesh(NavMeshBuildContext context)
        {
            context.PolyMeshDetail = RecastWrapper.Instance.rcAllocPolyMeshDetailWrapped();
            if (
                !RecastWrapper.Instance.rcBuildPolyMeshDetailWrapped(
                    context.ManagedContext,
                    context.PolyMesh,
                    context.CompactHeightfield,
                    context.Config.detailSampleDist,
                    context.Config.detailSampleMaxError,
                    context.PolyMeshDetail))
            {
                throw new NavMeshBuildException("could not build detail mesh");
            }

            if (!context.KeepIntermediateResults)
            {
                RecastWrapper.Instance.rcFreeCompactHeightfieldWrapped(context.CompactHeightfield);
                context.CompactHeightfield = null;
                RecastWrapper.Instance.rcFreeContourSetWrapped(context.ContourSet);
                context.ContourSet = null;
            }
        }
    }
}

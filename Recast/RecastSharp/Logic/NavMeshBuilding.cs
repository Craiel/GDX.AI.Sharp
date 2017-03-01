namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
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
            Diagnostic.Info($" - {context.Model.Triangles.Count / 1000.0f} tris");
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

#if DEBUG
            context.ManagedContext.StopTimer();
            context.ManagedContext.LogBuildTimes();

            Diagnostic.Info(">> Polymesh {0} vertices, {1} polygons", context.PolyMesh.VertexCount, context.PolyMesh.PolygonCount);

            Diagnostic.Info("Printing Context Log:");
            for (var i = 0; i < context.ManagedContext.getLogCount(); i++)
            {
                string msg = context.ManagedContext.getLogText(i);
                Diagnostic.Info(msg);
            }
#endif
        }

        private static void LoadModel(NavMeshBuildContext context, CarbonFile sourceFile)
        {
            if (sourceFile == null || !sourceFile.Exists)
            {
                throw new NavMeshBuildException("Source file not defined or not found");
            }
            
            using (var stream = sourceFile.OpenRead())
            {
                context.Model.Parse(stream);
            }

            Diagnostic.Info("Model Loaded with {0} Triangles and {1} Normals", context.Model.Triangles.Count, context.Model.Normals.Count);
        }

        private static void InitializeBoundsAndConfig(NavMeshBuildContext context)
        {
            context.Config.bmin[0] = context.Model.BoundingBox.Min.X;
            context.Config.bmin[1] = context.Model.BoundingBox.Min.Y;
            context.Config.bmin[2] = context.Model.BoundingBox.Min.Z;
            context.Config.bmax[0] = context.Model.BoundingBox.Max.X;
            context.Config.bmax[1] = context.Model.BoundingBox.Max.Y;
            context.Config.bmax[2] = context.Model.BoundingBox.Max.Z;

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

            float[] vertices = context.Model.GetVerticesArray();
            int[] triangles = context.Model.GetTriangleArray();

            // Find triangles which are walkable based on their slope and rasterize them.
            // If your input data is multiple meshes, you can transform them here, calculate
            // the are type for each of the meshes and rasterize them.
            context.Areas = new byte[context.Model.Triangles.Count];
            RecastWrapper.Instance.rcMarkWalkableTrianglesWrapped(
                context.ManagedContext,
                context.Config.walkableSlopeAngle,
                vertices,
                context.Model.Vertices.Count,
                triangles,
                context.Model.Triangles.Count,
                context.Areas);
            if (
                !RecastWrapper.Instance.rcRasterizeTrianglesWrapped(
                    context.ManagedContext,
                    vertices,
                    context.Model.Vertices.Count,
                    triangles,
                    context.Areas,
                    context.Model.Triangles.Count,
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

            // TODO: we don't have the convex volumes in the geometry at the moment
            // (Optional) Mark areas.
            //const ConvexVolume* vols = m_geom->getConvexVolumes();
            //for (int i = 0; i < m_geom->getConvexVolumeCount(); ++i)
            //    rcMarkConvexPolyArea(m_ctx, vols[i].verts, vols[i].nverts, vols[i].hmin, vols[i].hmax, (unsigned char)vols[i].area, *m_chf);
            
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

namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using System;

    using Geometry;

    using RecastWrapper;

    public class NavMeshBuildContext
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public NavMeshBuildContext()
        {
            this.ManagedContext = new ManagedBuildContext();
            this.Config = new ManagedRcConfig();

            this.SetDefaultConfig();

            this.FilterLedgeSpans = true;
            this.FilterLowHangingObstacles = true;
            this.FilterWalkableLowHeightSpans = true;

            this.PartitionType = PartitionType.Watershed;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public ManagedBuildContext ManagedContext { get; private set; }
        
        public ManagedInputGeom InputGeom { get; set; }

        public ManagedRcConfig Config { get; private set; }

        public bool KeepIntermediateResults { get; set; }

        public bool FilterLowHangingObstacles { get; set; }
        public bool FilterLedgeSpans { get; set; }
        public bool FilterWalkableLowHeightSpans { get; set; }

        public ManagedRcHeightfield Heightfield { get; set; }

        public ManagedRcCompactHeightfield CompactHeightfield { get; set; }

        public ManagedRcContourSet ContourSet { get; set; }

        public ManagedRcPolyMesh PolyMesh { get; set; }

        public ManagedRcPolyMeshDetail PolyMeshDetail { get; set; }

        public PartitionType PartitionType { get; set; }

        public byte[] Areas { get; set; }

        public void SetDefaultConfig()
        {
            // Rasterization
            float cellHeight = 0.2f;
            float cellSize = 0.3f;

            // Agent
            float agentHeight = 2.0f;
            float agentRadius = 0.6f;
            float agentMaxClimb = 0.9f;
            float agentMaxSlope = 45;

            // Region
            int minRegionSize = 8;
            int mergedRegionSize = 20;

            // Polygonization
            int maxEdgeLength = 12;
            float maxEdgeError = 1.3f;
            int versPerPoly = 6;

            // Detail Mesh
            float sampleDistance = 6.0f;
            int maxSampleError = 1;

            this.Config.ch = cellHeight;
            this.Config.cs = cellSize;
            this.Config.walkableSlopeAngle = agentMaxSlope;
            this.Config.walkableHeight = (int)Math.Ceiling(agentHeight / cellHeight);
            this.Config.walkableClimb = (int)Math.Floor(agentMaxClimb / cellHeight);
            this.Config.walkableRadius = (int)Math.Ceiling(agentRadius / cellSize);
            this.Config.maxEdgeLen = (int)(maxEdgeLength / cellSize);
            this.Config.maxSimplificationError = maxEdgeError;
            this.Config.minRegionArea = minRegionSize * minRegionSize;
            this.Config.mergeRegionArea = mergedRegionSize * mergedRegionSize;
            this.Config.maxVertsPerPoly = versPerPoly;
            this.Config.detailSampleDist = sampleDistance < 0.9f ? 0 : cellSize * sampleDistance;
            this.Config.detailSampleMaxError = cellHeight * maxSampleError;
        }
    }
}

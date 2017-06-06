namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using Geometry;
    using Microsoft.Xna.Framework;
    using RecastWrapper;

    // For some info see http://digestingduck.blogspot.ca/2009/08/recast-settings-uncovered.html
    public class RecastClientSettings
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public Vector3 WorldBoundsMin { get; set; } = new Vector3(0, 0, 0);
        public Vector3 WorldBoundsMax { get; set; } = new Vector3(7000, 7000, 7000);
        public float CellSize { get; set; } = 0.3f;
        public float CellHeight { get; set; } = 0.2f;
        public float AgentMaxSlope { get; set; } = 45.0f;
        public float AgentHeight { get; set; } = 2.0f;
        public float AgentMaxClimb { get; set; } = 0.9f;
        public float AgentRadius { get; set; } = 0.6f;
        public float EdgeMaxLen { get; set; } = 12.0f;
        public float EdgeMaxError { get; set; } = 1.3f;
        public float RegionMinSize { get; set; } = 8;
        public float RegionMergeSize { get; set; } = 20;
        public float DetailSampleDist { get; set; } = 6.0f;
        public float DetailSampleMaxError { get; set; } = 1.0f;

        public bool FilterLowHangingObstacles { get; set; } = true;
        public bool FilterLedgeSpans { get; set; } = true;
        public bool FilterWalkableLowHeightSpans { get; set; } = true;

        public int MaxAgents { get; set; } = 1000;

        public RecastPartitionMode PartitionType { get; set; } = RecastPartitionMode.WaterShed;

        // -------------------------------------------------------------------
        // Internal
        // -------------------------------------------------------------------
        internal ManagedRecastSettings ToManaged()
        {
            var managedSettings = new ManagedRecastSettings
                       {
                           CellSize = this.CellSize,
                           CellHeight = this.CellHeight,
                           AgentMaxSlope = this.AgentMaxSlope,
                           AgentHeight = this.AgentHeight,
                           AgentMaxClimb = this.AgentMaxClimb,
                           AgentRadius = this.AgentRadius,
                           EdgeMaxLen = this.EdgeMaxLen,
                           EdgeMaxError = this.EdgeMaxError,
                           RegionMinSize = this.RegionMinSize,
                           RegionMergeSize = this.RegionMergeSize,
                           DetailSampleDist = this.DetailSampleDist,
                           DetailSampleMaxError = this.DetailSampleMaxError,

                           FilterLowHangingObstacles = this.FilterLowHangingObstacles,
                           FilterLedgeSpans = this.FilterLedgeSpans,
                           FilterWalkableLowHeightSpans = this.FilterWalkableLowHeightSpans,

                           MaxAgents = this.MaxAgents,

                           PartitionType = (int)this.PartitionType
                       };

            managedSettings.SetWorldBounds(this.WorldBoundsMin.ToArray(), this.WorldBoundsMax.ToArray());

            return managedSettings;
        }
    }
}

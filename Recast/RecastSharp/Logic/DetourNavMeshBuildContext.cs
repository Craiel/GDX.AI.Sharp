namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using RecastWrapper;

    public class DetourNavMeshBuildContext
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DetourNavMeshBuildContext()
        {
            this.Params = new ManagedDtNavMeshCreateParams();
            
            this.AgentHeight = 2.0f;
            this.AgentRadius = 0.6f;
            this.AgentMaxClimb = 0.9f;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public ManagedDtNavMeshCreateParams Params { get; private set; }

        public ManagedDtNavMesh NavMesh { get; set; }

        public ManagedDtCrowd Crowd { get; set; }

        public ManagedDtNavMeshQuery Query { get; set; }

        public byte[] NavMeshData { get; set; }

        public float AgentHeight { get; set; }

        public float AgentRadius { get; set; }

        public float AgentMaxClimb { get; set; }
    }
}

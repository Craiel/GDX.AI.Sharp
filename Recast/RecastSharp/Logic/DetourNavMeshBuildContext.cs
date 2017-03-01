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
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public ManagedDtNavMeshCreateParams Params { get; private set; }

        public ManagedDtNavMesh NavMesh { get; set; }

        public ManagedDtCrowd Crowd { get; set; }

        public ManagedDtNavMeshQuery Query { get; set; }

        public byte[] NavMeshData { get; set; }
    }
}

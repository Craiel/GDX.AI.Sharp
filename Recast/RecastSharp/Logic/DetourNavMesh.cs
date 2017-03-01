namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using RecastWrapper;

    public class DetourNavMesh
    {
        private readonly RecastRuntime runtime;
        private readonly ManagedDtNavMesh navMesh;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DetourNavMesh(RecastRuntime runtime, ManagedDtNavMesh navMesh)
        {
            this.runtime = runtime;
            this.navMesh = navMesh;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        // -------------------------------------------------------------------
        // Internal
        // -------------------------------------------------------------------
        internal ManagedDtNavMesh GetManaged()
        {
            return this.navMesh;
        }
    }
}

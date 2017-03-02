namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using System.Collections.Generic;

    using Enums;

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
        public IList<float[]> GetDebugVertices(NavMeshPolyFlag flags = NavMeshPolyFlag.Walk)
        {
            return DetourWrapper.Instance.GetNavMeshDebugData(this.navMesh, (ushort)flags);
        }

        // -------------------------------------------------------------------
        // Internal
        // -------------------------------------------------------------------
        internal ManagedDtNavMesh GetManaged()
        {
            return this.navMesh;
        }
    }
}

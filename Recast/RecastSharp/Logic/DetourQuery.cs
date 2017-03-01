namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using Geometry;

    using Microsoft.Xna.Framework;

    using RecastWrapper;

    public class DetourQuery
    {
        private readonly RecastRuntime runtime;

        private readonly ManagedDtNavMeshQuery managedQuery;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DetourQuery(RecastRuntime runtime, ManagedDtNavMeshQuery managedQuery)
        {
            this.runtime = runtime;
            this.managedQuery = managedQuery;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool FindRandomPointAroundCircle(ref uint startRef, Vector3 centerPoint, float maxRadius, out uint randomRef, out Vector3 randomPoint)
        {
            float[] randomPointData;
            uint status = this.managedQuery.FindRandomPointAroundCircle(
                ref startRef,
                centerPoint.ToArray(),
                maxRadius,
                this.runtime.Crowd.GetManaged(),
                out randomRef,
                out randomPointData);

            randomPoint = new Vector3(randomPointData[0], randomPointData[1], randomPointData[2]);
            return DetourWrapper.Instance.dtStatusFailedWrapped(status);
        }

        public bool FindNearestPoly(Vector3 centerPoint, Vector3 extends, out uint nearestRef, out Vector3 nearestPoint)
        {
            float[] nearestPointData;
            uint status = this.managedQuery.FindNearestPoly(
                centerPoint.ToArray(),
                extends.ToArray(),
                this.runtime.Crowd.GetManaged(),
                out nearestRef,
                out nearestPointData);

            nearestPoint = new Vector3(nearestPointData[0], nearestPointData[1], nearestPointData[2]);
            return DetourWrapper.Instance.dtStatusFailedWrapped(status);
        }

        // -------------------------------------------------------------------
        // Internal
        // -------------------------------------------------------------------
        internal ManagedDtNavMeshQuery GetManaged()
        {
            return this.managedQuery;
        }
    }
}

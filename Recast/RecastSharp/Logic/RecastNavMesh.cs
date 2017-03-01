namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using System.Collections.Generic;

    using Geometry;

    using Microsoft.Xna.Framework;

    using RecastWrapper;

    public class RecastNavMesh
    {
        private readonly RecastRuntime runtime;

        private readonly ManagedRcPolyMesh polyMesh;

        private readonly ManagedRcPolyMeshDetail polyMeshDetail;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RecastNavMesh(RecastRuntime runtime, ManagedRcPolyMesh polyMesh, ManagedRcPolyMeshDetail polyMeshDetail)
        {
            this.runtime = runtime;
            this.polyMesh = polyMesh;
            this.polyMeshDetail = polyMeshDetail;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public IList<Vector3> GetVertices()
        {
            float[][] rawVertices = this.polyMeshDetail.GetVertices();
            IList<Vector3> result = new List<Vector3>(rawVertices.Length);
            for (var i = 0; i < rawVertices.Length; i++)
            {
                float[] vertex = rawVertices[i];
                result.Add(new Vector3(vertex[0], vertex[1], vertex[2]));
            }

            return result;
        }

        public IList<Triangle3Indexed> GetTriangles()
        {
            byte[][] rawTriangles = this.polyMeshDetail.GetTriangles();
            IList<Triangle3Indexed> result = new List<Triangle3Indexed>(rawTriangles.Length);
            for (var i = 0; i < rawTriangles.Length; i++)
            {
                byte[] vertex = rawTriangles[i];
                result.Add(new Triangle3Indexed(vertex[0], vertex[1], vertex[2]));
            }

            return result;
        }

        // -------------------------------------------------------------------
        // Internal
        // -------------------------------------------------------------------
        internal ManagedRcPolyMesh GetManagedPoly()
        {
            return this.polyMesh;
        }

        internal ManagedRcPolyMeshDetail GetManagedPolyDetail()
        {
            return this.polyMeshDetail;
        }
    }
}

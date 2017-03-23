namespace GDX.AI.Sharp.Geometry
{
    using System;
    using System.Collections.Generic;

    using CarbonCore.Utils;

    using Microsoft.Xna.Framework;

    public class StaticMesh : Mesh
    {
        public bool HasGeometry { get; private set; }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void Clear()
        {
            base.Clear();
            this.HasGeometry = false;
        }

        public override void Join(IList<Vector3> vertices, IList<Vector3> normals, IDictionary<int, int[]> normalMapping, IList<Triangle3Indexed> triangles, Vector3 offset)
        {
            if (this.HasGeometry)
            {
                throw new InvalidOperationException("Join attempted on Static Mesh with geometry data, call clear first before setting new data!");
            }

            if (offset == Vector3.Zero)
            {
                this.Vertices.AddRange(vertices);
            }
            else
            {
                foreach (Vector3 vertex in vertices)
                {
                    this.Vertices.Add(vertex + offset);
                }
            }
            
            this.Normals.AddRange(normals);
            this.NormalMapping.AddRange(normalMapping);
            this.Triangles.AddRange(triangles);

            this.RecalculateBounds();
        }
    }
}

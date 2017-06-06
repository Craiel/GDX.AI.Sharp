namespace GDX.AI.Sharp.Geometry
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Mathematics;

    using Microsoft.Xna.Framework;

    using NLog;

    using Spatial;

    public class Mesh : IEnumerable<Triangle3Indexed>
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public Mesh()
        {
            this.Vertices = new List<Vector3>();
            this.Triangles = new List<Triangle3Indexed>();
            this.Normals = new List<Vector3>();
            this.NormalMapping = new Dictionary<uint, uint[]>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string Name { get; set; }

        public IList<Vector3> Vertices { get; }

        public IList<Triangle3Indexed> Triangles { get; }

        public IList<Vector3> Normals { get; }

        public IDictionary<uint, uint[]> NormalMapping { get; }

        public BoundingBox Bounds { get; private set; }

        public IEnumerator<Triangle3Indexed> GetEnumerator()
        {
            return this.Triangles.GetEnumerator();
        }

        public virtual void Clear()
        {
            this.Name = null;
            this.Vertices.Clear();
            this.Triangles.Clear();
            this.Normals.Clear();
            this.Bounds = new BoundingBox();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Triangles.GetEnumerator();
        }

        public float[] GetVertexArray()
        {
            float[] result = new float[this.Vertices.Count * 3];
            int index = 0;
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                result[index++] = this.Vertices[i].X;
                result[index++] = this.Vertices[i].Y;
                result[index++] = this.Vertices[i].Z;
            }

            return result;
        }

        public int[] GetTriangleArray()
        {
            int[] result = new int[this.Triangles.Count * 3];
            int index = 0;
            for (var i = 0; i < this.Triangles.Count; i++)
            {
                result[index++] = this.Triangles[i].A;
                result[index++] = this.Triangles[i].B;
                result[index++] = this.Triangles[i].C;
            }

            return result;
        }

        public void Join(Mesh other)
        {
            this.Join(other.Vertices, other.Normals, other.NormalMapping, other.Triangles, Vector3.Zero);
        }

        public void Join(IList<Vector3> vertices, IList<Triangle3Indexed> triangles, Vector3 offset)
        {
            this.Join(vertices, new List<Vector3>(), new Dictionary<uint, uint[]>(), triangles, offset);
        }

        public virtual void Join(
            IList<Vector3> vertices,
            IList<Vector3> normals,
            IDictionary<uint, uint[]> normalMapping,
            IList<Triangle3Indexed> triangles,
            Vector3 offset)
        {
            throw new NotSupportedException("Join not supported in this mesh type");
        }

        public bool Verify()
        {
            bool result = true;
            for (var i = 0; i < this.Triangles.Count; i++)
            {
                var triangle = this.Triangles[i];
                if (triangle.A < 0 || triangle.A >= this.Vertices.Count
                    || triangle.B < 0 || triangle.B >= this.Vertices.Count
                    || triangle.C < 0 || triangle.C >= this.Vertices.Count)
                {
                    Logger.Error(" - Invalid Triangle {0} / {1}: {2}", i, this.Triangles.Count, triangle);
                    result = false;
                }
            }

            return result;
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        

        protected void RecalculateBounds()
        {
            this.RecalculateBounds(MathUtils.Epsilon * 2f);
        }

        protected void RecalculateBounds(float padding)
        {
            var newBounds = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
            foreach (Triangle3Indexed triangle in this.Triangles)
            {
                var va = this.Vertices[triangle.A];
                var vb = this.Vertices[triangle.B];
                var vc = this.Vertices[triangle.C];
                ApplyVertexToBounds(ref va, ref newBounds);
                ApplyVertexToBounds(ref vb, ref newBounds);
                ApplyVertexToBounds(ref vc, ref newBounds);
            }

            // pad the bounding box a bit to make sure outer triangles are fully contained.
            ApplyPaddingToBounds(padding, ref newBounds);

            this.Bounds = newBounds;
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Reviewed. Suppression is OK here.")]
        private static void ApplyVertexToBounds(ref Vector3 vertex, ref BoundingBox target)
        {
            if (vertex.X < target.Min.X) { target.Min.X = vertex.X; }
            if (vertex.Y < target.Min.Y) { target.Min.Y = vertex.Y; }
            if (vertex.Z < target.Min.Z) { target.Min.Z = vertex.Z; }
            if (vertex.X > target.Max.X) { target.Max.X = vertex.X; }
            if (vertex.Y > target.Max.Y) { target.Max.Y = vertex.Y; }
            if (vertex.Z > target.Max.Z) { target.Max.Z = vertex.Z; }
        }

        private static void ApplyPaddingToBounds(float padding, ref BoundingBox target)
        {
            target.Min.X -= padding;
            target.Min.Y -= padding;
            target.Min.Z -= padding;
            target.Max.X += padding;
            target.Max.Y += padding;
            target.Max.Z += padding;
        }
    }
}

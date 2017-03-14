namespace GDX.AI.Sharp.Geometry
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;

    using CarbonCore.Utils;
    using CarbonCore.Utils.Diagnostics;

    using Mathematics;

    using Microsoft.Xna.Framework;

    public class ObjModel : IEnumerable<Triangle3>
    {
        private const string CommentIndicator = "#";

        private const char PolygonFaceSeparator = '/';

        private static readonly char[] LineSplitChars = { ' ' };
        
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public ObjModel()
        {
            this.Vertices = new List<Vector3>();
            this.Triangles = new List<Triangle3>();
            this.TrianglesIndexed = new List<Triangle3Indexed>();
            this.Normals = new List<Vector3>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public IList<Vector3> Vertices { get; }
        
        public IList<Triangle3> Triangles { get; }

        public IList<Triangle3Indexed> TrianglesIndexed { get; }

        public IList<Vector3> Normals { get; }

        public BoundingBox BoundingBox { get; private set; }

        public IEnumerator<Triangle3> GetEnumerator()
        {
            return this.Triangles.GetEnumerator();
        }

        public void Parse(Stream stream)
        {
            this.Vertices.Clear();
            this.Triangles.Clear();
            this.Normals.Clear();

            var context = new ParsingContext();

            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    context.CurrentLine = reader.ReadLine();
                    if (string.IsNullOrEmpty(context.CurrentLine))
                    {
                        continue;
                    }

                    // Prepare the line by trimming extra characters and comments
                    TrimComments(context);
                    context.CurrentLine = context.CurrentLine.Trim();
                    context.CurrentLineNumber++;

                    context.CurrentSegments = context.CurrentLine.Split(LineSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (context.CurrentSegments.Length == 0)
                    {
                        continue;
                    }
                    
                    ProcessLine(context);
                }

                this.Vertices.AddRange(context.TempVertices);
                this.Triangles.AddRange(context.Triangles);
                this.TrianglesIndexed.AddRange(context.Triangle3Indexed);
                this.Normals.AddRange(context.Normals);
            }

            this.RecalculateBounds();
        }

        public float[] GetVerticesArray()
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
            int[] result = new int[this.TrianglesIndexed.Count * 3];
            int index = 0;
            for (var i = 0; i < this.Triangles.Count; i++)
            {
                result[index++] = this.TrianglesIndexed[i].A;
                result[index++] = this.TrianglesIndexed[i].B;
                result[index++] = this.TrianglesIndexed[i].C;
            }

            return result;
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Triangles.GetEnumerator();
        }

        private static void TrimComments(ParsingContext context)
        {
            int index = context.CurrentLine.IndexOf(CommentIndicator, StringComparison.Ordinal);
            if (index >= 0)
            {
                context.CurrentLine = context.CurrentLine.Substring(0, index);
            }
        }

        private static void ProcessGeometricVertex(ParsingContext context)
        {
            if (context.CurrentSegments.Length < 4)
            {
                Diagnostic.Warning("Invalid Segment count for Geometric Vertex, Expected 4 but got {0}, line {1}", context.CurrentSegments.Length, context.CurrentLineNumber);
                return;
            }

            Vector3 vertex;
            if (!TryParseVector3(context.CurrentSegments[1], context.CurrentSegments[2], context.CurrentSegments[3], out vertex))
            {
                Diagnostic.Warning("Invalid vertex format in line {0}: {1}", context.CurrentLineNumber, context.CurrentLine);
                return;
            }

            context.TempVertices.Add(vertex);
        }

        private static void ProcessVertexNormal(ParsingContext context)
        {
            if (context.CurrentSegments.Length < 4)
            {
                Diagnostic.Warning("Invalid Segment count for Vertex Normal, Expected 4 but got {0}, line {1}", context.CurrentSegments.Length, context.CurrentLineNumber);
                return;
            }

            Vector3 normal;
            if (!TryParseVector3(context.CurrentSegments[1], context.CurrentSegments[2], context.CurrentSegments[3], out normal))
            {
                Diagnostic.Warning("Invalid vertex normal format in line {0}: {1}", context.CurrentLineNumber, context.CurrentLine);
                return;
            }

            context.TempNormals.Add(normal);
        }

        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Reviewed. Suppression is OK here.")]
        private static void ProcessPolygonFace(ParsingContext context)
        {
            if (context.CurrentSegments.Length < 4)
            {
                Diagnostic.Warning("Invalid Segment count for Polygon Face, Expected  at least 4 but got {0}, line {1}", context.CurrentSegments.Length, context.CurrentLineNumber);
                return;
            }

            if (context.CurrentSegments.Length == 4)
            {
                int v0, v1, v2;
                int n0, n1, n2;

                if (!int.TryParse(context.CurrentSegments[1].Split(PolygonFaceSeparator)[0], out v0)) { return; }
                if (!int.TryParse(context.CurrentSegments[2].Split(PolygonFaceSeparator)[0], out v1)) { return; }
                if (!int.TryParse(context.CurrentSegments[3].Split(PolygonFaceSeparator)[0], out v2)) { return; }
                if (!int.TryParse(context.CurrentSegments[1].Split(PolygonFaceSeparator)[2], out n0)) { return; }
                if (!int.TryParse(context.CurrentSegments[2].Split(PolygonFaceSeparator)[2], out n1)) { return; }
                if (!int.TryParse(context.CurrentSegments[3].Split(PolygonFaceSeparator)[2], out n2)) { return; }

                v0 -= 1;
                v1 -= 1;
                v2 -= 1;
                n0 -= 1;
                n1 -= 1;
                n2 -= 1;

                context.Triangles.Add(new Triangle3(context.TempVertices[v0], context.TempVertices[v1], context.TempVertices[v2]));
                context.Triangle3Indexed.Add(new Triangle3Indexed(v0, v1, v2));

                if (context.TempNormals.Count > 0)
                {
                    context.Normals.Add(context.TempNormals[n0]);
                    context.Normals.Add(context.TempNormals[n1]);
                    context.Normals.Add(context.TempNormals[n2]);
                }
            }
            else
            {
                int v0, n0;
                if (!int.TryParse(context.CurrentSegments[1].Split(PolygonFaceSeparator)[0], out v0)) { return; }
                if (!int.TryParse(context.CurrentSegments[1].Split(PolygonFaceSeparator)[2], out n0)) { return; }

                v0 -= 1;
                n0 -= 1;

                for (int i = 2; i < context.CurrentSegments.Length - 1; i++)
                {
                    int vi, vii;
                    int ni, nii;
                    if (!int.TryParse(context.CurrentSegments[i].Split(PolygonFaceSeparator)[0], out vi)) { continue; }
                    if (!int.TryParse(context.CurrentSegments[i + 1].Split(PolygonFaceSeparator)[0], out vii)) { continue; }
                    if (!int.TryParse(context.CurrentSegments[i].Split(PolygonFaceSeparator)[2], out ni)) { continue; }
                    if (!int.TryParse(context.CurrentSegments[i + 1].Split(PolygonFaceSeparator)[2], out nii)) { continue; }

                    vi -= 1;
                    vii -= 1;
                    ni -= 1;
                    nii -= 1;

                    context.Triangles.Add(new Triangle3(context.TempVertices[v0], context.TempVertices[vi], context.TempVertices[vii]));
                    context.Triangle3Indexed.Add(new Triangle3Indexed(v0, vi, vii));

                    if (context.TempNormals.Count > 0)
                    {
                        context.Normals.Add(context.TempNormals[n0]);
                        context.Normals.Add(context.TempNormals[ni]);
                        context.Normals.Add(context.TempNormals[nii]);
                    }
                }
            }
        }

        private static void ProcessLine(ParsingContext context)
        {
            switch (context.CurrentSegments[0])
            {
                case "v":
                    {
                        ProcessGeometricVertex(context);
                        break;
                    }

                case "vn":
                    {
                        ProcessVertexNormal(context);
                        break;
                    }

                case "f":
                    {
                        ProcessPolygonFace(context);
                        break;
                    }
            }
        }

        private static bool TryParseVector3(string x, string y, string z, out Vector3 v)
        {
            v = Vector3.Zero;

            if (!float.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out v.X))
            {
                return false;
            }

            if (!float.TryParse(y, NumberStyles.Any, CultureInfo.InvariantCulture, out v.Y))
            {
                return false;
            }

            if (!float.TryParse(z, NumberStyles.Any, CultureInfo.InvariantCulture, out v.Z))
            {
                return false;
            }

            return true;
        }

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

        private void RecalculateBounds()
        {
            this.RecalculateBounds(MathUtils.Epsilon * 2f);
        }

        private void RecalculateBounds(float padding)
        {
            var newBounds = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
            foreach (Triangle3 triangle in this.Triangles)
            {
                var va = triangle.A;
                var vb = triangle.B;
                var vc = triangle.C;
                ApplyVertexToBounds(ref va, ref newBounds);
                ApplyVertexToBounds(ref vb, ref newBounds);
                ApplyVertexToBounds(ref vc, ref newBounds);
            }

            // pad the bounding box a bit to make sure outer triangles are fully contained.
            ApplyPaddingToBounds(padding, ref newBounds);

            this.BoundingBox = newBounds;
        }

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        private class ParsingContext
        {
            public readonly IList<Triangle3> Triangles;
            public readonly IList<Triangle3Indexed> Triangle3Indexed;
            public readonly IList<Vector3> Normals;

            public readonly IList<Vector3> TempVertices;
            public readonly IList<Vector3> TempNormals;

            public int CurrentLineNumber;
            public string CurrentLine;
            public string[] CurrentSegments;

            public ParsingContext()
            {
                this.Triangles = new List<Triangle3>();
                this.Triangle3Indexed = new List<Triangle3Indexed>();
                this.Normals = new List<Vector3>();

                this.TempNormals = new List<Vector3>();
                this.TempVertices = new List<Vector3>();
            }
        }
    }
}

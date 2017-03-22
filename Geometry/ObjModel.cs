namespace GDX.AI.Sharp.Geometry
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;

    using Mathematics;

    using Microsoft.Xna.Framework;
    
    public class ObjModel : IEnumerable<Triangle3Indexed>
    {
        private const string LogTag = "ObjModel";

        private const string CommentIndicator = "#";

        private const char PolygonFaceSeparator = '/';
        
        private static readonly char[] LineSplitChars = { ' ' };
        
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public ObjModel()
        {
            this.Vertices = new List<Vector3>();
            this.Triangles = new List<Triangle3Indexed>();
            this.Normals = new List<Vector3>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string Name { get; set; }

        public IList<Vector3> Vertices { get; }
        
        public IList<Triangle3Indexed> Triangles { get; }

        public IList<Vector3> Normals { get; }

        public BoundingBox BoundingBox { get; private set; }

        public IEnumerator<Triangle3Indexed> GetEnumerator()
        {
            return this.Triangles.GetEnumerator();
        }

        public void Clear()
        {
            this.Name = null;
            this.Vertices.Clear();
            this.Triangles.Clear();
            this.Normals.Clear();
            this.BoundingBox = new BoundingBox();
        }

        public void Join(Stream stream)
        {
            GDXAI.Logger.Info(LogTag, "Joining from stream");

            this.Join(this.DoParse(stream));
        }
        
        /// <summary>
        /// Joins two obj models together, this is a heavy operation since we have to re-index the triangles
        /// </summary>
        /// <param name="other">the obj to join with the current one</param>
        public void Join(ObjModel other)
        {
            GDXAI.Logger.Info(LogTag, "Joining from model");

            this.JoinVerticesAndTriangles(other.Vertices, other.Triangles);
            this.JoinNormals(other.Normals);
        }

        public void Parse(Stream stream)
        {
            GDXAI.Logger.Info(LogTag, "Parsing from stream");

            this.Clear();
            this.Join(this.DoParse(stream));
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

        public void Save(StreamWriter target)
        {
            int lineCount = 4;

            target.WriteLine($"g {this.Name ?? "No Name"}");
            foreach (Vector3 vertex in this.Vertices)
            {
                target.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");
                lineCount++;
            }
            
            target.WriteLine();
            foreach (Vector3 normal in this.Normals)
            {
                target.WriteLine($"vn {normal.X} {normal.Y} {normal.Z}");
                lineCount++;
            }

            target.WriteLine();
            foreach (Vector3 vertex in this.Vertices)
            {
                target.WriteLine($"vt {vertex.X} {vertex.Y}");
                lineCount++;
            }

            target.WriteLine();
            foreach (Triangle3Indexed triangle in this.Triangles)
            {
                target.WriteLine($"f {triangle.A}/{triangle.A}/{triangle.A} {triangle.B}/{triangle.B}/{triangle.B} {triangle.C}/{triangle.C}/{triangle.C}");
                lineCount++;
            }
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
                GDXAI.Logger.Error(LogTag, string.Format("Invalid Segment count for Geometric Vertex, Expected 4 but got {0}, line {1}", context.CurrentSegments.Length, context.CurrentLineNumber));
                return;
            }

            Vector3 vertex;
            if (!TryParseVector3(context.CurrentSegments[1], context.CurrentSegments[2], context.CurrentSegments[3], out vertex))
            {
                GDXAI.Logger.Error(LogTag, string.Format("Invalid vertex format in line {0}: {1}", context.CurrentLineNumber, context.CurrentLine));
                return;
            }

            context.TempVertices.Add(vertex);
        }

        private static void ProcessVertexNormal(ParsingContext context)
        {
            if (context.CurrentSegments.Length < 4)
            {
                GDXAI.Logger.Error(LogTag, string.Format("Invalid Segment count for Vertex Normal, Expected 4 but got {0}, line {1}", context.CurrentSegments.Length, context.CurrentLineNumber));
                return;
            }

            Vector3 normal;
            if (!TryParseVector3(context.CurrentSegments[1], context.CurrentSegments[2], context.CurrentSegments[3], out normal))
            {
                GDXAI.Logger.Error(LogTag, string.Format("Invalid vertex normal format in line {0}: {1}", context.CurrentLineNumber, context.CurrentLine));
                return;
            }

            context.TempNormals.Add(normal);
        }

        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Reviewed. Suppression is OK here.")]
        private static void ProcessPolygonFace(ParsingContext context)
        {
            if (context.CurrentSegments.Length < 4)
            {
                GDXAI.Logger.Error(LogTag, string.Format("Invalid Segment count for Polygon Face, Expected  at least 4 but got {0}, line {1}", context.CurrentSegments.Length, context.CurrentLineNumber));
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
                
                context.Triangles.Add(new Triangle3Indexed(v0, v1, v2));

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
                    
                    context.Triangles.Add(new Triangle3Indexed(v0, vi, vii));

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
                case "g":
                    {
                        if (context.CurrentSegments.Length == 2)
                        {
                            context.Name = context.CurrentSegments[1];
                        }

                        break;
                    }

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

            this.BoundingBox = newBounds;
        }

        private void JoinNormals(IList<Vector3> normals)
        {
            GDXAI.Logger.Info(LogTag, string.Format("- {0} normals", normals.Count));
            bool checkNormals = this.Normals.Count > 0;
            int skipped = 0;
            foreach (Vector3 normal in normals)
            {
                if (!checkNormals || !this.Normals.Contains(normal))
                {
                    this.Normals.Add(normal);
                }
                else
                {
                    skipped++;
                }
            }

            if (skipped > 0)
            {
                GDXAI.Logger.Info(LogTag, string.Format("  {0} duplicates", skipped));
            }
        }

        private void JoinVerticesAndTriangles(IList<Vector3> vertices, IList<Triangle3Indexed> triangles)
        {
            int[] indexMap = new int[vertices.Count];

            GDXAI.Logger.Info(LogTag, string.Format("- {0} vertices", vertices.Count));
            bool check = this.Vertices.Count > 0;
            int skipped = 0;
            for (var i = 0; i < vertices.Count; i++)
            {
                if (!check || !this.Vertices.Contains(vertices[i]))
                {
                    this.Vertices.Add(vertices[i]);
                    indexMap[i] = this.Vertices.Count - 1;
                }
                else
                {
                    indexMap[i] = i;
                    skipped++;
                }
            }

            if (skipped > 0)
            {
                GDXAI.Logger.Info(LogTag, string.Format("  {0} duplicates", skipped));
            }

            GDXAI.Logger.Info(LogTag, string.Format("- {0} triangles", triangles.Count));
            foreach (Triangle3Indexed triangle in triangles)
            {
                if (check)
                {
                    // Re-index the triangle
                    this.Triangles.Add(new Triangle3Indexed(indexMap[triangle.A], indexMap[triangle.B], indexMap[triangle.C]));
                }
                else
                {
                    this.Triangles.Add(triangle);
                }
            }
            
            this.RecalculateBounds();
        }

        private void Join(ParsingContext context)
        {
            this.Name = this.Name ?? context.Name;
            this.JoinVerticesAndTriangles(context.TempVertices, context.Triangles);
            this.JoinNormals(context.Normals);
        }

        private ParsingContext DoParse(Stream stream)
        {
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

                    context.CurrentSegments = context.CurrentLine.Split(
                        LineSplitChars,
                        StringSplitOptions.RemoveEmptyEntries);
                    if (context.CurrentSegments.Length == 0)
                    {
                        continue;
                    }

                    ProcessLine(context);
                }
            }
            
            return context;
        }

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        private class ParsingContext
        {
            public ParsingContext()
            {
                this.Triangles = new List<Triangle3Indexed>();
                this.Normals = new List<Vector3>();

                this.TempNormals = new List<Vector3>();
                this.TempVertices = new List<Vector3>();
            }

            public string Name { get; set; }

            public IList<Triangle3Indexed> Triangles { get; }
            public IList<Vector3> Normals { get; }

            public IList<Vector3> TempVertices { get; }
            public IList<Vector3> TempNormals { get; }

            public int CurrentLineNumber { get; set; }
            public string CurrentLine { get; set; }
            public string[] CurrentSegments { get; set; }
        }
    }
}

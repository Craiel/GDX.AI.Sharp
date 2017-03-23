namespace GDX.AI.Sharp.Geometry
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;

    using Mathematics;

    using Microsoft.Xna.Framework;

    using Spatial;

    public class ObjModel : IEnumerable<Triangle3Indexed>
    {
        private const string LogTag = "ObjModel";

        private const string CommentIndicator = "#";

        private const char PolygonFaceSeparator = '/';
        
        private static readonly char[] LineSplitChars = { ' ' };

        private readonly Octree<SpatialInfo> mergeTree;

        private bool enableMerging;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public ObjModel(bool enableMerging = false)
        {
            this.Vertices = new List<Vector3>();
            this.Triangles = new List<Triangle3Indexed>();
            this.Normals = new List<Vector3>();
            this.NormalMapping = new Dictionary<int, int[]>();

            this.enableMerging = enableMerging;

            // Since this is memory heavy we do not provide this by default
            if (this.enableMerging)
            {
                this.mergeTree = new Octree<SpatialInfo>(1f, Vector3.Zero, 1f);
            }
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string Name { get; set; }

        public IList<Vector3> Vertices { get; }
        
        public IList<Triangle3Indexed> Triangles { get; }

        public IList<Vector3> Normals { get; }

        public IDictionary<int, int[]> NormalMapping { get; }

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

            this.Join(other.Vertices, other.Normals, other.NormalMapping, other.Triangles, Vector3.Zero);
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

        // see https://en.wikipedia.org/wiki/Wavefront_.obj_file
        public void Save(StreamWriter target)
        {
            GDXAI.Logger.Info(LogTag, "Saving to stream");

            int lineCount = 4;
            
            target.WriteLine($"g {this.Name ?? "No Name"}");

            GDXAI.Logger.Info(LogTag, string.Format("  - {0} vertices", this.Vertices.Count));
            foreach (Vector3 vertex in this.Vertices)
            {
                target.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");
                lineCount++;
            }
            
            target.WriteLine();

            GDXAI.Logger.Info(LogTag, string.Format("  - {0} normals", this.Normals.Count));
            foreach (Vector3 normal in this.Normals)
            {
                target.WriteLine($"vn {normal.X} {normal.Y} {normal.Z}");
                lineCount++;
            }

            target.WriteLine();
            
            GDXAI.Logger.Info(LogTag, string.Format("  - {0} triangles", this.Triangles.Count));
            for(var i = 0; i < this.Triangles.Count; i++)
            {
                var triangle = this.Triangles[i];

                // Currently we do not support texture coordinates
                if (this.Normals.Count > 0)
                {
                    target.WriteLine($"f {triangle.A + 1}//{this.NormalMapping[i][0] + 1} {triangle.B + 1}//{this.NormalMapping[i][1] + 1} {triangle.C + 1}//{this.NormalMapping[i][2] + 1}");
                }
                else
                {
                    target.WriteLine($"f {triangle.A + 1} {triangle.B + 1} {triangle.C + 1}");
                }

                lineCount++;
            }

            GDXAI.Logger.Info(LogTag, string.Format("  {0} lines", lineCount));
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
                    GDXAI.Logger.Error(LogTag, string.Format(" - Invalid Triangle {0} / {1}: {2}", i, this.Triangles.Count, triangle));
                    result = false;
                }
            }

            return result;
        }

        public void Join(IList<Vector3> vertices, IList<Triangle3Indexed> triangles, Vector3 offset)
        {
            this.Join(vertices, new List<Vector3>(), new Dictionary<int, int[]>(), triangles, offset);
        }

        public void Join(
            IList<Vector3> vertices,
            IList<Vector3> normals,
            IDictionary<int, int[]> normalMapping,
            IList<Triangle3Indexed> triangles,
            Vector3 offset)
        {
            IList<Triangle3> triangleList = new List<Triangle3>();
            for (var i = 0; i < triangles.Count; i++)
            {
                Triangle3Indexed indexed = triangles[i];
                triangleList.Add(new Triangle3(vertices[indexed.A], vertices[indexed.B], vertices[indexed.C]));
            }

            Octree<SpatialInfo> cleanTree = new Octree<SpatialInfo>(1, Vector3.Zero, 1);
            IList<Vector3> cleanVertices = new List<Vector3>();
            IList<Triangle3Indexed> cleanTriangles = new List<Triangle3Indexed>();
            
            for (var i = 0; i < triangleList.Count; i++)
            {
                Triangle3 triangle = triangleList[i];
                int indexA = IndexOfVertex(cleanTree, triangle.A);

                if (indexA < 0)
                {
                    indexA = AddNewVertex(cleanVertices, triangle.A, cleanTree);
                }

                int indexB = IndexOfVertex(cleanTree, triangle.B);
                if (indexB < 0)
                {
                    indexB = AddNewVertex(cleanVertices, triangle.B, cleanTree);
                }

                int indexC = IndexOfVertex(cleanTree, triangle.C);
                if (indexC < 0)
                {
                    indexC = AddNewVertex(cleanVertices, triangle.C, cleanTree);
                }

                cleanTriangles.Add(new Triangle3Indexed(indexA, indexB, indexC));
            }

            if (cleanVertices.Count != vertices.Count)
            {
                GDXAI.Logger.Info(LogTag, string.Format("- {0} orphan vertices", vertices.Count - cleanVertices.Count));
            }

            this.DoJoin(cleanVertices, normals, normalMapping, cleanTriangles, offset);
        }

        private void DoJoin(IList<Vector3> vertices, IList<Vector3> normals, IDictionary<int, int[]> normalMapping, IList<Triangle3Indexed> triangles, Vector3 offset)
        {
            int[] indexMap = new int[vertices.Count];
            int[] normalMap = new int[normals.Count];

            GDXAI.Logger.Info(LogTag, string.Format("- {0} vertices", vertices.Count));
            bool check = this.Vertices.Count > 0;
            int skipped = 0;
            for (var i = 0; i < vertices.Count; i++)
            {
                Vector3 finalVertex = vertices[i] + offset;
                if (!check)
                {
                    indexMap[i] = AddNewVertex(this.Vertices, finalVertex, this.mergeTree);
                    continue;
                }

                if (!this.enableMerging)
                {
                    throw new InvalidOperationException("Obj Merge attempted but not enabled");
                }
                
                int index = IndexOfVertex(this.mergeTree, finalVertex);
                if (index >= 0)
                {
                    indexMap[i] = index;
                    skipped++;
                    continue;
                }

                indexMap[i] = AddNewVertex(this.Vertices, finalVertex, this.mergeTree);
            }

            if (skipped > 0)
            {
                GDXAI.Logger.Info(LogTag, string.Format("  {0} duplicates", skipped));
            }

            GDXAI.Logger.Info(LogTag, string.Format("- {0} normals", normals.Count));
            bool checkNormals = this.Normals.Count > 0;
            skipped = 0;
            for (var i = 0; i < normals.Count; i++)
            {
                var normal = normals[i];

                if (!checkNormals)
                {
                    normalMap[i] = AddNewNormal(this.Normals, normal, this.mergeTree);
                    continue;
                }

                int index = IndexOfNormal(this.mergeTree, normal);
                if (index >= 0)
                {
                    normalMap[i] = index;
                    skipped++;
                    continue;
                }

                normalMap[i] = AddNewNormal(this.Normals, normal, this.mergeTree);
            }

            if (skipped > 0)
            {
                GDXAI.Logger.Info(LogTag, string.Format("  {0} duplicates", skipped));
            }

            GDXAI.Logger.Info(LogTag, string.Format("- {0} triangles", triangles.Count));
            for (var i = 0; i < triangles.Count; i++)
            {
                Triangle3Indexed triangle = triangles[i];

                if (check)
                {
                    // Re-index the triangle
                    this.Triangles.Add(new Triangle3Indexed(indexMap[triangle.A], indexMap[triangle.B], indexMap[triangle.C]));
                }
                else
                {
                    this.Triangles.Add(triangle);
                }

                // Remap the normals for this triangle and 
                if (normalMapping.Count > 0)
                {
                    int[] map = new int[3];
                    for (var n = 0; n < 3; n++)
                    {
                        int normalIndex = normalMapping[i][n];
                        map[n] = normalMap[normalIndex];
                    }

                    this.NormalMapping.Add(this.Triangles.Count - 1, map);
                }
            }

            this.CleanOrphans();
            this.RecalculateBounds();
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

        private static void ReadPolyFacePoint(string[] face, out int vertex, out int? texture, out int? normal)
        {
            texture = null;
            normal = null;

            switch (face.Length)
            {
                case 1:
                    {
                        vertex = int.Parse(face[0]);
                        return;
                    }

                case 2:
                    {
                        vertex = int.Parse(face[0]);
                        texture = int.Parse(face[1]);
                        return;
                    }

                case 3:
                    {
                        vertex = int.Parse(face[0]);

                        if (!string.IsNullOrEmpty(face[1]))
                        {
                            // Texture is optional when having normals
                            texture = int.Parse(face[1]);
                        }

                        normal = int.Parse(face[2]);
                        return;
                    }

                default:
                    {
                        throw new DataException("Invalid PolyFace Data");
                    }
            }
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
                int? n0, n1, n2;
                int? t0, t1, t2;

                string[] f0 = context.CurrentSegments[1].Split(PolygonFaceSeparator);
                string[] f1 = context.CurrentSegments[2].Split(PolygonFaceSeparator);
                string[] f2 = context.CurrentSegments[3].Split(PolygonFaceSeparator);

                ReadPolyFacePoint(f0, out v0, out n0, out t0);
                ReadPolyFacePoint(f1, out v1, out n1, out t1);
                ReadPolyFacePoint(f2, out v2, out n2, out t2);

                v0 -= 1;
                v1 -= 1;
                v2 -= 1;
                if (n0 != null) n0 -= 1;
                if (n1 != null) n1 -= 1;
                if (n2 != null) n2 -= 1;
                if (t0 != null) t0 -= 1;
                if (t1 != null) t1 -= 1;
                if (t2 != null) t2 -= 1;

                context.Triangles.Add(new Triangle3Indexed(v0, v1, v2));

                // Check if we have some normals but not all
                if (n0 != null || n1 != null || n2 != null)
                {
                    if (n0 == null || n1 == null || n2 == null)
                    {
                        throw new DataException("Partial normals are not allowed");
                    }
                }
                
                if (context.TempNormals.Count > 0 && t0 != null && t1 != null && t2 != null)
                {
                    context.Normals.Add(context.TempNormals[n0.Value]);
                    context.Normals.Add(context.TempNormals[n1.Value]);
                    context.Normals.Add(context.TempNormals[n2.Value]);

                    context.NormalMapping.Add(
                            context.Triangles.Count - 1,
                            new[] { context.Normals.Count - 3, context.Normals.Count - 2, context.Normals.Count - 1 });
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
                        context.NormalMapping.Add(
                            context.Triangles.Count - 1,
                            new[] { context.Normals.Count - 3, context.Normals.Count - 2, context.Normals.Count - 1 });
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

        private static int AddNewVertex(IList<Vector3> target, Vector3 vertex, Octree<SpatialInfo> mergeTree)
        {
            target.Add(vertex);
            int index = target.Count - 1;

            if (mergeTree != null)
            {
                OctreeResult<SpatialInfo> info;
                if (mergeTree.GetAt(vertex, out info))
                {
                    info.Entry.Vertex = index;
                }
                else
                {
                    mergeTree.Add(new SpatialInfo(index), vertex);
                }
            }

            return index;
        }

        private static int AddNewNormal(IList<Vector3> target, Vector3 normal, Octree<SpatialInfo> mergeTree)
        {
            target.Add(normal);
            int index = target.Count - 1;

            if (mergeTree != null)
            {
                OctreeResult<SpatialInfo> info;
                if (mergeTree.GetAt(normal, out info))
                {
                    info.Entry.Normal = index;
                }
                else
                {
                    mergeTree.Add(new SpatialInfo(null, index), normal);
                }
            }

            return index;
        }

        private static int IndexOfVertex(Octree<SpatialInfo> tree, Vector3 position)
        {
            OctreeResult<SpatialInfo> result;
            if (tree.GetAt(position, out result) && result.Entry.Vertex != null)
            {
                return result.Entry.Vertex.Value;
            }

            return -1;
        }

        private static int IndexOfNormal(Octree<SpatialInfo> tree, Vector3 position)
        {
            OctreeResult<SpatialInfo> result;
            if (tree.GetAt(position, out result) && result.Entry.Normal != null)
            {
                return result.Entry.Normal.Value;
            }

            return -1;
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
        
        private void Join(ParsingContext context)
        {
            this.Name = this.Name ?? context.Name;
            this.Join(context.TempVertices, context.Normals, context.NormalMapping, context.Triangles, Vector3.Zero);
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

        private void CleanOrphans()
        {
            int[] vertexCheck = new int[this.Vertices.Count];
            int[] normalCheck = new int[this.Normals.Count];
            for (var i = 0; i < this.Triangles.Count; i++)
            {
                var triangle = this.Triangles[i];
                if (this.NormalMapping.ContainsKey(i))
                {
                    for (var n = 0; n < 3; n++)
                    {
                        normalCheck[this.NormalMapping[i][n]]++;
                    }
                }

                vertexCheck[triangle.A]++;
                vertexCheck[triangle.B]++;
                vertexCheck[triangle.C]++;
            }
        }

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        private class ParsingContext
        {
            public ParsingContext()
            {
                this.Triangles = new List<Triangle3Indexed>();
                this.Normals = new List<Vector3>();
                this.NormalMapping = new Dictionary<int, int[]>();

                this.TempNormals = new List<Vector3>();
                this.TempVertices = new List<Vector3>();
            }

            public string Name { get; set; }

            public IList<Triangle3Indexed> Triangles { get; }
            public IList<Vector3> Normals { get; }
            public IDictionary<int, int[]> NormalMapping { get; }

            public IList<Vector3> TempVertices { get; }
            public IList<Vector3> TempNormals { get; }

            public int CurrentLineNumber { get; set; }
            public string CurrentLine { get; set; }
            public string[] CurrentSegments { get; set; }
        }
        
        private class SpatialInfo
        {
            public int? Vertex { get; set; }

            public int? Normal { get; set; }

            public SpatialInfo(int? vertex = null, int? normal = null)
            {
                this.Vertex = vertex;
                this.Normal = normal;
            }
        }
    }
}

namespace GDX.AI.Sharp.Geometry
{
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    using NLog;

    using Spatial;

    public class DynamicMesh : Mesh
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Octree<SpatialInfo> mergeTree;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DynamicMesh(float initialSize = 1f)
            : this(Vector3.Zero, initialSize)
        {
        }

        public DynamicMesh(Vector3 initialPosition, float initialSize = 1f)
        {
            this.mergeTree = new Octree<SpatialInfo>(initialSize, initialPosition, 1f);
        }

        public override void Join(IList<Vector3> vertices, IList<Vector3> normals, IDictionary<int, int[]> normalMapping, IList<Triangle3Indexed> triangles, Vector3 offset)
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
                Logger.Info("- {0} orphan vertices", vertices.Count - cleanVertices.Count);
            }

            this.DoJoin(cleanVertices, normals, normalMapping, cleanTriangles, offset);
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
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

        private void DoJoin(IList<Vector3> vertices, IList<Vector3> normals, IDictionary<int, int[]> normalMapping, IList<Triangle3Indexed> triangles, Vector3 offset)
        {
            int[] indexMap = new int[vertices.Count];
            int[] normalMap = new int[normals.Count];

            Logger.Info("- {0} vertices", vertices.Count);
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
                Logger.Info("  {0} duplicates", skipped);
            }

            Logger.Info("- {0} normals", normals.Count);
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
                Logger.Info("  {0} duplicates", skipped);
            }

            Logger.Info("- {0} triangles", triangles.Count);
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
            
            this.RecalculateBounds();
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

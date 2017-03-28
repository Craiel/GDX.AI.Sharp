namespace GDX.AI.Sharp.Geometry
{
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    using NLog;

    using Spatial;

    public static class MeshUtils
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static void CleanOrphanVertices(IList<Vector3> vertices, IList<Triangle3Indexed> triangles, out IList<Vector3> cleanVertices, out IList<Triangle3Indexed> cleanTriangles)
        {
            // First we build non-indexed triangles so we can re-index them after the cleanup
            IList<Triangle3> triangleList = new List<Triangle3>();
            for (var i = 0; i < triangles.Count; i++)
            {
                Triangle3Indexed indexed = triangles[i];
                triangleList.Add(new Triangle3(vertices[indexed.A], vertices[indexed.B], vertices[indexed.C]));
            }

            cleanVertices = new List<Vector3>();
            cleanTriangles = new List<Triangle3Indexed>();

            Octree<MeshSpatialInfo> cleanTree = new Octree<MeshSpatialInfo>(1, Vector3.Zero, 1);

            for (var i = 0; i < triangleList.Count; i++)
            {
                Triangle3 triangle = triangleList[i];
                uint indexA;
                uint indexB;
                uint indexC;
                
                if (!IndexOfVertex(cleanTree, triangle.A, out indexA))
                {
                    indexA = AddNewVertex(cleanVertices, triangle.A, cleanTree);
                }
                
                if (!IndexOfVertex(cleanTree, triangle.B, out indexB))
                {
                    indexB = AddNewVertex(cleanVertices, triangle.B, cleanTree);
                }
                
                if (!IndexOfVertex(cleanTree, triangle.C, out indexC))
                {
                    indexC = AddNewVertex(cleanVertices, triangle.C, cleanTree);
                }

                cleanTriangles.Add(new Triangle3Indexed(indexA, indexB, indexC));
            }

            if (cleanVertices.Count != vertices.Count)
            {
                Logger.Info("- {0} orphan vertices", vertices.Count - cleanVertices.Count);
            }
        }

        // -------------------------------------------------------------------
        // Internal
        // -------------------------------------------------------------------
        internal static uint AddNewVertex(IList<Vector3> target, Vector3 vertex, Octree<MeshSpatialInfo> mergeTree)
        {
            target.Add(vertex);
            uint index = (uint)target.Count - 1;

            if (mergeTree != null)
            {
                OctreeResult<MeshSpatialInfo> info;
                if (mergeTree.GetAt(vertex, out info))
                {
                    info.Entry.Vertex = index;
                }
                else
                {
                    mergeTree.Add(new MeshSpatialInfo(index), vertex);
                }
            }

            return index;
        }

        internal static uint AddNewNormal(IList<Vector3> target, Vector3 normal, Octree<MeshSpatialInfo> mergeTree)
        {
            target.Add(normal);
            uint index = (uint)target.Count - 1;

            if (mergeTree != null)
            {
                OctreeResult<MeshSpatialInfo> info;
                if (mergeTree.GetAt(normal, out info))
                {
                    info.Entry.Normal = index;
                }
                else
                {
                    mergeTree.Add(new MeshSpatialInfo(null, index), normal);
                }
            }

            return index;
        }

        internal static bool IndexOfVertex(Octree<MeshSpatialInfo> tree, Vector3 position, out uint index)
        {
            OctreeResult<MeshSpatialInfo> result;
            if (tree.GetAt(position, out result) && result.Entry.Vertex != null)
            {
                index = result.Entry.Vertex.Value;
                return true;
            }

            index = 0;
            return false;
        }

        internal static bool IndexOfNormal(Octree<MeshSpatialInfo> tree, Vector3 position, out uint index)
        {
            OctreeResult<MeshSpatialInfo> result;
            if (tree.GetAt(position, out result) && result.Entry.Normal != null)
            {
                index = result.Entry.Normal.Value;
                return true;
            }

            index = 0;
            return false;
        }
    }
}

namespace GDX.AI.Sharp.Spatial
{
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    public class Octree<T>
    {
        private const int RecursionCheckDepth = 20;

        private const string LogTag = "Octree";

        private OctreeNode<T> root;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public Octree(float initialSize, Vector3 initialPosition, float minNodeSize)
        {
            if (minNodeSize > initialSize)
            {
                GDXAI.Logger.Info(LogTag, string.Format("Minimum node size must be bigger or equal initial size: {0} > {1}", minNodeSize, initialSize));
                minNodeSize = initialSize;
            }

            this.InitialSize = initialSize;
            this.MinNodeSize = minNodeSize;
            this.InitialPosition = initialPosition;
            
            this.root = new OctreeNode<T>(initialSize, minNodeSize, initialPosition);
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public float InitialSize { get; private set; }

        public float MinNodeSize { get; private set; }

        public Vector3 InitialPosition { get; private set; }
        
        public int Count { get; private set; }

        public bool AutoGrow { get; set; }

        public bool AutoShrink { get; set; }

        public bool Add(T obj, Vector3 objPos)
        {
            // Add object or expand the octree until it can be added
            int recursionCheck = 0;
            while (!this.root.Add(obj, objPos))
            {
                if (this.AutoGrow)
                {
                    this.Grow(objPos - this.root.Center);
                }
                else
                {
                    return false;
                }

                if (++recursionCheck > RecursionCheckDepth)
                {
                    GDXAI.Logger.Info(LogTag, "Add Operation exceeded recursion check");
                    return false;
                }
            }

            this.Count++;
            return true;
        }

        public bool Remove(T obj)
        {
            // See if we can shrink the octree down now that we've removed the item
            if (this.root.Remove(obj))
            {
                this.Count--;

                if (this.AutoShrink)
                {
                    this.Shrink();
                }

                return true;
            }

            return false;
        }

        public bool GetAt(Vector3 position, out OctreeResult<T> result)
        {
            return this.root.GetAt(position, out result);
        }

        public int GetNearby(Ray ray, float maxDistance, ref IList<OctreeResult<T>> results)
        {
            results.Clear();
            this.root.GetNearby(ref ray, ref maxDistance, ref results);
            return results.Count;
        }

        public void Grow(Vector3 direction)
        {
            int xDirection = direction.X >= 0 ? 1 : -1;
            int yDirection = direction.Y >= 0 ? 1 : -1;
            int zDirection = direction.Z >= 0 ? 1 : -1;
            OctreeNode<T> oldRoot = this.root;
            float half = this.root.Size / 2f;
            float newSize = this.root.Size * 2f;
            Vector3 newCenter = this.root.Center + new Vector3(xDirection * half, yDirection * half, zDirection * half);
            
            // Create 7 new octree children to go with the old root as children of the new root
            int rootPos = GetRootPositionIndex(xDirection, yDirection, zDirection);
            OctreeNode<T>[] children = new OctreeNode<T>[8];
            for (int i = 0; i < 8; i++)
            {
                if (i == rootPos)
                {
                    children[i] = oldRoot;
                }
                else
                {
                    xDirection = i % 2 == 0 ? -1 : 1;
                    yDirection = i > 3 ? -1 : 1;
                    zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                    children[i] = new OctreeNode<T>(newSize, this.MinNodeSize, newCenter + new Vector3(xDirection * half, yDirection * half, zDirection * half));
                }
            }

            // Attach the new children to the new root node
            this.root = new OctreeNode<T>(newSize, this.MinNodeSize, newCenter, children: children);
        }

        public bool Shrink()
        {
            return this.root.Shrink(this.InitialSize, ref this.root);
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private static int GetRootPositionIndex(int x, int y, int z)
        {
            int result = x > 0 ? 1 : 0;
            if (y < 0)
            {
                result += 4;
            }

            if (z > 0)
            {
                result += 2;
            }

            return result;
        }
    }
}

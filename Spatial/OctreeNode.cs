namespace GDX.AI.Sharp.Spatial
{
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    public class OctreeNode<T>
    {
        private const int DefaultObjectLimit = 15;

        private T[] objects;
        private Vector3[] objectPositions;
        private int nextFree;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public OctreeNode(float baseSize, float minSize, Vector3 center, int objectLimit = DefaultObjectLimit, OctreeNode<T>[] children = null)
        {
            this.Size = baseSize;
            this.MinSize = minSize;
            this.Center = center;

            Vector3 offset = new Vector3(this.Size);
            this.Bounds = new BoundingBox(this.Center - offset, this.Center + offset);

            this.objects = new T[objectLimit];
            this.objectPositions = new Vector3[objectLimit];

            this.Children = children;
            
            this.UpdateChildBounds();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public Vector3 Center { get; private set; }

        public float Size { get; private set; }

        public float MinSize { get; private set; }

        public BoundingBox Bounds { get; private set; }

        public BoundingBox[] ChildBounds { get; private set; }

        public OctreeNode<T>[] Children { get; private set; }

        public bool AutoMerge { get; set; }

        public int Count { get; private set; }

        public bool Add(T obj, Vector3 position)
        {
            if (this.Bounds.Contains(position) == ContainmentType.Disjoint)
            {
                return false;
            }

            if (this.Count < this.objects.Length || (this.Size / 2) < this.MinSize)
            {
                this.objects[this.nextFree] = obj;
                this.objectPositions[this.nextFree] = position;
                this.FindNextFreeSlot();
                this.Count++;
                return true;
            }

            int childIndex;
            this.Split();

            // Now that we have children move all objects into them
            for (int i = this.objects.Length - 1; i >= 0; i--)
            {
                if (this.objects[i] == null)
                {
                    continue;
                }

                childIndex = this.GetChildIndex(this.objectPositions[i]);
                this.Children[childIndex].Add(this.objects[i], this.objectPositions[i]);
                this.DeleteObject(i);
            }

            this.nextFree = 0;

            // Now handle the new object we're adding now
            childIndex = this.GetChildIndex(position);
            this.Children[childIndex].Add(obj, position);

            return true;
        }

        public bool Remove(T obj)
        {
            // If we have child nodes check those
            if (this.Children != null)
            {
                bool found = false;
                for (int i = 0; i < 8; i++)
                {
                    found = this.Children[i].Remove(obj);
                    if (found)
                    {
                        break;
                    }
                }

                if (found && this.AutoMerge)
                {
                    // Check if we should merge nodes now that we've removed an item
                    if (this.ShouldMerge())
                    {
                        this.Merge();
                    }
                }

                return false;
            }
            
            // Check local objects since we have no children yet
            for (int i = 0; i < this.objects.Length; i++)
            {
                if (this.objects[i] == null)
                {
                    continue;
                }

                if (this.objects[i].Equals(obj))
                {
                    this.objects[i] = default(T);
                    this.objectPositions[i] = Vector3.Zero;
                    this.nextFree = i;
                    this.Count--;
                    return true;
                }
            }

            return false;
        }

        public bool GetAt(Vector3 position, out OctreeResult<T> result)
        {
            result = default(OctreeResult<T>);
            if (this.Children != null)
            {
                for (var i = 0; i < this.Children.Length; i++)
                {
                    if (this.Children[i].GetAt(position, out result))
                    {
                        return true;
                    }
                }

                return false;
            }

            for (var i = 0; i < this.objectPositions.Length; i++)
            {
                if (this.objectPositions[i] == position)
                {
                    result = new OctreeResult<T>(this.objects[i], position);
                    return true;
                }
            }

            return false;
        }

        public void GetNearby(ref Ray ray, ref float maxDistance, ref IList<OctreeResult<T>> result)
        {
            // Does the ray hit this node at all?
            Vector3 area = new Vector3(maxDistance * 2);
            BoundingBox expanded = new BoundingBox(this.Bounds.Min - area, this.Bounds.Max + area);
            float? value = expanded.Intersects(ray);
            if (value == null)
            {
                return;
            }

            // Check children if any
            if (this.Children != null)
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    this.Children[i].GetNearby(ref ray, ref maxDistance, ref result);
                }

                return;
            }

            // No children so check against any objects in this node
            for (int i = 0; i < this.objects.Length; i++)
            {
                if (this.objects[i] == null)
                {
                    continue;
                }

                if (RayDistance(ray, this.objectPositions[i]) <= maxDistance)
                {
                    result.Add(new OctreeResult<T>(this.objects[i], this.objectPositions[i]));
                }
            }
        }

        public bool Shrink(float minLength, ref OctreeNode<T> parentNode)
        {
            if (this.Size < (2 * minLength))
            {
                return false;
            }

            if (this.Count == 0 && this.Children == null)
            {
                return false;
            }

            int bestFit = -1;

            // Check objects in children if there are any
            if (this.Children != null)
            {
                bool childHadContent = false;
                for (int i = 0; i < this.Children.Length; i++)
                {
                    if (this.Children[i].Count > 0)
                    {
                        if (childHadContent)
                        {
                            // Can't shrink - another child had content already
                            return false;
                        }

                        if (bestFit >= 0 && bestFit != i)
                        {
                            // Can't reduce - objects in root are in a different octant to objects in child
                            return false;
                        }

                        childHadContent = true;
                        bestFit = i;
                    }
                }

                parentNode = this.Children[bestFit];
                return true;
            }

            // Check objects in root
            for (int i = 0; i < this.objects.Length; i++)
            {
                Vector3 position = this.objectPositions[i];
                int newBestFit = this.GetChildIndex(position);
                if (i == 0 || newBestFit == bestFit)
                {
                    if (bestFit < 0)
                    {
                        bestFit = newBestFit;
                    }
                }
                else
                {
                    // Can't reduce - objects fit in different octants
                    return false;
                }
            }
            
            this.Size = this.Size / 2;
            this.Center = this.ChildBounds[bestFit].Min + (this.ChildBounds[bestFit].Max - this.ChildBounds[bestFit].Min);
            return true;
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private static float RayDistance(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.Direction, point - ray.Position).Length();
        }

        private int GetChildIndex(Vector3 position)
        {
            return (position.X <= this.Center.X ? 0 : 1) + (position.Y >= this.Center.Y ? 0 : 4) + (position.Z <= this.Center.Z ? 0 : 2);
        }
        
        private void UpdateChildBounds()
        {
            float quarter = this.Size / 4f;
            float childActualLength = this.Size / 2;
            Vector3 childActualSize = new Vector3(childActualLength, childActualLength, childActualLength);
            this.ChildBounds = new BoundingBox[8];
            this.ChildBounds[0] = new BoundingBox(this.Center + new Vector3(-quarter, quarter, -quarter), childActualSize);
            this.ChildBounds[1] = new BoundingBox(this.Center + new Vector3(quarter, quarter, -quarter), childActualSize);
            this.ChildBounds[2] = new BoundingBox(this.Center + new Vector3(-quarter, quarter, quarter), childActualSize);
            this.ChildBounds[3] = new BoundingBox(this.Center + new Vector3(quarter, quarter, quarter), childActualSize);
            this.ChildBounds[4] = new BoundingBox(this.Center + new Vector3(-quarter, -quarter, -quarter), childActualSize);
            this.ChildBounds[5] = new BoundingBox(this.Center + new Vector3(quarter, -quarter, -quarter), childActualSize);
            this.ChildBounds[6] = new BoundingBox(this.Center + new Vector3(-quarter, -quarter, quarter), childActualSize);
            this.ChildBounds[7] = new BoundingBox(this.Center + new Vector3(quarter, -quarter, quarter), childActualSize);
        }

        private void DeleteObject(int index)
        {
            this.objects[index] = default(T);
            this.objectPositions[index] = Vector3.Zero;
        }

        private void FindNextFreeSlot()
        {
            for (var i = this.nextFree; i < this.objects.Length; i++)
            {
                if (this.objects[i] == null)
                {
                    this.nextFree = i;
                    return;
                }
            }
        }

        private void Split()
        {
            float quarter = this.Size / 4f;
            float newLength = this.Size / 2;
            this.Children = new OctreeNode<T>[8];
            this.Children[0] = new OctreeNode<T>(newLength, this.MinSize, Center + new Vector3(-quarter, quarter, -quarter));
            this.Children[1] = new OctreeNode<T>(newLength, this.MinSize, Center + new Vector3(quarter, quarter, -quarter));
            this.Children[2] = new OctreeNode<T>(newLength, this.MinSize, Center + new Vector3(-quarter, quarter, quarter));
            this.Children[3] = new OctreeNode<T>(newLength, this.MinSize, Center + new Vector3(quarter, quarter, quarter));
            this.Children[4] = new OctreeNode<T>(newLength, this.MinSize, Center + new Vector3(-quarter, -quarter, -quarter));
            this.Children[5] = new OctreeNode<T>(newLength, this.MinSize, Center + new Vector3(quarter, -quarter, -quarter));
            this.Children[6] = new OctreeNode<T>(newLength, this.MinSize, Center + new Vector3(-quarter, -quarter, quarter));
            this.Children[7] = new OctreeNode<T>(newLength, this.MinSize, Center + new Vector3(quarter, -quarter, quarter));
        }

        private bool ShouldMerge()
        {
            int count = 0;
            for (var i = 0; i < this.Children.Length; i++)
            {
                count += this.Children[i].Count;
            }

            return count < this.objects.Length;
        }

        private void Merge()
        {
            for (int i = 0; i < this.Children.Length; i++)
            {
                OctreeNode<T> child = this.Children[i];
                int count = child.Count;
                for (int j = count - 1; j >= 0; j--)
                {
                    this.objects[this.nextFree] = child.objects[j];
                    this.objectPositions[this.nextFree] = child.objectPositions[j];
                    this.FindNextFreeSlot();
                }
            }

            // Remove the child nodes (and the objects in them - they've been added elsewhere now)
            this.Children = null;
        }
    }
}

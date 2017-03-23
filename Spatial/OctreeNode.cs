﻿namespace GDX.AI.Sharp.Spatial
{
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    public class OctreeNode<T>
        where T : class
    {
        private const int DefaultObjectLimit = 15;

        private readonly Octree<T> parent;

        private T[] objects;
        private Vector3[] objectPositions;
        private int nextFree;

        private OctreeNode<T>[] children;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public OctreeNode(Octree<T> parent, float size, float minSize, Vector3 min, int objectLimit = DefaultObjectLimit)
        {
            this.parent = parent;

            this.Size = size;
            this.MinSize = minSize;
            
            this.Bounds = new BoundingBox(min, min + new Vector3(size));
            this.Center = this.Bounds.Min + (this.Bounds.Max - this.Bounds.Min) / 2;
            
            this.objects = new T[objectLimit];
            this.objectPositions = new Vector3[objectLimit];
            
            this.UpdateChildBounds();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public Vector3 Center { get; private set; }

        public float Size { get; private set; }

        public float MinSize { get; }

        public BoundingBox Bounds { get; }

        public BoundingBox[] ChildBounds { get; private set; }

        public bool AutoMerge { get; set; }

        public int Count { get; private set; }

        public static int GetChildIndex(Vector3 center, Vector3 position)
        {
            return (position.X <= center.X ? 0 : 1) + (position.Y >= center.Y ? 0 : 4) + (position.Z <= center.Z ? 0 : 2);
        }

        public bool Add(T obj, Vector3 position)
        {
            if (this.Bounds.Contains(position) == ContainmentType.Disjoint)
            {
                return false;
            }

            if (this.children != null)
            {
                int index = GetChildIndex(this.Center, position);
                return this.children[index].Add(obj, position);
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

                childIndex = GetChildIndex(this.Center, this.objectPositions[i]);
                this.children[childIndex].Add(this.objects[i], this.objectPositions[i]);
                this.DeleteObject(i);
            }

            this.nextFree = 0;
            this.Count = 0;

            // Now handle the new object we're adding now
            childIndex = GetChildIndex(this.Center, position);
            this.children[childIndex].Add(obj, position);

            return true;
        }

        public bool Remove(T obj)
        {
            // If we have child nodes check those
            if (this.children != null)
            {
                bool found = false;
                for (int i = 0; i < 8; i++)
                {
                    found = this.children[i].Remove(obj);
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

        public int CountObjects()
        {
            int count = this.Count;
            if (this.children != null)
            {
                for (var i = 0; i < this.children.Length; i++)
                {
                    count += this.children[i].CountObjects();
                }
            }

            return count;
        }

        public bool GetAt(Vector3 position, out OctreeResult<T> result)
        {
            result = default(OctreeResult<T>);

            if (this.Bounds.Contains(position) == ContainmentType.Disjoint)
            {
                return false;
            }
            
            if (this.children != null)
            {
                for (var i = 0; i < this.children.Length; i++)
                {
                    if (this.children[i].GetAt(position, out result))
                    {
                        return true;
                    }
                }

                return false;
            }

            for (var i = 0; i < this.objectPositions.Length; i++)
            {
                if (this.objects[i] == null)
                {
                    continue;
                }

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
            if (this.children != null)
            {
                for (int i = 0; i < this.children.Length; i++)
                {
                    this.children[i].GetNearby(ref ray, ref maxDistance, ref result);
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

            if (this.Count == 0 && this.children == null)
            {
                return false;
            }

            int bestFit = -1;

            // Check objects in children if there are any
            if (this.children != null)
            {
                bool childHadContent = false;
                for (int i = 0; i < this.children.Length; i++)
                {
                    if (this.children[i].Count > 0)
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

                parentNode = this.children[bestFit];
                return true;
            }

            // Check objects in root
            for (int i = 0; i < this.objects.Length; i++)
            {
                Vector3 position = this.objectPositions[i];
                int newBestFit = GetChildIndex(this.Center, position);
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
            return true;
        }

        public void SetChild(int index, OctreeNode<T> node)
        {
            this.children[index] = node;
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private static float RayDistance(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.Direction, point - ray.Position).Length();
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

        internal void Split()
        {
            float half = this.Size / 2f;
            this.children = new OctreeNode<T>[8];
            this.children[0] = new OctreeNode<T>(this.parent, half, this.MinSize, this.Bounds.Min + new Vector3(0, half, 0));
            this.children[1] = new OctreeNode<T>(this.parent, half, this.MinSize, this.Bounds.Min + new Vector3(half, half, 0));
            this.children[2] = new OctreeNode<T>(this.parent, half, this.MinSize, this.Bounds.Min + new Vector3(0, half, half));
            this.children[3] = new OctreeNode<T>(this.parent, half, this.MinSize, this.Bounds.Min + new Vector3(half, half, half));
            this.children[4] = new OctreeNode<T>(this.parent, half, this.MinSize, this.Bounds.Min + new Vector3(0, 0, 0));
            this.children[5] = new OctreeNode<T>(this.parent, half, this.MinSize, this.Bounds.Min + new Vector3(half, 0, 0));
            this.children[6] = new OctreeNode<T>(this.parent, half, this.MinSize, this.Bounds.Min + new Vector3(0, 0, half));
            this.children[7] = new OctreeNode<T>(this.parent, half, this.MinSize, this.Bounds.Min + new Vector3(half, 0, half));
        }

        private bool ShouldMerge()
        {
            int count = 0;
            for (var i = 0; i < this.children.Length; i++)
            {
                count += this.children[i].Count;
            }

            return count < this.objects.Length;
        }

        private void Merge()
        {
            for (int i = 0; i < this.children.Length; i++)
            {
                OctreeNode<T> child = this.children[i];
                int count = child.Count;
                for (int j = count - 1; j >= 0; j--)
                {
                    this.objects[this.nextFree] = child.objects[j];
                    this.objectPositions[this.nextFree] = child.objectPositions[j];
                    this.FindNextFreeSlot();
                }
            }

            // Remove the child nodes (and the objects in them - they've been added elsewhere now)
            this.children = null;
        }
    }
}
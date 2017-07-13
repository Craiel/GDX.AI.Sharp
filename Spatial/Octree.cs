namespace GDX.AI.Sharp.Spatial
{
    using System;
    using System.Collections.Generic;
    using Mathematics;
    using Microsoft.Xna.Framework;

    using NLog;

    internal static class OctreeConstants
    {
        internal const int RecursionCheckDepth = 20;

        internal const int OctreeFloatPrecision = 4;
    }

    internal static class OctreeStatic
    {
        internal static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
    }
    
    public class Octree<T>
        where T : class
    {
        private OctreeNode<T> root;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public Octree(float initialSize, Vector3 initialPosition, float minNodeSize)
        {
            if (minNodeSize > initialSize)
            {
                OctreeStatic.Logger.Info("Minimum node size must be bigger or equal initial size: {0} > {1}", minNodeSize, initialSize);
                minNodeSize = initialSize;
            }

            this.InitialSize = initialSize;
            this.MinNodeSize = minNodeSize;
            this.InitialPosition = initialPosition;
            
            this.root = new OctreeNode<T>(this, initialSize, minNodeSize, initialPosition);

            this.AutoGrow = true;
            this.AutoShrink = true;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public float InitialSize { get; }

        public float MinNodeSize { get; }

        public Vector3 InitialPosition { get; }
        
        public int Count { get; private set; }

        public bool AutoGrow { get; set; }

        public bool AutoShrink { get; set; }

        public bool AutoMerge
        {
            get => this.root.AutoMerge;
            set => this.root.AutoMerge = value;
        }

        public bool Add(T obj, Vector3 objPos)
        {
#if DEBUG
            if (Math.Abs(objPos.Length()) > MathUtils.MaxFloat)
            {
                OctreeStatic.Logger.Error("Add Operation failed, coordinates are outside of safe range");
                return false;
            }
#endif

            Vector3 positionVector = objPos.WithMaxPrecision(OctreeConstants.OctreeFloatPrecision);

            if (positionVector.X < this.root.Bounds.Min.X || positionVector.Y < this.root.Bounds.Min.Y ||
                positionVector.Z < this.root.Bounds.Min.Z)
            {
                OctreeStatic.Logger.Error("Object position outside of octree lower bounds!");
                return false;
            }
            
            // Add object or expand the octree until it can be added
            int recursionCheck = 0;
            while (!this.root.Add(obj, positionVector))
            {
                if (this.AutoGrow)
                {
                    this.Grow();
                }
                else
                {
                    return false;
                }

                if (++recursionCheck > OctreeConstants.RecursionCheckDepth)
                {
                    OctreeStatic.Logger.Info("Add Operation exceeded recursion check");
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

        public int CountObjects()
        {
            return this.root.CountObjects();
        }

        public bool GetAt(Vector3 position, out OctreeResult<T> result)
        {
            return this.root.GetAt(position.WithMaxPrecision(OctreeConstants.OctreeFloatPrecision), out result);
        }

        public int GetNearby(Ray ray, float maxDistance, ref IList<OctreeResult<T>> results)
        {
            results.Clear();
            this.root.GetNearby(ref ray, ref maxDistance, ref results);
            return results.Count;
        }

        public void Grow()
        {
            float newSize = this.root.Size * 2f;

            var newRoot =
                new OctreeNode<T>(this, newSize, this.MinNodeSize, this.InitialPosition)
                {
                    AutoMerge = this.root.AutoMerge
                };

            newRoot.Split();
            int rootPos = OctreeNode<T>.GetChildIndex(newRoot.Center, this.root.Center);
            newRoot.SetChild(rootPos, this.root);
            this.root = newRoot;
        }

        public bool Shrink()
        {
            return this.root.Shrink(this.InitialSize, ref this.root);
        }

        public void Merge()
        {
            this.root.ForceMerge();
        }
    }
}

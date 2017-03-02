namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using System.Collections.Generic;

    using Enums;

    using Geometry;

    using Microsoft.Xna.Framework;

    using RecastWrapper;

    public class RecastNavMesh
    {
        private readonly RecastRuntime runtime;

        private readonly ManagedRcPolyMesh polyMesh;

        private readonly ManagedRcPolyMeshDetail polyMeshDetail;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RecastNavMesh(RecastRuntime runtime, ManagedRcPolyMesh polyMesh, ManagedRcPolyMeshDetail polyMeshDetail)
        {
            this.runtime = runtime;
            this.polyMesh = polyMesh;
            this.polyMeshDetail = polyMeshDetail;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void SetDefaultPolyFlags()
        {
            // Update poly flags from areas.
            for (int i = 0; i < this.polyMesh.PolygonCount; ++i)
            {
                int area = this.polyMesh.GetArea(i);
                if (area == RecastWrapper.RC_WALKABLE_AREA_W)
                {
                    area = (int)NavMeshPolyArea.Ground;
                    this.polyMesh.SetArea(i, (int)NavMeshPolyArea.Ground);
                }

                if (area == (int)NavMeshPolyArea.Ground ||
                    area == (int)NavMeshPolyArea.Grass ||
                    area == (int)NavMeshPolyArea.Road)
                {
                    this.polyMesh.SetFlags(i, (int)NavMeshPolyFlag.Walk);
                }
                else if (area == (int)NavMeshPolyArea.Water)
                {
                    this.polyMesh.SetFlags(i, (int)NavMeshPolyFlag.Swim);
                }
                else if (area == (int)NavMeshPolyArea.Door)
                {
                    this.polyMesh.SetFlags(i, (int)(NavMeshPolyFlag.Walk | NavMeshPolyFlag.Door));
                }
            }
        }

        // -------------------------------------------------------------------
        // Internal
        // -------------------------------------------------------------------
        internal ManagedRcPolyMesh GetManagedPoly()
        {
            return this.polyMesh;
        }

        internal ManagedRcPolyMeshDetail GetManagedPolyDetail()
        {
            return this.polyMeshDetail;
        }
    }
}

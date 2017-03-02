namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using CarbonCore.Utils.Diagnostics;

    using Geometry;

    using Microsoft.Xna.Framework;

    using RecastWrapper;

    public class DetourCrowd
    {
        private readonly RecastRuntime runtime;
        private readonly ManagedDtCrowd managedCrowd;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DetourCrowd(RecastRuntime runtime, ManagedDtCrowd managedCrowd)
        {
            this.runtime = runtime;
            this.managedCrowd = managedCrowd;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public int Agents => this.managedCrowd.GetAgentCount();

        public void Initialize(int maxAgents, float agentRadius, bool setDefaultAvoidance = true)
        {
            if (!this.managedCrowd.Init(maxAgents, agentRadius, this.runtime.DetourNavMesh.GetManaged()))
            {
                Diagnostic.Warning("Failed to initialize crowd");
                return;
            }

            if (setDefaultAvoidance)
            {
                this.managedCrowd.SetDefaultAvoidanceParameters();
            }
        }

        public void Update(float delta)
        {
            //this.managedCrowd.Update(delta);
        }

        public DetourCrowdAgent GetAgent(int index)
        {
            return new DetourCrowdAgent(this.runtime, this.managedCrowd.GetAgent(index));
        }

        public int AddAgent(Vector3 position, DetourCrowdAgentParameters parameters)
        {
            return this.managedCrowd.AddAgent(position.ToArray(), parameters.GetManaged());
        }

        public bool RequestMoveTarget(int index, uint polyRef, Vector3 position)
        {
            return this.managedCrowd.RequestMoveTarget(index, polyRef, position.ToArray());
        }

        // -------------------------------------------------------------------
        // Internal
        // -------------------------------------------------------------------
        internal ManagedDtCrowd GetManaged()
        {
            return this.managedCrowd;
        }
    }
}

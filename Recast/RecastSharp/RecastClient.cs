namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using System.Collections.Generic;

    using CarbonCore.Utils.IO;

    using Contracts;

    using Geometry;

    using Microsoft.Xna.Framework;

    using RecastWrapper;

    public class RecastClient
    {
        private readonly ILogger logger;
        private readonly ManagedRecastClient managedClient;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RecastClient(ILogger logger)
        {
            this.logger = logger;
            this.managedClient = new ManagedRecastClient();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool Load(CarbonFile file)
        {
            bool result = this.managedClient.Load(file.GetPath());
            
            IList<string> buildlogText = this.managedClient.GetLogText();
            foreach (string line in buildlogText)
            {
                this.logger.Debug("RecastClient", line);
            }

            return result;
        }

        public void Update(float delta)
        {
            this.managedClient.Update(delta);
        }

        public bool FindRandomPointAroundCircle(ref uint startRef, Vector3 centerPosition, float maxRadius, out uint randomRef, out Vector3 randomPosition)
        {
            float[] pos;
            bool result = this.managedClient.FindRandomPointAroundCircle(
                ref startRef,
                centerPosition.ToArray(),
                maxRadius,
                out randomRef,
                out pos);

            randomPosition = new Vector3(pos[0], pos[1], pos[2]);
            return result;
        }
        
        public bool FindNearestPoly(Vector3 center, Vector3 extents, out uint nearestRef, out Vector3 nearestPoint)
        {
            float[] pos;
            bool result = this.managedClient.FindNearestPoly(center.ToArray(), extents.ToArray(), out nearestRef, out pos);
            nearestPoint = new Vector3(pos[0], pos[1], pos[2]);
            return result;
        }

        public int AddAgent(Vector3 position, DetourCrowdAgentParameters parameters)
        {
            return this.managedClient.AddAgent(position.ToArray(), parameters.GetManaged());
        }

        public bool RequestMoveTarget(int agentIndex, uint polyRef, Vector3 position)
        {
            return this.managedClient.RequestMoveTarget(agentIndex, polyRef, position.ToArray());
        }

        public bool ResetMoveTarget(int agentIndex)
        {
            return this.managedClient.ResetMoveTarget(agentIndex);
        }

        public DetourCrowdAgentInfo GetAgentInfo(int agentIndex)
        {
            ManagedDtCrowdAgentInfo info;
            this.managedClient.GetAgentInfo(agentIndex, out info);
            return new DetourCrowdAgentInfo
                       {
                           position = new Vector3(info.npos[0], info.npos[1], info.npos[2]),
                           velocity = new Vector3(info.vel[0], info.vel[1], info.vel[2]),
                           targetState = info.targetState,
                           targetPosition =
                               new Vector3(info.targetPos[0], info.targetPos[1], info.targetPos[2])
                       };
        }
    }
}

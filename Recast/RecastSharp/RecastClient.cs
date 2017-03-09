namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using System.Collections.Generic;

    using CarbonCore.Utils.IO;
    
    using Geometry;

    using Microsoft.Xna.Framework;

    using RecastWrapper;

    public class RecastClient
    {
        public const uint InvalidObstacleRef = 0;

        private readonly ManagedRecastClient managedClient;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RecastClient(RecastClientSettings settings)
        {
            this.managedClient = new ManagedRecastClient(RecastClientMode.RECAST_TILED_MESH, settings.ToManaged());
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool LoadObj(CarbonFile file)
        {
            bool result = this.managedClient.LoadObj(file.GetPath());
            
            this.managedClient.LogBuildTimes();

            IList<string> buildlogText = this.managedClient.GetLogText();
            foreach (string line in buildlogText)
            {
                GDXAI.Logger.Debug("RecastClient", line);
            }

            return result;
        }

        public bool Load(byte[] data)
        {
            return this.managedClient.Load(data);
        }

        public bool Save(out byte[] data)
        {
            return this.managedClient.Save(out data);
        }

        public bool GetDebugNavMesh(out byte[] data)
        {
            return this.managedClient.GetDebugNavMesh(out data);
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

        public void RemoveAgent(int index)
        {
            this.managedClient.RemoveAgent(index);
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

        public bool AddObstacle(Vector3 position, float radius, float height, out uint obstacleRef)
        {
            return this.managedClient.AddObstacle(position.ToArray(), radius, height, out obstacleRef);
        }

        public bool AddObstacleBox(Vector3 min, Vector3 max, out uint obstacleRef)
        {
            return this.managedClient.AddObstacleBox(min.ToArray(), max.ToArray(), out obstacleRef);
        }

        public bool RemoveObstacle(uint obstacleRef)
        {
            return this.managedClient.RemoveObstacle(obstacleRef);
        }

        public void ClearObstacles()
        {
            this.managedClient.ClearObstacles();
        }
    }
}

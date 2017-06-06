namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using Geometry;

    using Microsoft.Xna.Framework;

    using RecastWrapper;
    
    public abstract class RecastClient
    {
        public const uint InvalidObstacleRef = 0;
        
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void Update(float delta)
        {
            this.ManagedClient.Update(delta);
        }

        public bool FindRandomPointAroundCircle(uint startRef, Vector3 centerPosition, float maxRadius, out uint randomRef, out Vector3 randomPosition)
        {
            float[] pos;
            bool result = this.ManagedClient.FindRandomPointAroundCircle(
                startRef,
                centerPosition.ToArray(),
                maxRadius,
                out randomRef,
                out pos);

            randomPosition = new Vector3(pos[0], pos[1], pos[2]);
            return result;
        }

        public bool FindPath(uint startRef, uint endRef, Vector3 startPos, Vector3 endPos, out uint[] path)
        {
            bool result = this.ManagedClient.FindPath(startRef, endRef, startPos.ToArray(), endPos.ToArray(),
                out path);
            
            return result;
        }

        public bool GetSmoothPath(uint startRef, Vector3 startPos, Vector3 endPos, uint[] path, out float[] points)
        {
            return this.ManagedClient.GetSmoothPath(startRef, startPos.ToArray(), endPos.ToArray(), path, out points);
        }
        
        public bool FindNearestPoly(Vector3 center, Vector3 extents, out uint nearestRef, out Vector3 nearestPoint)
        {
            float[] pos;
            bool result = this.ManagedClient.FindNearestPoly(center.ToArray(), extents.ToArray(), out nearestRef, out pos);
            if (pos == null || pos.Length != 3)
            {
                nearestPoint = Vector3.Zero;
                return false;
            }

            nearestPoint = new Vector3(pos[0], pos[1], pos[2]);
            return result;
        }

        public int AddAgent(Vector3 position, DetourCrowdAgentParameters parameters)
        {
            return this.ManagedClient.AddAgent(position.ToArray(), parameters.GetManaged());
        }

        public void RemoveAgent(int index)
        {
            this.ManagedClient.RemoveAgent(index);
        }

        public void UpdateAgent(int index, DetourCrowdAgentParameters parameters)
        {
            this.ManagedClient.UpdateAgent(index, parameters.GetManaged());
        }

        public bool RequestMoveTarget(int agentIndex, uint polyRef, Vector3 position)
        {
            return this.ManagedClient.RequestMoveTarget(agentIndex, polyRef, position.ToArray());
        }

        public bool ResetMoveTarget(int agentIndex)
        {
            return this.ManagedClient.ResetMoveTarget(agentIndex);
        }

        public DetourCrowdAgentInfo GetAgentInfo(int agentIndex)
        {
            ManagedDtCrowdAgentInfo info;
            this.ManagedClient.GetAgentInfo(agentIndex, out info);
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
            return this.ManagedClient.AddObstacle(position.ToArray(), radius, height, out obstacleRef);
        }

        public bool AddObstacleBox(Vector3 min, Vector3 max, out uint obstacleRef)
        {
            return this.ManagedClient.AddObstacleBox(min.ToArray(), max.ToArray(), out obstacleRef);
        }

        public bool RemoveObstacle(uint obstacleRef)
        {
            return this.ManagedClient.RemoveObstacle(obstacleRef);
        }

        public void ClearObstacles()
        {
            this.ManagedClient.ClearObstacles();
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected ManagedRecastClient ManagedClient { get; set; }
    }
}

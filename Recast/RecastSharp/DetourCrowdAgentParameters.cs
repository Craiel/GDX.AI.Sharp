namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using RecastWrapper;

    public class DetourCrowdAgentParameters
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DetourCrowdAgentParameters()
        {
            this.ObstacleAvoidanceType = 3.0f;
            this.UpdateFlags = DetourUpdateFlags.AnticipateTurns | DetourUpdateFlags.ObstacleAvoidance;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public float Radius { get; set; }

        public float Height { get; set; }

        public float MaxAcceleration { get; set; }

        public float MaxSpeed { get; set; }

        public float CollisionQueryRange { get; set; }

        public float PathOptimizationRange { get; set; }

        public float SeparationWeight { get; set; }

        public float ObstacleAvoidanceType { get; set; }

        public byte QueryFilterType { get; set; }

        public DetourUpdateFlags UpdateFlags { get; set; }

        // -------------------------------------------------------------------
        // Internal
        // -------------------------------------------------------------------
        internal ManagedDtCrowdAgentParams GetManaged()
        {
            return new ManagedDtCrowdAgentParams
            {
                radius = this.Radius,
                height = this.Height,
                maxAcceleration = this.MaxAcceleration,
                maxSpeed = this.MaxSpeed,
                collisionQueryRange = this.CollisionQueryRange,
                pathOptimizationRange = this.PathOptimizationRange,
                separationWeight = this.SeparationWeight,
                obstacleAvoidanceType = this.ObstacleAvoidanceType,
                queryFilterType = this.QueryFilterType,
                updateFlags = (byte)this.UpdateFlags
            };
        }
    }
}

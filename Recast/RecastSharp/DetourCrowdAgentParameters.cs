namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using AI.Recast.Protocol;
    using Google.Protobuf;
    using Microsoft.Xna.Framework;

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
        public Vector3 InitialPosition { get; set; }

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
        internal byte[] GetData()
        {
            var proto = new ProtoCrowdAgentParameters
            {
                InitialPosition = new ProtoNavMeshVector
                {
                    X = this.InitialPosition.X,
                    Y = this.InitialPosition.Y,
                    Z = this.InitialPosition.Z
                },
                Radius = this.Radius,
                Height = this.Height,
                MaxAcceleration = this.MaxAcceleration,
                MaxSpeed = this.MaxSpeed,
                CollisionQueryRange = this.CollisionQueryRange,
                PathOptimizationRange = this.PathOptimizationRange,
                SeparationWeight = this.SeparationWeight,
                ObstacleAvoidanceType = this.ObstacleAvoidanceType,
                QueryFilterType = this.QueryFilterType,
                UpdateFlags = (uint) this.UpdateFlags
            };

            return proto.ToByteArray();
        }
    }
}

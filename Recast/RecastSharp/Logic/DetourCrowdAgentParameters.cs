namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using RecastWrapper;

    public class DetourCrowdAgentParameters
    {
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
                           separationWeight = this.SeparationWeight
                       };
        }
    }
}

namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using Microsoft.Xna.Framework;

    using RecastWrapper;

    public class DetourCrowdAgent
    {
        private readonly RecastRuntime runtime;

        private readonly ManagedDtCrowdAgent agent;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DetourCrowdAgent(RecastRuntime runtime, ManagedDtCrowdAgent agent)
        {
            this.runtime = runtime;
            this.agent = agent;
        }

        /*public Vector3 Position
        {
            get
            {
                // TODO: this is some heavy marshal, check if we can optimize this
                float[] pos = this.agent.GetPosition();
                return new Vector3(pos[0], pos[1], pos[2]);
            }
        }

        public GDX.AI.Sharp.Recast.RecastSharp.Enums.MoveRequestState State => (GDX.AI.Sharp.Recast.RecastSharp.Enums.MoveRequestState)this.agent.TargetState;*/
    }
}

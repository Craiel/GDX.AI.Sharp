namespace Craiel.GDX.AI.Sharp.Runtime
{
    using Contracts;

    public class DefaultTimePiece : ITimePiece
    {
        private readonly float maxDeltaTime;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DefaultTimePiece()
            : this(float.MaxValue)
        {
        }

        public DefaultTimePiece(float maxDeltaTime)
        {
            this.maxDeltaTime = maxDeltaTime;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public ulong Tick { get; private set; }

        public float Time { get; private set; }

        public float DeltaTime { get; private set; }

        public void Update(float delta)
        {
            this.Tick++;
            this.DeltaTime = delta > this.maxDeltaTime ? this.maxDeltaTime : delta;
            this.Time += delta;
        }
    }
}

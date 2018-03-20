using IBlackboard = Craiel.GDX.AI.Sharp.Contracts.IBlackboard;

namespace Craiel.GDX.AI.Sharp.BTree.Decorators
{
    /// <summary>
    /// Executes the child only when a certain delay time has passed and gets reset on execution
    /// </summary>
    /// <typeparam name="T">the type of blackbard this task uses</typeparam>
    public class Interval<T> : Decorator<T>
        where T : IBlackboard
    {
        private const float DefaultDelay = 1f;

        private float time;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public Interval()
            : this(TaskId.Invalid, DefaultDelay)
        {
        }

        public Interval(float delay)
            : this(TaskId.Invalid, delay)
        {
        }

        public Interval(TaskId child, float delay)
            : base(child)
        {
            this.Delay = delay;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        /// <summary>
        /// The delay between each execution of the child task
        /// </summary>
        public float Delay { get; set; }
        
        public override void Run()
        {
            this.time += GDXAI.TimePiece.DeltaTime;
            if (this.time < this.Delay)
            {
                // Not enough time passed
                this.Success();
                return;
            }

            this.time = 0;
            base.Run();
        }
    }
}

using BTTaskStatus = Craiel.GDX.AI.Sharp.Runtime.Enums.BTTaskStatus;
using ConstantFloatDistribution = Craiel.UnityEssentials.Runtime.Mathematics.Rnd.ConstantFloatDistribution;
using FloatDistribution = Craiel.UnityEssentials.Runtime.Mathematics.Rnd.FloatDistribution;
using IBlackboard = Craiel.GDX.AI.Sharp.Runtime.Contracts.IBlackboard;

namespace Craiel.GDX.AI.Sharp.Runtime.BTree.Leafs
{
    /// <summary>
    /// <see cref="Wait{T}"/> is a leaf that keeps running for the specified amount of time then succeeds
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class Wait<T> : LeafTask<T>
        where T : IBlackboard
    {
        private float startTime;
        private float timeout;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------

        /// <summary>
        /// Creates a <see cref="Wait{T}"/> task that immediately succeeds
        /// </summary>
        public Wait()
            : this(ConstantFloatDistribution.Zero)
        {
        }

        /// <summary>
        /// Creates a <see cref="Wait{T}"/> task running for the specified number of seconds
        /// </summary>
        /// <param name="seconds">the number of seconds to wait for</param>
        public Wait(float seconds)
            : this(new ConstantFloatDistribution(seconds))
        {
        }

        /// <summary>
        /// Creates a <see cref="Wait{T}"/> task running for the specified number of seconds
        /// </summary>
        /// <param name="seconds">the random distribution determining the number of seconds to wait for</param>
        public Wait(FloatDistribution seconds)
        {
            this.Seconds = seconds;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        /// <summary>
        /// Mandatory task attribute specifying the random distribution that determines the timeout in seconds
        /// </summary>
        public FloatDistribution Seconds { get; private set; }

        public override void Start()
        {
            this.timeout = this.Seconds.NextFloat();
            this.startTime = GDXAI.TimePiece.Time;

            base.Start();
        }

        /// <summary>
        /// Executes this <see cref="Wait{T}"/> task
        /// </summary>
        /// <returns>Succeeded if the specified timeout has expired; Running otherwise</returns>
        protected override BTTaskStatus Execute()
        {
            return GDXAI.TimePiece.Time - this.startTime < this.timeout ? BTTaskStatus.Running : BTTaskStatus.Succeeded;
        }

        protected override void CopyTo(Task<T> clone)
        {
            Wait<T> wait = (Wait<T>)clone;
            wait.Seconds = this.Seconds.Clone<FloatDistribution>();
        }
    }
}

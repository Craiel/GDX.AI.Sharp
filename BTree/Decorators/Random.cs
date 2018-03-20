using ConstantFloatDistribution = Craiel.UnityEssentials.Mathematics.Rnd.ConstantFloatDistribution;
using FloatDistribution = Craiel.UnityEssentials.Mathematics.Rnd.FloatDistribution;
using IBlackboard = Craiel.GDX.AI.Sharp.Contracts.IBlackboard;

namespace Craiel.GDX.AI.Sharp.BTree.Decorators
{
    /// <summary>
    /// The <see cref="Random{T}"/> decorator succeeds with the specified probability, regardless of whether the wrapped task fails or succeeds.
    /// Also, the wrapped task is optional, meaning that this decorator can act like a leaf task.
    /// <para></para>
    /// Notice that if success probability is 1 this task is equivalent to the decorator <see cref="Craiel.GDX.AI.Sharp.BTree.Decorators.AlwaysSucceed{T}"/> and the leaf <see cref="Craiel.GDX.AI.Sharp.BTree.Leafs.Success{T}"/>.
    /// Similarly if success probability is 0 this task is equivalent to the decorator <see cref="Craiel.GDX.AI.Sharp.BTree.Decorators.AlwaysFail{T}"/> and the leaf <see cref="Craiel.GDX.AI.Sharp.BTree.Leafs.Failure{T}"/>.
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class Random<T> : Decorator<T>
        where T : IBlackboard
    {
        private float value;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public Random()
            : this(ConstantFloatDistribution.ZeroPointFive)
        {
        }

        public Random(TaskId task)
            : this(task, ConstantFloatDistribution.ZeroPointFive)
        {
        }

        public Random(FloatDistribution success)
        {
            this.SuccessValue = success;
        }

        public Random(TaskId task, FloatDistribution success)
            : base(task)
        {
            this.SuccessValue = success;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public FloatDistribution SuccessValue { get; private set; }

        public override void Start()
        {
            this.value = this.SuccessValue.NextFloat();

            base.Start();
        }

        public override void Run()
        {
            if (this.Child != TaskId.Invalid)
            {
                this.Stream.CurrentTaskToRun = new BehaviorStream<T>.BehaviorStreamTaskToRun(this.Child, this.Id);
            }
            else
            {
                this.Decide();
            }
        }

        public override void ChildFail(TaskId task)
        {
            this.Decide();
        }

        public override void ChildSuccess(TaskId task)
        {
            this.Decide();
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override void CopyTo(Task<T> clone)
        {
            Random<T> random = (Random<T>)clone;
            random.SuccessValue = this.SuccessValue.Clone<FloatDistribution>();
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void Decide()
        {
            if (GDXAI.Rand.NextDouble() <= this.value)
            {
                this.Success();
            }
            else
            {
                this.Fail();
            }
        }
    }
}

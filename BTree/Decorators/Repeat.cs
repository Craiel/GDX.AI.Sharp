namespace GDX.AI.Sharp.BTree.Decorators
{
    using Contracts;

    using Sharp.Utils.Rnd;

    /// <summary>
    /// A <see cref="Repeat{T}"/> decorator will repeat the wrapped task a certain number of times, possibly infinite. 
    /// This task always succeeds when reaches the specified number of repetitions
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class Repeat<T> : LoopDecorator<T>
        where T : IBlackboard
    {
        private int count;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public Repeat()
        {
        }

        public Repeat(Task<T> child)
            : this(child, ConstantIntegerDistribution.NegativeOne)
        {
        }

        public Repeat(Task<T> child, IntegerDistribution times)
            : base(child)
        {
            this.Times = times;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public IntegerDistribution Times { get; set; }

        public override bool Condition
        {
            get
            {
                return base.Condition && this.count != 0;
            }

            protected set
            {
                base.Condition = value;
            }
        }

        public override void Start()
        {
            this.count = this.Times.NextInt();
        }

        public override void ChildSuccess(Task<T> task)
        {
            if (this.count > 0)
            {
                this.count--;
            }

            if (this.count == 0)
            {
                base.ChildSuccess(task);
                this.Condition = false;
            }
            else
            {
                this.Condition = true;
            }
        }

        public override void ChildFail(Task<T> task)
        {
            this.ChildSuccess(task);
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override void CopyTo(Task<T> clone)
        {
            Repeat<T> repeat = (Repeat<T>)clone;
            repeat.Times = this.Times.Clone<IntegerDistribution>();
        }
    }
}

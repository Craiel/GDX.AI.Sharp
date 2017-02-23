namespace GDX.AI.Sharp.BTree
{
    using Contracts;

    using Enums;

    /// <summary>
    /// <see cref="LoopDecorator{T}"/> is an abstract class providing basic functionalities for concrete looping decorators
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public abstract class LoopDecorator<T> : Decorator<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        protected LoopDecorator()
        {
        }

        protected LoopDecorator(Task<T> child)
            : base(child)
        {
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        /// <summary>
        /// whether the <see cref="LoopDecorator{T}.Run"/> method must keep looping or not.
        /// true if it must keep looping; false otherwise
        /// </summary>
        public virtual bool Condition { get; protected set; }

        public override void Run()
        {
            this.Condition = true;
            while (this.Condition)
            {
                if (this.Child.Status == BTTaskStatus.Running)
                {
                    this.Child.Run();
                }
                else
                {
                    this.Child.SetControl(this);
                    this.Child.Start();
                    if (this.Child.CheckGuard(this))
                    {
                        this.Child.Run();
                    }
                    else
                    {
                        this.Child.Fail();
                    }
                }
            }
        }

        public override void ChildRunning(Task<T> task, Task<T> reporter)
        {
            base.ChildRunning(task, reporter);
            this.Condition = false;
        }
    }
}

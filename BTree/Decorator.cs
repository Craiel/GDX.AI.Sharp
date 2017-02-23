namespace GDX.AI.Sharp.BTree
{
    using System;

    using Contracts;

    using Enums;

    using Exceptions;

    /// <summary>
    /// A <see cref="Decorator{T}"/> is a wrapper that provides custom behavior for its child. The child can be of any kind (branch task, leaf task, or another decorator)
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class Decorator<T> : Task<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------

        /// <summary>
        /// Creates a decorator with no child task
        /// </summary>
        public Decorator()
        {
        }

        /// <summary>
        /// Creates a decorator that wraps the given task
        /// </summary>
        /// <param name="child">the task that will be wrapped</param>
        public Decorator(Task<T> child)
        {
            this.Child = child;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override int ChildCount => this.Child == null ? 0 : 1;

        public override Task<T> GetChild(int index)
        {
            if (index == 0 && this.Child != null)
            {
                return this.Child;
            }

            throw new IndexOutOfRangeException("index invalid or child not set");
        }

        public override void Run()
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

        public override void ChildRunning(Task<T> task, Task<T> reporter)
        {
            this.Running();
        }

        public override void ChildSuccess(Task<T> task)
        {
            this.Success();
        }

        public override void ChildFail(Task<T> task)
        {
            this.Fail();
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------

        /// <summary>
        /// Gets the child task wrapped by this decorator
        /// </summary>
        protected Task<T> Child { get; private set; }

        protected override int AddChildToTask(Task<T> child)
        {
            if (this.Child != null)
            {
                throw new IllegalStateException("A decorator task cannot have more than one child");
            }

            this.Child = child;
            return 0;
        }

        protected override void CopyTo(Task<T> clone)
        {
            if (this.Child != null)
            {
                Decorator<T> decorator = (Decorator<T>)clone;
                decorator.Child = this.Child.Clone();
            }
        }
    }
}

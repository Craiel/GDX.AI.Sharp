namespace GDX.AI.Sharp.BTree
{
    using System.Linq;

    using Contracts;

    using Exceptions;

    using Utils;

    /// <summary>
    /// A <see cref="SingleRunningBranchTask{T}"/> task is a branch task that supports only one running child at a time
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public abstract class SingleRunningBranchTask<T> : BranchTask<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void ChildRunning(Task<T> task, Task<T> reporter)
        {
            this.RunningChild = task;
            this.Running();
        }

        public override void ChildSuccess(Task<T> task)
        {
            this.RunningChild = null;
        }

        public override void ChildFail(Task<T> task)
        {
            this.RunningChild = null;
        }

        public override void Run()
        {
            if (this.RunningChild != null)
            {
                this.RunningChild.Run();
            }
            else
            {
                if (this.CurrentChildIndex >= this.Children.Count)
                {
                    throw new IllegalStateException("Should never happen; this case must be handled by subclasses in childXXX methods");
                }

                if (this.RandomChildren != null)
                {
                    int last = this.Children.Count - 1;
                    if (this.CurrentChildIndex < last)
                    {
                        // Random swap
                        int otherChildIndex = MathUtils.Rnd.Next(this.CurrentChildIndex, last);
                        Task<T> temp = this.RandomChildren[this.CurrentChildIndex];
                        this.RandomChildren[this.CurrentChildIndex] = this.RandomChildren[otherChildIndex];
                        this.RandomChildren[otherChildIndex] = temp;
                    }

                    this.RunningChild = this.RandomChildren[this.CurrentChildIndex];
                }
                else
                {
                    this.RunningChild = this.GetChild(this.CurrentChildIndex);
                }

                this.RunningChild.SetControl(this);
                this.RunningChild.Start();
                if (this.RunningChild.CheckGuard(this))
                {
                    this.RunningChild.Run();
                }
                else
                {
                    this.RunningChild.Fail();
                }
            }
        }

        public override void Start()
        {
            this.CurrentChildIndex = 0;
            this.RunningChild = null;
        }

        public override void Reset()
        {
            base.Reset();

            this.CurrentChildIndex = 0;
            this.RunningChild = null;
            this.RandomChildren = null;
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------

        /// <summary>
        /// The child in the running status or null if no child is running
        /// </summary>
        protected Task<T> RunningChild { get; set; }

        /// <summary>
        /// The index of the child currently processed
        /// </summary>
        protected int CurrentChildIndex { get; set; }

        /// <summary>
        /// Array of random children. If it's {@code null} this task is deterministic
        /// </summary>
        protected Task<T>[] RandomChildren { get; set; }

        protected override void CancelRunningChildren(int startIndex)
        {
            base.CancelRunningChildren(startIndex);

            this.RunningChild = null;
        }

        protected override void CopyTo(Task<T> clone)
        {
            SingleRunningBranchTask<T> branch = (SingleRunningBranchTask<T>)clone;
            branch.RandomChildren = null;

            base.CopyTo(clone);
        }

        protected Task<T>[] CreateRandomChildren()
        {
            return this.Children.ToArray();
        }
    }
}

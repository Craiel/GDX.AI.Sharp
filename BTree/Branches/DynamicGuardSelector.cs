namespace GDX.AI.Sharp.BTree.Branches
{
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    /// <summary>
    /// A <see cref="DynamicGuardSelector{T}"/> is a branch task that executes the first child whose guard is evaluated to <code>false</code>. 
    /// At every AI cycle, the children's guards are re-evaluated, so if the guard of the running child is evaluated to <code>false</code>, 
    /// it is cancelled, and the child with the highest priority starts running.The <see cref="DynamicGuardSelector{T}"/>
    /// task finishes when no guard is evaluated to true (thus failing) or when its active child finishes(returning the active child's termination status)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicGuardSelector<T> : BranchTask<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DynamicGuardSelector() : this(new Task<T>[0])
        {
        }

        public DynamicGuardSelector(IEnumerable<Task<T>> children)
        {
            this.Children = children.ToList();
        }

        public DynamicGuardSelector(params Task<T>[] children)
        {
            this.Children = children.ToList();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        /// <summary>
        /// The child in the running status or null if no child is running
        /// </summary>
        public Task<T> RunningChild { get; protected set; }

        public override void ChildRunning(Task<T> task, Task<T> reporter)
        {
            this.RunningChild = task;
            this.Running();
        }

        public override void ChildSuccess(Task<T> task)
        {
            this.RunningChild = null;
            this.Success();
        }

        public override void ChildFail(Task<T> task)
        {
            this.RunningChild = null;
            this.Fail();
        }

        public override void Run()
        {
            Task<T> childToRun = null;
            for (var i = 0; i < this.Children.Count; i++)
            {
                Task<T> child = this.GetChild(i);
                if (child.CheckGuard(this))
                {
                    childToRun = child;
                    break;
                }
            }

            if (this.RunningChild != null && this.RunningChild != childToRun)
            {
                this.RunningChild.Cancel();
                this.RunningChild = null;
            }

            if (childToRun == null)
            {
                this.Fail();
            }
            else
            {
                if (this.RunningChild == null)
                {
                    this.RunningChild = childToRun;
                    this.RunningChild.SetControl(this);
                    this.RunningChild.Start();
                }

                this.RunningChild.Run();
            }
        }

        public override void Reset()
        {
            base.Reset();
            this.RunningChild = null;
        }
    }
}

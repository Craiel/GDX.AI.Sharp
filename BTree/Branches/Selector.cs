namespace GDX.AI.Sharp.BTree.Branches
{
    using System.Collections.Generic;

    using Contracts;

    /// <summary>
    /// A <see cref="Selector{T}"/> is a branch task that runs every children until one of them succeeds. 
    /// If a child task fails, the selector will start and run the next child task.
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class Selector<T> : SingleRunningBranchTask<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        protected Selector()
        {
        }
        
        protected Selector(IEnumerable<Task<T>> children)
            : base(children)
        {
        }
        
        protected Selector(params Task<T>[] children)
            : base(children)
        {
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void ChildFail(Task<T> task)
        {
            base.ChildFail(task);
            if (++this.CurrentChildIndex < this.Children.Count)
            {
                // Run next child
                this.Run();
            }
            else
            {
                // All children processed, return failure status
                this.Fail();
            }
        }

        public override void ChildSuccess(Task<T> task)
        {
            base.ChildSuccess(task);

            // Return success status when a child says it succeeded
            this.Success();
        }
    }
}

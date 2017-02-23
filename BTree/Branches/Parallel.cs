namespace GDX.AI.Sharp.BTree.Branches
{
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    using Enums;

    /// <summary>
    /// A {@code Parallel} is a special branch task that starts or resumes all children every single time.
    ///  The actual behavior of parallel task depends on its <see cref="ParallelPolicy"/>:
    /// <para></para>
    /// <see cref="PolicySequence"/>: the parallel task fails as soon as one child fails; if all children succeed, then the parallel task succeeds.
    /// This is the default policy.
    /// <para></para>
    /// <see cref="PolicySelector"/>: the parallel task succeeds as soon as one child succeeds; if all children fail, then the parallel task fails.
    /// <para></para>
    /// The typical use case: make the game entity react on event while sleeping or wandering
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class Parallel<T> : BranchTask<T>
        where T : IBlackboard
    {
        private IParallelPolicy policyImplementation;
        private int currentChildIndex;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        protected Parallel(ParallelPolicy policy = ParallelPolicy.Sequence) 
            : this(policy, new Task<T>[0])
        {
        }
        
        protected Parallel(ParallelPolicy policy, IEnumerable<Task<T>> children)
        {
            this.SetPolicy(policy);
            this.Children = children.ToList();
        }
        
        protected Parallel(ParallelPolicy policy, params Task<T>[] children)
        {
            this.SetPolicy(policy);
            this.Children = children.ToList();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public enum ParallelPolicy
        {
            Sequence,
            Selector
        }

        public ParallelPolicy Policy { get; private set; }

        public bool NoRunningTasks { get; private set; }

        public bool? LastResult { get; private set; }

        public override void Run()
        {
            this.NoRunningTasks = true;
            this.LastResult = null;
            for (this.currentChildIndex = 0; this.currentChildIndex < this.Children.Count; this.currentChildIndex++)
            {
                Task<T> child = this.GetChild(this.currentChildIndex);
                if (child.Status == BTTaskStatus.Running)
                {
                    child.Run();
                }
                else
                {
                    child.SetControl(this);
                    child.Start();
                    if (child.CheckGuard(this))
                    {
                        child.Run();
                    }
                    else
                    {
                        child.Fail();
                    }
                }

                if (this.LastResult != null)
                {
                    // Current child has finished either with success or fail
                    this.CancelRunningChildren(this.NoRunningTasks ? this.currentChildIndex + 1 : 0);
                    if (this.LastResult.Value)
                    {
                        this.Success();
                    }
                    else
                    {
                        this.Fail();
                    }

                    return;
                }
            }

            this.Running();
        }

        public override void ChildRunning(Task<T> task, Task<T> reporter)
        {
            this.NoRunningTasks = false;
        }

        public override void ChildSuccess(Task<T> task)
        {
            this.LastResult = this.policyImplementation.ChildSuccess(this);
        }

        public override void ChildFail(Task<T> task)
        {
            this.LastResult = this.policyImplementation.ChildFail(this);
        }

        public override void Reset()
        {
            base.Reset();
            this.NoRunningTasks = true;
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected interface IParallelPolicy
        {
            bool? ChildSuccess(Parallel<T> parallel);
            bool? ChildFail(Parallel<T> parallel);
        }

        protected override void CopyTo(Task<T> clone)
        {
            Parallel<T> parallel = (Parallel<T>)clone;
            parallel.SetPolicy(this.Policy);
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void SetPolicy(ParallelPolicy policy)
        {
            this.Policy = policy;
            switch (policy)
            {
                case ParallelPolicy.Selector:
                    {
                        this.policyImplementation = new PolicySelector();
                        break;
                    }

                case ParallelPolicy.Sequence:
                    {
                        this.policyImplementation = new PolicySequence();
                        break;
                    }
            }
        }

        private class PolicySelector : IParallelPolicy
        {
            public bool? ChildSuccess(Parallel<T> parallel)
            {
                return true;
            }

            public bool? ChildFail(Parallel<T> parallel)
            {
                return parallel.NoRunningTasks && parallel.currentChildIndex == parallel.Children.Count - 1 ? (bool?)false : null;
            }
        }

        private class PolicySequence : IParallelPolicy
        {
            public bool? ChildSuccess(Parallel<T> parallel)
            {
                return parallel.NoRunningTasks && parallel.currentChildIndex == parallel.Children.Count - 1 ? (bool?)true : null;
            }

            public bool? ChildFail(Parallel<T> parallel)
            {
                return false;
            }
        }
    }
}

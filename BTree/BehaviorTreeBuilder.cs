namespace GDX.AI.Sharp.BTree
{
    using System.Collections.Generic;

    using Branches;

    using Contracts;

    using Decorators;

    using Exceptions;

    using Leafs;

    using Sharp.Utils.Rnd;

    /// <summary>
    /// Helper class to build a behavior tree using the fluint API
    /// </summary>
    public class BehaviorTreeBuilder<T>
        where T : IBlackboard
    {
        private readonly Stack<BranchTask<T>> parentStack;
        
        public BehaviorTreeBuilder()
        {
            this.parentStack = new Stack<BranchTask<T>>();
        }

        public BranchTask<T> CurrentParent { get; private set; }

        public Decorator<T> CurrentDecorator { get; private set; }

        public LeafTask<T> CurrentLeaf { get; private set; }

        public BehaviorTreeBuilder<T> Branch(BranchTask<T> task)
        {
            if (this.CurrentDecorator != null)
            {
                // Decorate the branch and close the decorator out
                this.CurrentDecorator.AddChild(task);
                this.CurrentDecorator = null;
            }
            else if (this.parentStack.Count > 0)
            {
                // Add as a sub-branch
                this.parentStack.Peek().AddChild(task);
            }

            this.parentStack.Push(task);
            this.CurrentParent = task;
            this.CurrentLeaf = null;
            return this;
        }

        public BehaviorTreeBuilder<T> DynamicGuardSelector()
        {
            return this.Branch(new DynamicGuardSelector<T>());
        }

        public BehaviorTreeBuilder<T> Parallel()
        {
            return this.Branch(new Parallel<T>());
        }

        public BehaviorTreeBuilder<T> RandomSelector()
        {
            return this.Branch(new RandomSelector<T>());
        }

        public BehaviorTreeBuilder<T> RandomSequence()
        {
            return this.Branch(new RandomSequence<T>());
        }

        public BehaviorTreeBuilder<T> Selector()
        {
            return this.Branch(new Selector<T>());
        }

        public BehaviorTreeBuilder<T> Sequence()
        {
            return this.Branch(new Sequence<T>());
        }

        public BehaviorTreeBuilder<T> Leaf(LeafTask<T> task)
        {
            if (this.CurrentDecorator != null)
            {
                this.CurrentDecorator.AddChild(task);
                this.CurrentDecorator = null;
                this.CurrentLeaf = task;
                return this;
            }

            if (this.CurrentParent == null)
            {
                throw new BehaviorTreeBuilderException("No Root node defined yet, add a branch first");
            }

            this.CurrentParent.AddChild(task);
            this.CurrentLeaf = task;
            return this;
        }

        public BehaviorTreeBuilder<T> Fail()
        {
            return this.Leaf(new Failure<T>());
        }

        public BehaviorTreeBuilder<T> Succeed()
        {
            return this.Leaf(new Success<T>());
        }

        public BehaviorTreeBuilder<T> Wait(float seconds)
        {
            return this.Leaf(new Wait<T>(seconds));
        }

        public BehaviorTreeBuilder<T> Action(ActionDelegate action = null)
        {
            return this.Leaf(new Action<T>(action));
        }

        public BehaviorTreeBuilder<T> Decorator(Decorator<T> task)
        {
            if (this.CurrentParent == null)
            {
                throw new BehaviorTreeBuilderException("No Root node defined yet, add a branch first");
            }

            this.CurrentParent.AddChild(task);

            if (task.ChildCount <= 0)
            {
                // This decorator has no child yet, wait for a fluent set of a leaf node
                this.CurrentDecorator = task;
            }

            this.CurrentLeaf = null;
            return this;
        }

        public BehaviorTreeBuilder<T> AlwaysFail(Task<T> child = null)
        {
            return this.Decorator(new AlwaysFail<T>(child));
        }

        public BehaviorTreeBuilder<T> AlwaysSucceed(Task<T> child = null)
        {
            return this.Decorator(new AlwaysSucceed<T>());
        }

        public BehaviorTreeBuilder<T> Include(string subTree = null, bool lazy = false)
        {
            return this.Decorator(new Include<T>(subTree, lazy));
        }

        public BehaviorTreeBuilder<T> Invert(Task<T> child = null)
        {
            return this.Decorator(new Invert<T>(child));
        }

        public BehaviorTreeBuilder<T> Random(Task<T> child = null)
        {
            return this.Decorator(new Random<T>(child));
        }

        public BehaviorTreeBuilder<T> Repeat(Task<T> child, IntegerDistribution times)
        {
            return this.Decorator(new Repeat<T>(child, times));
        }

        public BehaviorTreeBuilder<T> SemaphoreGuard(Task<T> child, string name = null)
        {
            return this.Decorator(new SemaphoreGuard<T>(name, child));
        }

        public BehaviorTreeBuilder<T> UntilFail(Task<T> child)
        {
            return this.Decorator(new UntilFail<T>(child));
        }

        public BehaviorTreeBuilder<T> UntilSuccess(Task<T> child)
        {
            return this.Decorator(new UntilSuccess<T>(child));
        }

        public BehaviorTreeBuilder<T> End()
        {
            if (this.parentStack.Count <= 1)
            {
                throw new BehaviorTreeBuilderException("End() called on empty stack or root node");
            }

            this.parentStack.Pop();
            this.CurrentParent = this.parentStack.Peek();
            return this;
        }

        public BehaviorTree<T> Build(T blackboard = default(T))
        {
            if (this.parentStack.Count > 1)
            {
                throw new BehaviorTreeBuilderException("Build() called with open parent nodes, you are missing End() calls");
            }

            if (this.CurrentParent == null)
            {
                throw new BehaviorTreeBuilderException("Build() called on empty tree");
            }

            var result = new BehaviorTree<T>(this.CurrentParent);
            result.SetBlackboard(blackboard);

            return result;
        }
    }
}

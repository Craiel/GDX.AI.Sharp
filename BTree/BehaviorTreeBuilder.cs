namespace GDX.AI.Sharp.BTree
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    /// <summary>
    /// Helper class to build a behavior tree using the fluint API
    /// </summary>
    public class BehaviorTreeBuilder<T>
        where T : IBlackboard
    {
        private readonly Stack<Task<T>> parentStack;

        private Task<T> currentNode;

        public BehaviorTreeBuilder()
        {
            this.parentStack = new Stack<Task<T>>();
        }

        public BehaviorTreeBuilder<T> Do(BranchTask<T> task)
        {
            if (this.parentStack.Count > 0)
            {
                this.parentStack.Peek().AddChild(task);
            }
            
            this.parentStack.Push(task);
            return this;
        }

        public BehaviorTreeBuilder<T> Leaf(LeafTask<T> task)
        {
            this.parentStack.Peek().AddChild(task);
            return this;
        }

        public BehaviorTreeBuilder<T> Decorator(Decorator<T> task)
        {
            this.parentStack.Peek().AddChild(task);
            return this;
        }

        public BehaviorTreeBuilder<T> End()
        {
            this.currentNode = this.parentStack.Pop();
            return this;
        }

        public BehaviorTree<T> Build(T blackboard)
        {
            var result = new BehaviorTree<T>(this.currentNode);
            result.SetBlackboard(blackboard);

            return result;
        }
    }
}

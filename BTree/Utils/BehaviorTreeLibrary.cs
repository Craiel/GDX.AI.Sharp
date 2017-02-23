namespace GDX.AI.Sharp.BTree.Utils
{
    using System.Collections.Generic;

    using Contracts;

    public class BehaviorTreeLibrary
    {
        public BehaviorTreeLibrary()
        {
            this.Repository = new Dictionary<string, BehaviorTree<IBlackboard>>();
        }

        public IDictionary<string, BehaviorTree<IBlackboard>> Repository { get; private set; }
    }
}

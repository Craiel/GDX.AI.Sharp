using IBlackboard = Craiel.GDX.AI.Sharp.Contracts.IBlackboard;

namespace Craiel.GDX.AI.Sharp.BTree.Utils
{
    using System.Collections.Generic;

    public class BehaviorTreeLibrary
    {
        public BehaviorTreeLibrary()
        {
            this.Repository = new Dictionary<string, BehaviorStream<IBlackboard>>();
        }

        public IDictionary<string, BehaviorStream<IBlackboard>> Repository { get; private set; }
    }
}

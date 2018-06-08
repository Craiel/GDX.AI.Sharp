using IBlackboard = Craiel.GDX.AI.Sharp.Runtime.Contracts.IBlackboard;

namespace Craiel.GDX.AI.Sharp.Runtime.BTree.Utils
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

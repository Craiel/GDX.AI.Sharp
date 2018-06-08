using IBlackboard = Craiel.GDX.AI.Sharp.Runtime.Contracts.IBlackboard;

namespace Craiel.GDX.AI.Sharp.Runtime.BTree.Branches
{
    using System.Collections.Generic;

    /// <summary>
    /// A <see cref="RandomSelector{T}"/> is a sequence task's variant that runs its children in a random order
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class RandomSelector<T> : Selector<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RandomSelector()
        {
        }

        public RandomSelector(IEnumerable<TaskId> children)
            : base(children)
        {
        }

        public RandomSelector(params TaskId[] children)
            : base(children)
        {
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void Start()
        {
            base.Start();

            if (this.RandomChildren == null)
            {
                this.RandomChildren = this.CreateRandomChildren();
            }
        }
    }
}

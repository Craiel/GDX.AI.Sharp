using BTTaskStatus = Craiel.GDX.AI.Sharp.Runtime.Enums.BTTaskStatus;
using IBlackboard = Craiel.GDX.AI.Sharp.Runtime.Contracts.IBlackboard;

namespace Craiel.GDX.AI.Sharp.Runtime.BTree.Leafs
{
    /// <summary>
    /// <see cref="Failure{T}"/> is a leaf that immediately fails
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class Failure<T> : LeafTask<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override BTTaskStatus Execute()
        {
            return BTTaskStatus.Failed;
        }

        protected override void CopyTo(Task<T> clone)
        {
        }
    }
}

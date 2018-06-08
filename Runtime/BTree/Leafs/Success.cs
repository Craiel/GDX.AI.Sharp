using BTTaskStatus = Craiel.GDX.AI.Sharp.Runtime.Enums.BTTaskStatus;
using IBlackboard = Craiel.GDX.AI.Sharp.Runtime.Contracts.IBlackboard;

namespace Craiel.GDX.AI.Sharp.Runtime.BTree.Leafs
{
    /// <summary>
    /// <see cref="Success{T}"/> is a leaf that immediately succeeds
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class Success<T> : LeafTask<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override BTTaskStatus Execute()
        {
            return BTTaskStatus.Succeeded;
        }

        protected override void CopyTo(Task<T> clone)
        {
        }
    }
}

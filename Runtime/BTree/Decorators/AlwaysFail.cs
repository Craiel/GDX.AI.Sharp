using IBlackboard = Craiel.GDX.AI.Sharp.Runtime.Contracts.IBlackboard;

namespace Craiel.GDX.AI.Sharp.Runtime.BTree.Decorators
{
    /// <summary>
    /// An <see cref="AlwaysFail{T}"/> decorator will fail no matter the wrapped task fails or succeeds
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class AlwaysFail<T> : Decorator<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public AlwaysFail()
        {
        }
        
        public AlwaysFail(TaskId child)
            : base(child)
        {
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void ChildSuccess(TaskId task)
        {
            this.ChildFail(task);
        }
    }
}

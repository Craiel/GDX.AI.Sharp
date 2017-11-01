namespace Assets.Scripts.Craiel.GDX.AI.Sharp.BTree.Decorators
{
    using Assets.Scripts.Craiel.GDX.AI.Sharp.BTree;
    using Contracts;

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

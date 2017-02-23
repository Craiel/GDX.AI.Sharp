namespace GDX.AI.Sharp.Core
{
    using Contracts;

    public sealed class GuardEvaluator<T> : BTTask<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GuardEvaluator()
        {
        }

        public GuardEvaluator(BehaviorTree<T> tree)
        {
            this.Tree = tree;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override int ChildCount => 0;
        
        public override BTTask<T> GetChild(int index)
        {
            return null;
        }

        public override void Run()
        {
        }

        public override void ChildSuccess(BTTask<T> task)
        {
        }

        public override void ChildFail(BTTask<T> task)
        {
        }

        public override void ChildRunning(BTTask<T> task, BTTask<T> reporter)
        {
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override int AddChildToTask(BTTask<T> child)
        {
            return 0;
        }

        protected override void CopyTo(BTTask<T> clone)
        {
        }
    }
}

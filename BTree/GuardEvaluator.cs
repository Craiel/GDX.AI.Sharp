namespace GDX.AI.Sharp.BTree
{
    using Contracts;

    public sealed class GuardEvaluator<T> : Task<T>
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
        
        public override Task<T> GetChild(int index)
        {
            return null;
        }

        public override void Run()
        {
        }

        public override void ChildSuccess(Task<T> task)
        {
        }

        public override void ChildFail(Task<T> task)
        {
        }

        public override void ChildRunning(Task<T> task, Task<T> reporter)
        {
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override int AddChildToTask(Task<T> child)
        {
            return 0;
        }

        protected override void CopyTo(Task<T> clone)
        {
        }
    }
}

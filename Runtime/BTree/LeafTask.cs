using BTTaskStatus = Craiel.GDX.AI.Sharp.Runtime.Enums.BTTaskStatus;
using IBlackboard = Craiel.GDX.AI.Sharp.Runtime.Contracts.IBlackboard;
using IllegalStateException = Craiel.UnityEssentials.Runtime.Exceptions.IllegalStateException;

namespace Craiel.GDX.AI.Sharp.Runtime.BTree
{
    using System;

    /// <summary>
    /// A <see cref="LeafTask{T}"/> is a terminal task of a behavior tree, contains action or condition logic, can not have any child
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public abstract class LeafTask<T> : Task<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override int ChildCount
        {
            get { return 0; }
        }

        /// <summary>
        /// This method contains the update logic of this task. The implementation delegates the <see cref="Execute"/> method
        /// </summary>
        public override void Run()
        {
            BTTaskStatus status = this.Execute();
            switch (status)
            {
                case BTTaskStatus.Succeeded:
                    {
                        this.Success();
                        break;
                    }

                case BTTaskStatus.Failed:
                    {
                        this.Fail();
                        break;
                    }

                case BTTaskStatus.Running:
                    {
                        this.Running();
                        break;
                    }

                default:
                    {
                        throw new IllegalStateException(string.Format("Invalid status '{0}' returned by the execute method", status));
                    }
            }
        }
        
        public override TaskId GetChild(int index)
        {
            throw new IndexOutOfRangeException("A leaf task can not have any child");
        }

        public override void ChildRunning(TaskId task, TaskId reporter)
        {
        }

        public override void ChildSuccess(TaskId task)
        {
        }

        public override void ChildFail(TaskId task)
        {
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------

        /// <summary>
        /// This method contains the update logic of this leaf task. The actual implementation MUST return one of Running,
        /// Succeeded or Failed. Other return values will cause an <see cref="IllegalStateException"/>
        /// </summary>
        /// <returns>the status of this leaf task</returns>
        protected abstract BTTaskStatus Execute();

        protected override int AddChildToTask(TaskId child)
        {
            throw new IllegalStateException("A leaf task cannot have any children");
        }
    }
}

﻿namespace GDX.AI.Sharp.Core
{
    using System;

    using Contracts;

    using Enums;

    using Exceptions;

    /// <summary>
    /// A <see cref="BTLeafTask{T}"/> is a terminal task of a behavior tree, contains action or condition logic, can not have any child
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public abstract class BTLeafTask<T> : BTTask<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override int ChildCount => 0;

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
                        throw new IllegalStateException($"Invalid status '{status}' returned by the execute method");
                    }
            }
        }
        
        public override BTTask<T> GetChild(int index)
        {
            throw new IndexOutOfRangeException("A leaf task can not have any child");
        }

        public override void ChildRunning(BTTask<T> task, BTTask<T> reporter)
        {
        }

        public override void ChildSuccess(BTTask<T> task)
        {
        }

        public override void ChildFail(BTTask<T> task)
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

        protected override int AddChildToTask(BTTask<T> child)
        {
            throw new IllegalStateException("A leaf task cannot have any children");
        }
    }
}

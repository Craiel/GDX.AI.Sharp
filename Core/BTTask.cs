namespace GDX.AI.Sharp.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Contracts;

    using Enums;

    using Exceptions;

    /// <summary>
    /// This is the abstract base class of all behavior tree tasks. The Task of a behavior tree has a status, one control and a list of children.
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.")]
    public abstract class BTTask<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        /// <summary>
        /// Gets the number of children of this task
        /// </summary>
        public abstract int ChildCount { get; }

        /// <summary>
        /// Gets or sets the parent of this task
        /// </summary>
        public BTTask<T> Control { get; protected set; }

        /// <summary>
        /// Gets or sets The guard of this task
        /// </summary>
        public BTTask<T> Guard { get; set; }

        /// <summary>
        /// Gets or sets the behavior tree this task belongs to
        /// </summary>
        public BehaviorTree<T> Tree { get; protected set; }

        /// <summary>
        /// Gets or sets the status of this task
        /// </summary>
        public BTTaskStatus Status { get; protected set; }

        /// <summary>
        /// Gets the blackboard object of the behavior tree this task belongs to
        /// </summary>
        /// <exception cref="IllegalStateException">if this task has never run</exception>
        public T Blackboard
        {
            get
            {
                if (this.Tree == null)
                {
                    throw new IllegalStateException("Task has not ben run");
                }

                return this.Tree.GetBlackboard();
            }
        }

        /// <summary>
        /// This method will add a child to the list of this task's children
        /// </summary>
        /// <param name="child">the child task which will be added</param>
        /// <returns>the index where the child has been added</returns>
        /// <exception cref="IllegalStateException">if the child cannot be added for whatever reason</exception>
        public int AddChild(BTTask<T> child)
        {
            int index = this.AddChildToTask(child);

            if (this.Tree != null && this.Tree.HasListeners)
            {
                this.Tree.NotifyChildAdded(this, index);
            }

            return index;
        }

        /// <summary>
        /// Returns the child at the given index
        /// </summary>
        /// <param name="index">index of the child</param>
        /// <returns>the child task at the specified index</returns>
        public abstract BTTask<T> GetChild(int index);

        /// <summary>
        /// This method will set a task as this task's control (parent)
        /// </summary>
        /// <param name="parent">the parent task</param>
        public void SetControl(BTTask<T> parent)
        {
            this.Control = parent;
            this.Tree = parent.Tree;
        }

        /// <summary>
        /// Checks the guard of this task
        /// </summary>
        /// <param name="control">the parent task</param>
        /// <returns>true if guard evaluation succeeds or there's no guard; false otherwise</returns>
        /// <exception cref="IllegalStateException">if guard evaluation returns any status other than Succeeded or Failed (<see cref="BTTaskStatus"/>)</exception>
        public bool CheckGuard(BTTask<T> control)
        {
            if (this.Guard == null)
            {
                return true;
            }

            // Guard of guard check, recursive
            if (!this.Guard.CheckGuard(control))
            {
                return false;
            }

            this.Guard.SetControl(control.Tree.GuardEvaluator);
            this.Guard.Start();
            this.Guard.Run();
            switch (this.Guard.Status)
            {
                case BTTaskStatus.Succeeded:
                    {
                        return true;
                    }

                case BTTaskStatus.Failed:
                    {
                        return false;
                    }

                default:
                    {
                        throw new IllegalStateException($"Illegal guard status: {this.Guard.Status}. Guards should succeed or fail in one step");
                    }
            }
        }

        /// <summary>
        /// This method will be called once before this task's first run
        /// </summary>
        public virtual void Start()
        {
        }

        /// <summary>
        /// This method will be called by <see cref="Success"/>, <see cref="Fail"/> or <see cref="Cancel"/>, 
        /// meaning that this task's status has just been set to Succeeded, Failed or Cancelled respectively (<see cref="BTTaskStatus"/>)
        /// </summary>
        public virtual void End()
        {
        }

        /// <summary>
        /// This method contains the update logic of this task. The actual implementation MUST call <see cref="Running"/>, <see cref="Success"/> or <see cref="Fail"/> exactly once
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// This method will be called when one of the ancestors of this task needs to run again
        /// </summary>
        /// <param name="task">the task that needs to run again</param>
        /// <param name="reporter">the task that reports, usually one of this task's children</param>
        public abstract void ChildRunning(BTTask<T> task, BTTask<T> reporter);

        /// <summary>
        /// This method will be called when one of the children of this task succeeds
        /// </summary>
        /// <param name="task">the task that succeeded</param>
        public abstract void ChildSuccess(BTTask<T> task);

        /// <summary>
        /// This method will be called when one of the children of this task fails
        /// </summary>
        /// <param name="task">the task that failed</param>
        public abstract void ChildFail(BTTask<T> task);

        /// <summary>
        /// This method will be called in <see cref="Run"/> to inform control that this task needs to run again
        /// </summary>
        public virtual void Running()
        {
            BTTaskStatus previous = this.Status;
            this.Status = BTTaskStatus.Running;
            if (this.Tree.HasListeners)
            {
                this.Tree.NotifyStatusUpdated(this, previous);
            }

            this.Control?.ChildRunning(this, this);
        }

        /// <summary>
        /// This method will be called in <see cref="Run"/> to inform control that this task has finished running with a success result
        /// </summary>
        public virtual void Success()
        {
            BTTaskStatus previous = this.Status;
            this.Status = BTTaskStatus.Succeeded;
            if (this.Tree.HasListeners)
            {
                this.Tree.NotifyStatusUpdated(this, previous);
            }

            this.End();
            this.Control?.ChildSuccess(this);
        }

        /// <summary>
        /// This method will be called in <see cref="Run"/> to inform control that this task has finished running with a failure result
        /// </summary>
        public virtual void Fail()
        {
            BTTaskStatus previous = this.Status;
            this.Status = BTTaskStatus.Failed;
            if (this.Tree.HasListeners)
            {
                this.Tree.NotifyStatusUpdated(this, previous);
            }

            this.End();
            this.Control?.ChildFail(this);
        }

        /// <summary>
        /// Terminates this task and all its running children. This method MUST be called only if this task is running
        /// </summary>
        public virtual void Cancel()
        {
            this.CancelRunningChildren(0);
            BTTaskStatus previous = this.Status;
            this.Status = BTTaskStatus.Canceled;
            if (this.Tree.HasListeners)
            {
                this.Tree.NotifyStatusUpdated(this, previous);
            }

            this.End();
        }

        /// <summary>
        /// Resets this task to make it restart from scratch on next run
        /// </summary>
        public virtual void Reset()
        {
            if (this.Status == BTTaskStatus.Running)
            {
                this.Cancel();
            }

            for (var i = 0; i < this.ChildCount; i++)
            {
                this.GetChild(i).Reset();
            }

            this.Status = BTTaskStatus.Fresh;
            this.Tree = null;
            this.Control = null;
        }

        /// <summary>
        /// Clones this task to a new one. If you don't specify a clone strategy through <see cref="BTTaskCloner"/> the new task is instantiated via reflection and <see cref="CopyTo"/> is invoked
        /// </summary>
        /// <returns>the cloned task</returns>
        /// <exception cref="TaskCloneException">if the task cannot be successfully cloned</exception>
        public BTTask<T> Clone()
        {
            if (BTTaskCloner.Current != null)
            {
                try
                {
                    return BTTaskCloner.Current.CloneTask(this);
                }
                catch (Exception e)
                {
                    throw new TaskCloneException(e);
                }
            }

            try
            {
                BTTask<T> clone = (BTTask<T>)Activator.CreateInstance(this.GetType());
                this.CopyTo(clone);
                clone.Guard = this.Guard?.Clone();
                return clone;
            }
            catch (Exception e)
            {
                throw new TaskCloneException(e);
            }
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------

        /// <summary>
        /// This method will add a child to the list of this task's children
        /// </summary>
        /// <param name="child">the child task which will be added</param>
        /// <returns>the index where the child has been added</returns>
        /// <exception cref="IllegalStateException">if the child cannot be added for whatever reason</exception>
        protected abstract int AddChildToTask(BTTask<T> child);

        /// <summary>
        /// Copies this task to the given task. This method is invoked by CloneTask only if <see cref="BTTaskCloner"/> is null which is its default value
        /// </summary>
        /// <param name="clone">the task to be filled</param>
        protected abstract void CopyTo(BTTask<T> clone);

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------

        /// <summary>
        /// Terminates the running children of this task starting from the specified index up to the end
        /// </summary>
        /// <param name="startIndex"> The start Index. </param>
        private void CancelRunningChildren(int startIndex)
        {
            for (var i = startIndex; i < this.ChildCount; i++)
            {
                BTTask<T> child = this.GetChild(i);
                if (child.Status == BTTaskStatus.Running)
                {
                    child.Cancel();
                }
            }
        }
    }
}

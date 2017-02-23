namespace GDX.AI.Sharp.BTree
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    using Enums;

    using Exceptions;

    /// <summary>
    /// The behavior tree itself
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class BehaviorTree<T> : Task<T>
        where T : IBlackboard
    {
        private readonly IList<IListener<T>> listeners;

        private T blackboard;
        private Task<T> rootTask;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------

        /// <summary>
        /// Creates a <see cref="BehaviorTree{T}"/> with no root task and no blackboard object. Both the root task and the <see cref="IBlackboard"/> object must
        /// be set before running this behavior tree, see <see cref="AddChildToTask"/> and <see cref="SetBlackboard"/> respectively
        /// </summary>
        public BehaviorTree() 
            : this(null, default(T))
        {
        }

        /// <summary>
        /// Creates a <see cref="BehaviorTree{T}"/> with a root task and no blackboard object. Both the root task and the <see cref="IBlackboard"/> object must be set
        /// be set before running this behavior tree, see <see cref="AddChildToTask"/> and <see cref="SetBlackboard"/> respectively
        /// </summary>
        /// <param name="rootTask">the root task of this tree. It can be null</param>
        public BehaviorTree(Task<T> rootTask)
            : this(rootTask, default(T))
        {
        }

        /// <summary>
        /// Creates a <see cref="BehaviorTree{T}"/> with a root task and a blackboard object. Both the root task and the <see cref="IBlackboard"/> object must be set
        /// be set before running this behavior tree, see <see cref="AddChildToTask"/> and <see cref="SetBlackboard"/> respectively
        /// </summary>
        /// <param name="rootTask">the root task of this tree. It can be null</param>
        /// <param name="blackboard">the <see cref="IBlackboard"/>. It can be null</param>
        public BehaviorTree(Task<T> rootTask, T blackboard)
        {
            this.listeners = new List<IListener<T>>();

            this.rootTask = rootTask;
            this.blackboard = blackboard;
            this.Tree = this;
            this.GuardEvaluator = new GuardEvaluator<T>(this);
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override int ChildCount => this.rootTask == null ? 0 : 1;

        public GuardEvaluator<T> GuardEvaluator { get; private set; }

        /// <summary>
        /// Gets if the tree has any listeners
        /// </summary>
        public bool HasListeners { get; private set; }

        /// <summary>
        /// Gets the <see cref="IBlackboard"/> object of this behavior tree
        /// </summary>
        /// <returns>the <see cref="IBlackboard"/> object</returns>
        public T GetBlackboard()
        {
            return this.blackboard;
        }

        /// <summary>
        /// Sets the blackboard object of this behavior tree
        /// </summary>
        /// <param name="newBlackboard">the new <see cref="IBlackboard"/></param>
        public void SetBlackboard(T newBlackboard)
        {
            this.blackboard = newBlackboard;
        }

        public override Task<T> GetChild(int index)
        {
            if (index == 0 && this.rootTask != null)
            {
                return this.rootTask;
            }

            throw new IndexOutOfRangeException("Index for root has to be 0");
        }

        public override void ChildRunning(Task<T> task, Task<T> reporter)
        {
            this.Running();
        }

        public override void ChildFail(Task<T> task)
        {
            this.Fail();
        }

        public override void ChildSuccess(Task<T> task)
        {
            this.Success();
        }

        /// <summary>
        /// This method should be called when game entity needs to make decisions: call this in game loop or after a fixed time slice if
        /// the game is real-time, or on entity's turn if the game is turn-based
        /// </summary>
        public void Step()
        {
            if (this.rootTask.Status == BTTaskStatus.Running)
            {
                this.rootTask.Run();
            }
            else
            {
                this.rootTask.SetControl(this);
                this.rootTask.Start();
                if (this.rootTask.CheckGuard(this))
                {
                    this.rootTask.Run();
                }
                else
                {
                    this.rootTask.Fail();
                }
            }
        }

        public override void Run()
        {
        }

        public override void Reset()
        {
            base.Reset();
            this.Tree = this;
        }

        /// <summary>
        /// Adds a new listener which will receive events defined in <see cref="IListener{T}"/>
        /// </summary>
        /// <param name="listener">the listener to add</param>
        public void AddListener(IListener<T> listener)
        {
            this.listeners.Add(listener);
            this.HasListeners = true;
        }

        /// <summary>
        /// Removes a listener
        /// </summary>
        /// <param name="listener">the listener to remove</param>
        public void RemoveListener(IListener<T> listener)
        {
            this.listeners.Remove(listener);
            this.HasListeners = this.listeners.Count > 0;
        }

        /// <summary>
        /// Removes all listeners from the tree
        /// </summary>
        public void RemoveListeners()
        {
            this.listeners.Clear();
            this.HasListeners = false;
        }

        /// <summary>
        /// Notifies all listeners of a child being added
        /// </summary>
        /// <param name="task">the added task</param>
        /// <param name="index">index the task was added at</param>
        public void NotifyChildAdded(Task<T> task, int index)
        {
            for (var i = 0; i < this.listeners.Count; i++)
            {
                this.listeners[i].ChildAdded(task, index);
            }
        }

        /// <summary>
        /// Notifies all listeners of a task's status getting updated
        /// </summary>
        /// <param name="task">task of which the status changed</param>
        /// <param name="previousState">the previous <see cref="BTTaskStatus"/></param>
        public void NotifyStatusUpdated(Task<T> task, BTTaskStatus previousState)
        {
            for (var i = 0; i < this.listeners.Count; i++)
            {
                this.listeners[i].StatusUpdated(task, previousState);
            }
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------

        /// <summary>
        /// This method will add a child, namely the root, to this behavior tree
        /// </summary>
        /// <param name="child">the root task to add</param>
        /// <returns>the index where the root task has been added (always 0)</returns>
        /// <exception cref="IllegalStateException">if the root task is already set</exception>
        protected override int AddChildToTask(Task<T> child)
        {
            if (this.rootTask != null)
            {
                throw new IllegalStateException("A behavior tree cannot have more than one root task");
            }

            this.rootTask = child;
            return 0;
        }

        protected override void CopyTo(Task<T> clone)
        {
            BehaviorTree<T> target = (BehaviorTree<T>)clone;
            target.rootTask = this.rootTask.Clone();
        }
    }
}

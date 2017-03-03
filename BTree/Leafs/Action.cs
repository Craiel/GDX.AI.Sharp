﻿namespace GDX.AI.Sharp.BTree.Leafs
{
    using Contracts;

    using Enums;

    public delegate bool ActionDelegate();

    /// <summary>
    /// The <see cref="Action{T}"/> leaf runs a delegate and returns Success or Failure depending on the result
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class Action<T> : LeafTask<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public Action()
        {
        }

        public Action(ActionDelegate action)
        {
            this.Delegate = action;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        /// <summary>
        /// The delegate to execute
        /// </summary>
        public ActionDelegate Delegate { get; set; }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override BTTaskStatus Execute()
        {
            if (this.Delegate.Invoke())
            {
                return BTTaskStatus.Succeeded;
            }

            return BTTaskStatus.Failed;
        }

        protected override void CopyTo(Task<T> clone)
        {
            Action<T> action = (Action<T>)clone;
            action.Delegate = this.Delegate;
        }
    }
}
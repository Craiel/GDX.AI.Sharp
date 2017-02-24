﻿namespace GDX.AI.Sharp.BTree.Branches
{
    using System.Collections.Generic;

    using Contracts;

    /// <summary>
    /// A <see cref="RandomSelector{T}"/> is a sequence task's variant that runs its children in a random order
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class RandomSelector<T> : Selector<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RandomSelector()
        {
        }

        public RandomSelector(IEnumerable<Task<T>> children)
            : base(children)
        {
        }

        public RandomSelector(params Task<T>[] children)
            : base(children)
        {
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void Start()
        {
            base.Start();

            if (this.RandomChildren == null)
            {
                this.RandomChildren = this.CreateRandomChildren();
            }
        }
    }
}
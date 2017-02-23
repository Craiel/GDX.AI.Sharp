﻿namespace GDX.AI.Sharp.BTree.Decorators
{
    using Contracts;

    /// <summary>
    /// An <see cref="AlwaysSucceed{T}"/> decorator will succeed no matter the wrapped task succeeds or fails
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class AlwaysSucceed<T> : Decorator<T>
        where T : IBlackboard
    {
        public override void ChildFail(Task<T> task)
        {
            this.ChildSuccess(task);
        }
    }
}

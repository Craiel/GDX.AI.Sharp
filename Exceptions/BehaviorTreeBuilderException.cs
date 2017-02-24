namespace GDX.AI.Sharp.Exceptions
{
    using System;

    using BTree;

    /// <summary>
    /// Thrown when invalid calls are made inside the <see cref="BehaviorTreeBuilder{T}"/>
    /// </summary>
    public class BehaviorTreeBuilderException : Exception
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public BehaviorTreeBuilderException()
        {
        }

        public BehaviorTreeBuilderException(string message)
            : base(message)
        {
        }

        public BehaviorTreeBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

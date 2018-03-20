namespace Craiel.GDX.AI.Sharp.Exceptions
{
    using System;

    public class TaskCloneException : Exception
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public TaskCloneException()
        {
        }

        public TaskCloneException(Exception inner)
            : base(string.Empty, inner)
        {
        }

        public TaskCloneException(string message, Exception inner = null)
            : base(message, inner)
        {
        }
    }
}

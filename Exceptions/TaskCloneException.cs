namespace GDX.AI.Sharp.Exceptions
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
    }
}

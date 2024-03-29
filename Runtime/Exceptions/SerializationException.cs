namespace Craiel.GDX.AI.Sharp.Runtime.Exceptions
{
    using System;

    /// <summary>
    /// Exception thrown when serialization issues occur
    /// </summary>
    public class SerializationException : Exception
    {
        public SerializationException()
        {
        }

        public SerializationException(string message)
            : base(message)
        {
        }

        public SerializationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

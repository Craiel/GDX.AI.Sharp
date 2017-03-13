namespace GDX.AI.Sharp
{
    using System;

    using Contracts;

    /// <summary>
    /// A logger that never logs
    /// </summary>
    public sealed class NullLogger : ILogger
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void Debug(string tag, string message, Exception e = null)
        {
        }

        public void DebugFormat(string tag, string message, params object[] formatArguments)
        {
        }

        public void Info(string tag, string message, Exception e = null)
        {
        }

        public void InfoFormat(string tag, string message, params object[] formatArguments)
        {
        }

        public void Error(string tag, string message, Exception e = null)
        {
        }

        public void ErrorFormat(string tag, string message, params object[] formatArguments)
        {
        }
    }
}

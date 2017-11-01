namespace Assets.Scripts.Craiel.GDX.AI.Sharp.Contracts
{
    using System;

    /// <summary>
    /// The <see cref="ILogger"/> interface provides an abstraction over logging facilities
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="tag">used to identify the source of the log message</param>
        /// <param name="message">the message to log</param>
        /// <param name="e">the exception to log</param>
        void Debug(string tag, string message, Exception e = null);

        /// <summary>
        /// Logs a debug message using string.Format()
        /// </summary>
        /// <param name="tag">used to identify the source of the log message</param>
        /// <param name="message">the message to log</param>
        /// <param name="formatArguments">the arguments for string.format()</param>
        void DebugFormat(string tag, string message, params object[] formatArguments);

        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="tag">used to identify the source of the log message</param>
        /// <param name="message">the message to log</param>
        /// <param name="e">the exception to log</param>
        void Info(string tag, string message, Exception e = null);

        /// <summary>
        /// Logs a info message using string.Format()
        /// </summary>
        /// <param name="tag">used to identify the source of the log message</param>
        /// <param name="message">the message to log</param>
        /// <param name="formatArguments">the arguments for string.format()</param>
        void InfoFormat(string tag, string message, params object[] formatArguments);

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="tag">used to identify the source of the log message</param>
        /// <param name="message">the message to log</param>
        /// <param name="e">the exception to log</param>
        void Error(string tag, string message, Exception e = null);
        
        /// <summary>
        /// Logs a error message using string.Format()
        /// </summary>
        /// <param name="tag">used to identify the source of the log message</param>
        /// <param name="message">the message to log</param>
        /// <param name="formatArguments">the arguments for string.format()</param>
        void ErrorFormat(string tag, string message, params object[] formatArguments);
    }
}

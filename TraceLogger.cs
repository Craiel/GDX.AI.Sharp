namespace GDX.AI.Sharp
{
    using System;

    using Contracts;

    public class TraceLogger : ILogger
    {
        public void Debug(string tag, string message, Exception e = null)
        {
            System.Diagnostics.Debug.WriteLine($"{tag}: {message}");
            if (e != null)
            {
                System.Diagnostics.Debug.Write(e.StackTrace);
            }
        }

        public void DebugFormat(string tag, string message, params object[] formatArguments)
        {
            System.Diagnostics.Debug.WriteLine($"{tag}: {string.Format(message, formatArguments)}");
        }

        public void Info(string tag, string message, Exception e = null)
        {
            System.Diagnostics.Trace.TraceInformation($"{tag}: {message}");
            if (e != null)
            {
                System.Diagnostics.Trace.TraceInformation(e.StackTrace);
            }
        }

        public void InfoFormat(string tag, string message, params object[] formatArguments)
        {
            System.Diagnostics.Trace.TraceInformation($"{tag}: {string.Format(message, formatArguments)}");
        }

        public void Error(string tag, string message, Exception e = null)
        {
            System.Diagnostics.Trace.TraceError($"{tag}: {message}");
            if (e != null)
            {
                System.Diagnostics.Trace.TraceError(e.StackTrace);
            }
        }

        public void ErrorFormat(string tag, string message, params object[] formatArguments)
        {
            System.Diagnostics.Trace.TraceError($"{tag}: {string.Format(message, formatArguments)}");
        }
    }
}

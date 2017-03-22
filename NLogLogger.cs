namespace GDX.AI.Sharp
{
    using System;

    using NLog;

    using ILogger = Contracts.ILogger;

    public class NLogLogger : ILogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Debug(string tag, string message, Exception e = null)
        {
            Logger.Debug($"{tag}: {message}");
            if (e != null)
            {
                Logger.Debug(e.StackTrace);
            }
        }

        public void DebugFormat(string tag, string message, params object[] formatArguments)
        {
            Logger.Debug($"{tag}: {string.Format(message, formatArguments)}");
        }

        public void Info(string tag, string message, Exception e = null)
        {
            Logger.Info($"{tag}: {message}");
            if (e != null)
            {
                Logger.Info(e.StackTrace);
            }
        }

        public void InfoFormat(string tag, string message, params object[] formatArguments)
        {
            Logger.Info($"{tag}: {string.Format(message, formatArguments)}");
        }

        public void Error(string tag, string message, Exception e = null)
        {
            Logger.Error($"{tag}: {message}");
            if (e != null)
            {
                Logger.Error(e.StackTrace);
            }
        }

        public void ErrorFormat(string tag, string message, params object[] formatArguments)
        {
            Logger.Error($"{tag}: {string.Format(message, formatArguments)}");
        }
    }
}

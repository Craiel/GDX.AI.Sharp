namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using System.Collections.Generic;

    using NLog;

    using RecastWrapper;

    public class RecastClientSoloMesh : RecastClient
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ManagedRecastClientSoloMesh typedClient;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RecastClientSoloMesh(RecastClientSettings settings)
        {
            this.typedClient = new ManagedRecastClientSoloMesh(settings.ToManaged());
            this.ManagedClient = this.typedClient;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool LoadObj(string file)
        {
            bool result = this.typedClient.LoadObj(file);

            this.ManagedClient.LogBuildTimes();

            IList<string> buildlogText = this.ManagedClient.GetLogText();
            foreach (string line in buildlogText)
            {
                Logger.Debug(line);
            }

            return result;
        }
    }
}

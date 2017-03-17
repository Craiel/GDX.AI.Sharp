namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using System.Collections.Generic;

    using CarbonCore.Utils.IO;

    using RecastWrapper;

    public class RecastClientSoloMesh : RecastClient
    {
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
        public bool LoadObj(CarbonFile file)
        {
            bool result = this.typedClient.LoadObj(file.GetPath());

            this.ManagedClient.LogBuildTimes();

            IList<string> buildlogText = this.ManagedClient.GetLogText();
            foreach (string line in buildlogText)
            {
                GDXAI.Logger.Debug("RecastClient", line);
            }

            return result;
        }
    }
}

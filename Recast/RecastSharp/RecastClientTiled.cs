namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using System.Collections.Generic;

    using RecastWrapper;

    public class RecastClientTiled : RecastClient
    {
        private readonly ManagedRecastClientTiled typedClient;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RecastClientTiled(RecastClientSettings settings)
        {
            this.typedClient = new ManagedRecastClientTiled(settings.ToManaged());
            this.ManagedClient = this.typedClient;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool GetDebugNavMesh(out byte[] data)
        {
            return this.typedClient.GetDebugNavMesh(out data);
        }

        public bool Load(byte[] data)
        {
            return this.typedClient.Load(data);
        }

        public bool Save(out byte[] data)
        {
            return this.typedClient.Save(out data);
        }

        public bool Generate(string path)
        {
            bool result = this.typedClient.Generate(path);

            this.ManagedClient.LogBuildTimes();

            IList<string> buildlogText = this.ManagedClient.GetLogText();
            foreach (string line in buildlogText)
            {
                GDXAI.Logger.Info("RecastClient", line);
            }

            return result;
        }
    }
}

namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using System.Collections.Generic;

    using CarbonCore.Utils.IO;

    using Contracts;

    using RecastWrapper;

    public class RecastClient
    {
        private readonly ILogger logger;
        private readonly ManagedRecastClient managedClient;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public RecastClient(ILogger logger)
        {
            this.logger = logger;
            this.managedClient = new ManagedRecastClient();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool Load(CarbonFile file)
        {
            bool result = this.managedClient.Load(file.GetPath());
            
            IList<string> buildlogText = this.managedClient.GetLogText();
            foreach (string line in buildlogText)
            {
                this.logger.Debug("RecastClient", line);
            }

            return result;
        }
    }
}

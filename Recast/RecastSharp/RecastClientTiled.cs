namespace GDX.AI.Sharp.Recast.RecastSharp
{
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
        public bool Load(byte[] data)
        {
            return this.typedClient.Load(data);
        }

        public bool Save(out byte[] data)
        {
            return this.typedClient.Save(out data);
        }
    }
}

namespace GDX.AI.Sharp.Spatial
{
    using Microsoft.Xna.Framework;

    public struct OctreeResult<T>
    {
        public readonly T Entry;

        public Vector3 Position;

        public OctreeResult(T entry, Vector3 position)
        {
            this.Entry = entry;
            this.Position = position;
        }
    }
}

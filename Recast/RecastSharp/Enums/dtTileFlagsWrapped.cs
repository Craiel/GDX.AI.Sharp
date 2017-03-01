namespace RecastTest.Enums
{
    using System;

    [Flags]
    public enum dtTileFlagsWrapped
    {
        /// The navigation mesh owns the tile memory and is responsible for freeing it.
        DT_TILE_FREE_DATA = 0x01,
    }
}

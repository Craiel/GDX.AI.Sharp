namespace GDX.AI.Sharp.Recast.RecastSharp.Enums
{
    using System;

    [Flags]
    public enum NavMeshPolyFlag
    {
        Walk = 0x01, // Ability to walk (ground, grass, road)
        Swim = 0x02, // Ability to swim (water).
        Door = 0x04, // Ability to move through doors.
        Jump = 0x08, // Ability to jump.
        Disabled = 0x10, // Disabled polygon
        All = 0xffff // All abilities.
    }
}

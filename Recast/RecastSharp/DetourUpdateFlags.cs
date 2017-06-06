namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using System;

    [Flags]
    public enum DetourUpdateFlags
    {
        AnticipateTurns = 1 << 0,
        ObstacleAvoidance = 1 << 1,
        Separation = 1 << 2,
        OptimizeVis = 1 << 3,
        OptimizeTopo = 1 << 4
    }
}

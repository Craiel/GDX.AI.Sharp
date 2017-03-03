namespace GDX.AI.Sharp.Recast.RecastSharp
{
    using System;

    [Flags]
    public enum DetourUpdateFlags
    {
        AnticipateTurns = 1,
        ObstacleAvoidance = 2,
        Separation = 4,
        OptimizeVis = 8,
        OptimizeTopo = 16
    }
}

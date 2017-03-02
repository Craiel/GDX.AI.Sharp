namespace GDX.AI.Sharp.Recast.RecastSharp.Enums
{
    public enum MoveRequestState
    {
        None = 0,
        Failed,
        Valid,
        Requesting,
        WaitingForQueue,
        WaitingForPath,
        Velocity,
    }
}

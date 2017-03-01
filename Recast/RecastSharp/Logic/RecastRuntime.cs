namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    public class RecastRuntime
    {
        public RecastNavMesh NavMesh { get; set; }

        public DetourNavMesh DetourNavMesh { get; set; }

        public DetourQuery Query { get; set; }
        
        public DetourCrowd Crowd { get; set; }
    }
}

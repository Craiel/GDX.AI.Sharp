namespace GDX.AI.Sharp.Recast.RecastSharp.Logic
{
    using System;

    public class NavMeshBuildException : Exception
    {
        public NavMeshBuildException()
        {
        }

        public NavMeshBuildException(string message) : base(message)
        {
        }

        public NavMeshBuildException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

namespace GDX.AI.Sharp.Geometry
{
    using Microsoft.Xna.Framework;

    public static class VectorExtensions
    {
        public static float[] ToArray(this Vector3 vector)
        {
            var result = new float[3];
            result[0] = vector.X;
            result[1] = vector.Y;
            result[2] = vector.Z;
            return result;
        }
    }
}

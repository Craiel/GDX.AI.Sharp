namespace GDX.AI.Sharp.Utils.Rnd
{
    using Contracts;

    public abstract class FloatDistribution : IDistribution
    {
        public int NextInt()
        {
            return (int)this.NextFloat();
        }

        public long NextLong()
        {
            return (long)this.NextFloat();
        }

        public abstract float NextFloat();

        public double NextDouble()
        {
            return this.NextFloat();
        }
    }
}

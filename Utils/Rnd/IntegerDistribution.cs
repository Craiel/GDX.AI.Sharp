﻿namespace GDX.AI.Sharp.Utils.Rnd
{
    using Contracts;

    public abstract class IntegerDistribution : IDistribution
    {
        public abstract int NextInt();

        public long NextLong()
        {
            return this.NextInt();
        }

        public float NextFloat()
        {
            return this.NextInt();
        }

        public double NextDouble()
        {
            return this.NextInt();
        }
    }
}

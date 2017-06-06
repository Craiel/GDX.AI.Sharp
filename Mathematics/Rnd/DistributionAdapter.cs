namespace GDX.AI.Sharp.Mathematics.Rnd
{
    using System;

    using Contracts;

    using Exceptions;

    public abstract class DistributionAdapter
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        protected DistributionAdapter(string category, Type type)
        {
            this.Category = category;
            this.Type = type;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string Category { get; private set; }

        public Type Type { get; private set; }

        public static double ParseDouble(string value)
        {
            double result;
            if (double.TryParse(value, out result))
            {
                return result;
            }

            throw new DistributionFormatException($"Not a double value: {value}");
        }

        public static float ParseFloat(string value)
        {
            float result;
            if (float.TryParse(value, out result))
            {
                return result;
            }

            throw new DistributionFormatException($"Not a float value: {value}");
        }

        public static int ParseInteger(string value)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }

            throw new DistributionFormatException($"Not a integer value: {value}");
        }

        public static long ParseLong(string value)
        {
            long result;
            if (long.TryParse(value, out result))
            {
                return result;
            }

            throw new DistributionFormatException($"Not a long value: {value}");
        }

        public abstract T ToDistribution<T>(params string[] parameters) where T : IDistribution;

        public abstract string[] ToParameters(IDistribution distribution);
    }
}

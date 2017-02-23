﻿namespace GDX.AI.Sharp.Utils.Rnd
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Contracts;

    public class TriangularIntegerDistribution : IntegerDistribution
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public TriangularIntegerDistribution(int high)
            : this(-high, high)
        {
        }

        public TriangularIntegerDistribution(int low, int high)
            : this(low, high, (low + high) * .5f)
        {
        }

        public TriangularIntegerDistribution(int low, int high, float mode)
        {
            this.Low = low;
            this.High = high;
            this.Mode = mode;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public int Low { get; }

        public int High { get; }

        public float Mode { get; }

        public override int NextInt()
        {
            if (Math.Abs(-this.Low - this.High) < double.Epsilon && Math.Abs(this.Mode) < double.Epsilon)
            {
                // Faster
                return (int)Math.Round(MathUtils.RandomTriangular(this.High));
            }

            return (int)Math.Round(MathUtils.RandomTriangular(this.Low, this.High, this.Mode));
        }

        public override T Clone<T>()
        {
            return (T)(IDistribution)new TriangularIntegerDistribution(this.Low, this.High, this.Mode);
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class TriangularIntegerDistributionAdapter : DistributionAdapters.IntegerAdapter<TriangularIntegerDistribution>
    {
        public TriangularIntegerDistributionAdapter()
            : base("triangular")
        {
        }

        protected override TriangularIntegerDistribution ToDistributionTyped(params string[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    {
                        return new TriangularIntegerDistribution(ParseInteger(parameters[0]));
                    }

                case 2:
                    {
                        return new TriangularIntegerDistribution(ParseInteger(parameters[0]), ParseInteger(parameters[1]));
                    }

                case 3:
                    {
                        return new TriangularIntegerDistribution(ParseInteger(parameters[0]), ParseInteger(parameters[1]), ParseFloat(parameters[2]));
                    }
            }

            throw new ArgumentException("Expected 1-3 parameters");
        }

        protected override string[] ToParametersTyped(TriangularIntegerDistribution distribution)
        {
            return new[] { distribution.Low.ToString(CultureInfo.InvariantCulture), distribution.High.ToString(CultureInfo.InvariantCulture), distribution.Mode.ToString(CultureInfo.InvariantCulture) };
        }
    }
}

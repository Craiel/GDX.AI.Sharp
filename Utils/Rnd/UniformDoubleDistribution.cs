﻿namespace GDX.AI.Sharp.Utils.Rnd
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    public class UniformDoubleDistribution : DoubleDistribution
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public UniformDoubleDistribution(double high)
            : this(0, high)
        {
        }

        public UniformDoubleDistribution(double low, double high)
        {
            this.Low = low;
            this.High = high;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public double Low { get; }

        public double High { get; }

        public override double NextDouble()
        {
            return this.Low + (MathUtils.Rnd.NextDouble() * (this.High - this.Low));
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class UniformDoubleDistributionAdapter : DistributionAdapters.DoubleAdapter<UniformDoubleDistribution>
    {
        public UniformDoubleDistributionAdapter()
            : base("uniform")
        {
        }

        protected override UniformDoubleDistribution ToDistributionTyped(params string[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    {
                        return new UniformDoubleDistribution(ParseDouble(parameters[0]));
                    }

                case 2:
                    {
                        return new UniformDoubleDistribution(ParseDouble(parameters[0]), ParseDouble(parameters[1]));
                    }
            }

            throw new ArgumentException("Expected 1-2 parameters");
        }

        protected override string[] ToParametersTyped(UniformDoubleDistribution distribution)
        {
            return new[] { distribution.Low.ToString(CultureInfo.InvariantCulture), distribution.High.ToString(CultureInfo.InvariantCulture) };
        }
    }
}

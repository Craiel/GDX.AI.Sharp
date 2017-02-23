﻿namespace GDX.AI.Sharp.Utils.Rnd
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    public class GaussianDoubleDistribution : DoubleDistribution
    {
        public static readonly GaussianDoubleDistribution StandardNormal = new GaussianDoubleDistribution(0, 1);

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GaussianDoubleDistribution(double mean, double standardDeviation)
        {
            this.Mean = mean;
            this.StandardDeviation = standardDeviation;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public double Mean { get; }

        public double StandardDeviation { get; }

        public override double NextDouble()
        {
            return this.Mean + MathUtils.NextGaussian() * this.StandardDeviation;
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class GaussianDoubleDistributionAdapter : DistributionAdapters.DoubleAdapter<GaussianDoubleDistribution>
    {
        public GaussianDoubleDistributionAdapter()
            : base("gaussian")
        {
        }

        protected override GaussianDoubleDistribution ToDistributionTyped(params string[] parameters)
        {
            if (parameters.Length != 2)
            {
                throw new ArgumentException("Expected 2 parameters");
            }

            return new GaussianDoubleDistribution(ParseDouble(parameters[0]), ParseDouble(parameters[1]));
        }

        protected override string[] ToParametersTyped(GaussianDoubleDistribution distribution)
        {
            return new[] { distribution.Mean.ToString(CultureInfo.InvariantCulture), distribution.StandardDeviation.ToString(CultureInfo.InvariantCulture) };
        }
    }
}

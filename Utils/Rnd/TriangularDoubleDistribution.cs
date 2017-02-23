namespace GDX.AI.Sharp.Utils.Rnd
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Contracts;

    public class TriangularDoubleDistribution : DoubleDistribution
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public TriangularDoubleDistribution(double high)
            : this(-high, high)
        {
        }

        public TriangularDoubleDistribution(double low, double high)
            : this(low, high, (low + high) * .5)
        {
        }

        public TriangularDoubleDistribution(double low, double high, double mode)
        {
            this.Low = low;
            this.High = high;
            this.Mode = mode;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public double Low { get; }

        public double High { get; }

        public double Mode { get; }

        public override double NextDouble()
        {
            if (Math.Abs(-this.Low - this.High) < double.Epsilon && Math.Abs(this.Mode) < double.Epsilon)
            {
                // Faster
                return MathUtils.RandomTriangular(this.High);
            }

            return MathUtils.RandomTriangular(this.Low, this.High, this.Mode);
        }

        public override T Clone<T>()
        {
            return (T)(IDistribution)new TriangularDoubleDistribution(this.Low, this.High, this.Mode);
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class TriangularDoubleDistributionAdapter : DistributionAdapters.DoubleAdapter<TriangularDoubleDistribution>
    {
        public TriangularDoubleDistributionAdapter()
            : base("triangular")
        {
        }

        protected override TriangularDoubleDistribution ToDistributionTyped(params string[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    {
                        return new TriangularDoubleDistribution(ParseDouble(parameters[0]));
                    }

                case 2:
                    {
                        return new TriangularDoubleDistribution(ParseDouble(parameters[0]), ParseDouble(parameters[1]));
                    }

                case 3:
                    {
                        return new TriangularDoubleDistribution(ParseDouble(parameters[0]), ParseDouble(parameters[1]), ParseDouble(parameters[2]));
                    }
            }

            throw new ArgumentException("Expected 1-3 parameters");
        }

        protected override string[] ToParametersTyped(TriangularDoubleDistribution distribution)
        {
            return new[] { distribution.Low.ToString(CultureInfo.InvariantCulture), distribution.High.ToString(CultureInfo.InvariantCulture), distribution.Mode.ToString(CultureInfo.InvariantCulture) };
        }
    }
}

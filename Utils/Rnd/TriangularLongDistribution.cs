namespace GDX.AI.Sharp.Utils.Rnd
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    public class TriangularLongDistribution : LongDistribution
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public TriangularLongDistribution(long high)
            : this(-high, high)
        {
        }

        public TriangularLongDistribution(long low, long high)
            : this(low, high, (low + high) * .5)
        {
        }

        public TriangularLongDistribution(long low, long high, double mode)
        {
            this.Low = low;
            this.High = high;
            this.Mode = mode;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public long Low { get; }

        public long High { get; }

        public double Mode { get; }

        public override long NextLong()
        {
            if (Math.Abs(-this.Low - this.High) < double.Epsilon && Math.Abs(this.Mode) < double.Epsilon)
            {
                // Faster
                return (long)Math.Round(MathUtils.RandomTriangular(this.High));
            }

            return (long)Math.Round(MathUtils.RandomTriangular(this.Low, this.High, this.Mode));
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class TriangularLongDistributionAdapter : DistributionAdapters.LongAdapter<TriangularLongDistribution>
    {
        public TriangularLongDistributionAdapter()
            : base("triangular")
        {
        }

        protected override TriangularLongDistribution ToDistributionTyped(params string[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    {
                        return new TriangularLongDistribution(ParseLong(parameters[0]));
                    }

                case 2:
                    {
                        return new TriangularLongDistribution(ParseLong(parameters[0]), ParseLong(parameters[1]));
                    }

                case 3:
                    {
                        return new TriangularLongDistribution(ParseLong(parameters[0]), ParseLong(parameters[1]), ParseDouble(parameters[2]));
                    }
            }

            throw new ArgumentException("Expected 1-3 parameters");
        }

        protected override string[] ToParametersTyped(TriangularLongDistribution distribution)
        {
            return new[] { distribution.Low.ToString(CultureInfo.InvariantCulture), distribution.High.ToString(CultureInfo.InvariantCulture), distribution.Mode.ToString(CultureInfo.InvariantCulture) };
        }
    }
}

namespace GDX.AI.Sharp.Utils.Rnd
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Contracts;

    public class UniformIntegerDistribution : IntegerDistribution
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public UniformIntegerDistribution(int high)
            : this(0, high)
        {
        }

        public UniformIntegerDistribution(int low, int high)
        {
            this.Low = low;
            this.High = high;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public int Low { get; }

        public int High { get; }

        public override int NextInt()
        {
            return MathUtils.Rnd.Next(this.Low, this.High);
        }

        public override T Clone<T>()
        {
            return (T)(IDistribution)new UniformIntegerDistribution(this.Low, this.High);
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class UniformIntegerDistributionAdapter : DistributionAdapters.IntegerAdapter<UniformIntegerDistribution>
    {
        public UniformIntegerDistributionAdapter()
            : base("uniform")
        {
        }

        protected override UniformIntegerDistribution ToDistributionTyped(params string[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    {
                        return new UniformIntegerDistribution(ParseInteger(parameters[0]));
                    }

                case 2:
                    {
                        return new UniformIntegerDistribution(ParseInteger(parameters[0]), ParseInteger(parameters[1]));
                    }
            }

            throw new ArgumentException("Expected 1-2 parameters");
        }

        protected override string[] ToParametersTyped(UniformIntegerDistribution distribution)
        {
            return new[] { distribution.Low.ToString(CultureInfo.InvariantCulture), distribution.High.ToString(CultureInfo.InvariantCulture) };
        }
    }
}

namespace GDX.AI.Sharp.Utils.Rnd
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    public sealed class ConstantIntegerDistribution : IntegerDistribution
    {
        public static readonly ConstantIntegerDistribution Zero = new ConstantIntegerDistribution(0);
        public static readonly ConstantIntegerDistribution NegativeOne = new ConstantIntegerDistribution(-1);
        public static readonly ConstantIntegerDistribution One = new ConstantIntegerDistribution(1);

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public ConstantIntegerDistribution(int value)
        {
            this.Value = value;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public int Value { get; }

        public override int NextInt()
        {
            return this.Value;
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class ConstantIntegerDistributionAdapter : DistributionAdapters.IntegerAdapter<ConstantIntegerDistribution>
    {
        public ConstantIntegerDistributionAdapter()
            : base("constant")
        {
        }

        protected override ConstantIntegerDistribution ToDistributionTyped(params string[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new ArgumentException("Expected 1 parameter");
            }

            return new ConstantIntegerDistribution(ParseInteger(parameters[0]));
        }

        protected override string[] ToParametersTyped(ConstantIntegerDistribution distribution)
        {
            return new[] { distribution.Value.ToString(CultureInfo.InvariantCulture) };
        }
    }
}

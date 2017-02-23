namespace GDX.AI.Sharp.Utils.Rnd
{
    public sealed class ConstantFloatDistribution : FloatDistribution
    {
        public static readonly ConstantFloatDistribution Zero = new ConstantFloatDistribution(0);
        public static readonly ConstantFloatDistribution NegativeOne = new ConstantFloatDistribution(-1);
        public static readonly ConstantFloatDistribution One = new ConstantFloatDistribution(1);
        public static readonly ConstantFloatDistribution ZeroPointFive = new ConstantFloatDistribution(.5f);

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public ConstantFloatDistribution(float value)
        {
            this.Value = value;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public float Value { get; }

        public override float NextFloat()
        {
            return this.Value;
        }
    }
}

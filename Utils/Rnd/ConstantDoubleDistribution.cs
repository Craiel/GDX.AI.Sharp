namespace GDX.AI.Sharp.Utils.Rnd
{
    public sealed class ConstantDoubleDistribution : DoubleDistribution
    {
        public static readonly ConstantDoubleDistribution Zero = new ConstantDoubleDistribution(0);
        public static readonly ConstantDoubleDistribution NegativeOne = new ConstantDoubleDistribution(-1);
        public static readonly ConstantDoubleDistribution One = new ConstantDoubleDistribution(1);
        public static readonly ConstantDoubleDistribution ZeroPointFive = new ConstantDoubleDistribution(.5f);

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public ConstantDoubleDistribution(double value)
        {
            this.Value = value;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public double Value { get; }

        public override double NextDouble()
        {
            return this.Value;
        }
    }
}

namespace GDX.AI.Sharp.Utils.Rnd
{
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
}

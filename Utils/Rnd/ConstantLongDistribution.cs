namespace GDX.AI.Sharp.Utils.Rnd
{
    public sealed class ConstantLongDistribution : LongDistribution
    {
        public static readonly ConstantLongDistribution Zero = new ConstantLongDistribution(0);
        public static readonly ConstantLongDistribution NegativeOne = new ConstantLongDistribution(-1);
        public static readonly ConstantLongDistribution One = new ConstantLongDistribution(1);

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public ConstantLongDistribution(long value)
        {
            this.Value = value;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public long Value { get; }

        public override long NextLong()
        {
            return this.Value;
        }
    }
}

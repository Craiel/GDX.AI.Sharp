﻿namespace GDX.AI.Sharp.Utils.Rnd
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

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

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class ConstantLongDistributionAdapter : DistributionAdapters.LongAdapter<ConstantLongDistribution>
    {
        public ConstantLongDistributionAdapter()
            : base("constant")
        {
        }

        protected override ConstantLongDistribution ToDistributionTyped(params string[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new ArgumentException("Expected 1 parameter");
            }

            return new ConstantLongDistribution(ParseLong(parameters[0]));
        }

        protected override string[] ToParametersTyped(ConstantLongDistribution distribution)
        {
            return new[] { distribution.Value.ToString(CultureInfo.InvariantCulture) };
        }
    }
}

namespace GDX.AI.Sharp.Utils
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Math utilities for GDX-AI
    /// </summary>
    public static class MathUtils
    {
        private static double nextNextGaussian;
        private static bool haveNextGaussian;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        static MathUtils()
        {
            // Initialize Random but can be re-seeded if needed
            Rnd = new Random((int)DateTime.Now.Ticks);
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        /// <summary>
        /// The <see cref="Random"/> instance used, re-seed if you want defined behavior
        /// </summary>
        public static Random Rnd { get; set; }

        /// <summary>
        /// <para>Returns the next pseudorandom, Gaussian ("normally") distributed double value with mean 0.0 and standard deviation 1.0 from this random number generator's sequence.</para>
        /// <para>The general contract of <see cref="NextGaussian"/> is that one double value, chosen from(approximately) the usual
        /// normal distribution with mean 0.0 and standard deviation 1.0, is pseudorandomly generated and returned.</para>
        /// <para>The method <see cref="NextGaussian"/> is implemented by class <see cref="Random"/> as if by a threadsafe version of the following:
        /// <code>
        /// public double nextGaussian()
        /// {
        ///     if (haveNextNextGaussian)
        ///     {
        ///         haveNextNextGaussian = false;
        ///         return nextNextGaussian;
        ///     }
        /// 
        ///     double v1, v2, s;
        ///     do
        ///     {
        ///         v1 = 2 * nextDouble() - 1;   // between -1.0 and 1.0
        ///         v2 = 2 * nextDouble() - 1;   // between -1.0 and 1.0
        ///         s = v1 * v1 + v2 * v2;
        ///      } while (s >= 1 || s == 0);
        /// 
        ///      double multiplier = StrictMath.sqrt(-2 * StrictMath.log(s) / s);
        ///      nextNextGaussian = v2 * multiplier;
        ///      haveNextNextGaussian = true;
        ///      return v1 * multiplier;
        /// }
        /// </code></para>
        ///  <para>This uses the<i>polar method</i> of G. E.P.Box, M.E.Muller, and
        ///  G. Marsaglia, as described by Donald E. Knuth in <i>The Art of
        ///  Computer Programming</i>, Volume 3: <i>Seminumerical Algorithms</i>,
        ///  section 3.4.1, subsection C, algorithm P. Note that it generates two
        ///  independent values at the cost of only one call to <see cref="Math.Log"/>
        ///  and one call to <see cref="Math.Sqrt"/>.</para>
        /// </summary>
        /// <returns>the next pseudorandom, Gaussian("normally") distributed double value with mean 0.0 and standard deviation 1.0 from this random number generator's sequence</returns>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Reviewed. Suppression is OK here.")]
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static double NextGaussian()
        {
            // See Knuth, ACP, Section 3.4.1 Algorithm C.
            if (haveNextGaussian)
            {
                haveNextGaussian = false;
                return nextNextGaussian;
            }

            double v1, v2, s;
            do
            {
                v1 = 2 * Rnd.NextDouble() - 1; // between -1 and 1
                v2 = 2 * Rnd.NextDouble() - 1; // between -1 and 1
                s = v1 * v1 + v2 * v2;
            }
            while (s >= 1 || Math.Abs(s) < double.Epsilon);

            double multiplier = Math.Sqrt(-2 * Math.Log(s) / s);
            nextNextGaussian = v2 * multiplier;
            haveNextGaussian = true;
            return v1 * multiplier;
        }

        public static double RandomTriangular(double high)
        {
            return (Rnd.NextDouble() - Rnd.NextDouble()) * high;
        }

        public static double RandomTriangular(double low, double high, double mode)
        {
            double u = Rnd.NextDouble();
            double d = high - low;
            if (u <= (mode - low) / d)
            {
                return low + Math.Sqrt(u * d * (mode - low));
            }

            return high - Math.Sqrt((1 - u) * d * (high - mode));
        }

        public static float RandomTriangular(float high)
        {
            return ((float)Rnd.NextDouble() - (float)Rnd.NextDouble()) * high;
        }

        public static float RandomTriangular(float low, float high, float mode)
        {
            float u = (float)Rnd.NextDouble();
            float d = high - low;
            if (u <= (mode - low) / d)
            {
                return low + (float)Math.Sqrt(u * d * (mode - low));
            }

            return high - (float)Math.Sqrt((1 - u) * d * (high - mode));
        }
    }
}

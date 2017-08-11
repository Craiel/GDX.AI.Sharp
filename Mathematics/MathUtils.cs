namespace GDX.AI.Sharp.Mathematics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Math utilities for GDX-AI
    /// </summary>
    public static class MathUtils
    {
        // Largest and lowest safe numbers to do comparisons on, c# gets inaccurate after
        public const float MinFloat = 1 - (1 << 24);
        public const float MaxFloat = (1 << 24) - 1;
        public const double MaxDouble = 9007199254740991;
        public const double MinDouble = 1 - MaxDouble;

        public static float Epsilon;

        public static double DoubleEpsilon;

        private static double nextNextGaussian;
        private static bool haveNextGaussian;
        
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        static MathUtils()
        {
            UpdateEpsilon();
            UpdateDoubleEpsilon();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        /// <summary>
        /// Recalculates the epsilon value for floats
        /// See https://www.johndcook.com/blog/2010/06/08/c-math-gotchas/ for the specific reason why this exists
        /// </summary>
        /// <param name="center">the center to calculate for, higher = less precision</param>
        public static void UpdateEpsilon(float center = 1.0f)
        {
            Epsilon = 1f;
            while (Epsilon + center > center)
            {
                Epsilon /= 2f;
            }
        }
        
        /// <summary>
        /// Recalculates the epsilon value for floats
        /// See https://www.johndcook.com/blog/2010/06/08/c-math-gotchas/ for the specific reason why this exists
        /// </summary>
        /// <param name="center">the center to calculate for, higher = less precision</param>
        public static void UpdateDoubleEpsilon(double center = 1.0d)
        {
            DoubleEpsilon = 1d;
            while (DoubleEpsilon + center > center)
            {
                DoubleEpsilon /= 2d;
            }
        }

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
                v1 = 2 * GDXAI.Rand.NextDouble() - 1; // between -1 and 1
                v2 = 2 * GDXAI.Rand.NextDouble() - 1; // between -1 and 1
                s = v1 * v1 + v2 * v2;
            }
            while (s >= 1 || Math.Abs(s) < DoubleEpsilon);

            double multiplier = Math.Sqrt(-2 * Math.Log(s) / s);
            nextNextGaussian = v2 * multiplier;
            haveNextGaussian = true;
            return v1 * multiplier;
        }

        public static double RandomTriangular(double high)
        {
            return (GDXAI.Rand.NextDouble() - GDXAI.Rand.NextDouble()) * high;
        }

        public static double RandomTriangular(double low, double high, double mode)
        {
            double u = GDXAI.Rand.NextDouble();
            double d = high - low;
            if (u <= (mode - low) / d)
            {
                return low + Math.Sqrt(u * d * (mode - low));
            }

            return high - Math.Sqrt((1 - u) * d * (high - mode));
        }

        public static float RandomTriangular(float high)
        {
            return ((float)GDXAI.Rand.NextDouble() - (float)GDXAI.Rand.NextDouble()) * high;
        }

        public static float RandomTriangular(float low, float high, float mode)
        {
            float u = (float)GDXAI.Rand.NextDouble();
            float d = high - low;
            if (u <= (mode - low) / d)
            {
                return low + (float)Math.Sqrt(u * d * (mode - low));
            }

            return high - (float)Math.Sqrt((1 - u) * d * (high - mode));
        }

        /// <summary>
        /// Returns the specified value if the value is already a power of two
        /// </summary>
        /// <param name="value">the value to start at</param>
        /// <returns>the next power of two</returns>
        public static int NextPowerOfTwo(int value)
        {
            if (value == 0)
            {
                return 1;
            }

            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }

        public static int NumberOfTrailingZeros(this int i)
        {
            // HD, Figure 5-14
            int y;
            if (i == 0)
            {
                return 32;
            }

            int n = 31;
            y = i << 16; if (y != 0) { n = n - 16; i = y; }
            y = i << 8; if (y != 0) { n = n - 8; i = y; }
            y = i << 4; if (y != 0) { n = n - 4; i = y; }
            y = i << 2; if (y != 0) { n = n - 2; i = y; }
            return n - ((i << 1) >> 31);
        }

        public static Vector3 WithMaxPrecision(this Vector3 source, int precision)
        {
            return new Vector3((float)Math.Round(source.X, precision), (float)Math.Round(source.Y, precision), (float)Math.Round(source.Z, precision));
        }

        public static float Max(params float[] values)
        {
            float result = float.MinValue;
            for (var i = 0; i < values.Length; i++)
            {
                if (result < values[i])
                {
                    result = values[i];
                }
            }

            return result;
        }
    }
}

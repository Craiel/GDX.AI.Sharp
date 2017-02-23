namespace GDX.AI.Sharp.Utils
{
    using System;

    /// <summary>
    /// Math utilities for GDX-AI
    /// </summary>
    public static class MathUtils
    {
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
    }
}

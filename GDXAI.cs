namespace GDX.AI.Sharp
{
    using System;

    using Contracts;

    /// <summary>
    /// Environment class holding references to the {@link Timepiece}, {@link Logger} and {@link FileSystem} instances. The references
    /// are held in static fields which allows static access to all sub systems. 
    /// Basically, this class is the locator of the service locator design pattern.The locator contains references to the services and
    /// encapsulates the logic that locates them.Being a decoupling pattern, the service locator provides a global point of access to
    /// a set of services without coupling users to the concrete classes that implement them.
    /// </summary>
    public static class GDXAI
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        static GDXAI()
        {
            TimePiece = new DefaultTimePiece();
            
#if TRACE
            // If we have tracing enabled use the trace logger
            Logger = new TraceLogger();
#else
            Logger = new NullLogger();
#endif

            // Initialize Random but can be re-seeded if needed
            Rand = new Random((int)DateTime.Now.Ticks);
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static ITimePiece TimePiece { get; set; }

        public static ILogger Logger { get; set; }

        public static IFileSystem FileSystem { get; set; }

        /// <summary>
        /// The <see cref="Random"/> instance used, re-seed if you want defined behavior
        /// </summary>
        public static Random Rand { get; set; }
    }
}

namespace GDX.AI.Sharp.Core
{
    using Contracts;

    /// <summary>
    /// Static holder of the Task Cloner settings
    /// </summary>
    public static class BTTaskCloner
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the clone strategy (if any) that <see cref="BTTask{T}.Clone"/> will use. Defaults to null, meaning that <see cref="BTTask{T}.CopyTo"/> is used instead.
        /// In this case, properly overriding this method in each task is developer's responsibility
        /// </summary>
        public static IBTTaskCloner Current { get; set; }
    }
}

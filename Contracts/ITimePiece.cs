namespace Craiel.GDX.AI.Sharp.Contracts
{
    using BTree.Leafs;

    /// <summary>
    /// The <see cref="ITimePiece"/> is the AI clock which gives you the current time and the last delta time i.e., the time span between the
    /// current frame and the last frame in seconds. This is the only service provider that does not depend on the environment.
    /// It is needed because some parts of GDX.AI.Sharp (like for instance <see cref="MessageDispatcher"/>, <see cref="Jump"/> steering
    /// behavior and <see cref="Wait{T}"/> task) have a notion of spent time and we want to support game pause.It's developer's responsibility
    /// to update the timepiece on each game loop.When the game is paused you simply don't update the timepiece.
    /// </summary>
    public interface ITimePiece
    {
        /// <summary>
        /// The tick of the current frame, used to identify loops and issues in btree's
        /// </summary>
        ulong Tick { get; }

        /// <summary>
        /// Returns the time accumulated up to the current frame in seconds
        /// </summary>
        float Time { get; }

        /// <summary>
        /// Returns the time span between the current frame and the last frame in seconds
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        /// Updates this timepiece with the given delta time
        /// </summary>
        /// <param name="delta">the time in seconds since the last frame</param>
        void Update(float delta);
    }
}

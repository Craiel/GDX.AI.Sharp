namespace GDX.AI.Sharp.Contracts
{
    using Core;

    /// <summary>
    /// A <see cref="IBTTaskCloner"/> allows you to use third-party libraries to clone behavior trees. See <see cref="BTTaskCloner.Current"/>
    /// </summary>
    public interface IBTTaskCloner
    {
        /// <summary>
        /// Makes a deep copy of the given task
        /// </summary>
        /// <typeparam name="T">type of the blackboard object</typeparam>
        /// <param name="task">the task to clone</param>
        /// <returns>the cloned task</returns>
        BTTask<T> CloneTask<T>(BTTask<T> task) where T : IBlackboard;
    }
}

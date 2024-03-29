namespace Craiel.GDX.AI.Sharp.Runtime.Contracts
{
    using BTree;

    /// <summary>
    /// A <see cref="ITaskCloner"/> allows you to use third-party libraries to clone behavior trees. See <see cref="TaskCloner.Current"/>
    /// </summary>
    public interface ITaskCloner
    {
        /// <summary>
        /// Makes a deep copy of the given task
        /// </summary>
        /// <typeparam name="T">type of the blackboard object</typeparam>
        /// <param name="task">the task to clone</param>
        /// <returns>the cloned task</returns>
        Task<T> CloneTask<T>(Task<T> task) where T : IBlackboard;
    }
}

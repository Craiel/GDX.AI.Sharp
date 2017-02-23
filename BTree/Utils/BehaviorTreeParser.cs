namespace GDX.AI.Sharp.BTree.Utils
{
    using Contracts;

    /// <summary>
    /// A <see cref="BehaviorTree{T}"/> Parser
    /// </summary>
    /// <typeparam name="T">type of the blackboard object that tasks use to read or modify game state</typeparam>
    public class BehaviorTreeParser<T>
        where T : IBlackboard
    {
        public enum DebugLevel
        {
            None = 0,
            Low = 1,
            High = 2
        }

        public DebugLevel Debug { get; set; }
    }
}

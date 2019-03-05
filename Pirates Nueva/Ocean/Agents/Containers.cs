using Pirates_Nueva.Path;

namespace Pirates_Nueva.Ocean.Agents
{
    /// <summary>
    /// An object that can contain Agents and Jobs.
    /// </summary>
    /// <typeparam name="TSelf">The type that is implementing this container.</typeparam>
    /// <typeparam name="TSpot">The type of spot that this instance contains.</typeparam>
    public interface IAgentContainer<TSelf, TSpot> : IGraph<TSpot>
        where TSelf : class, IAgentContainer<TSelf, TSpot>
        where TSpot : class, IAgentSpot<TSpot>
    {
        /// <summary>
        /// Gets a <see cref="Job"/> that can currently be worked on by the specified <see cref="Agent{TC, TSpot}"/>
        /// </summary>
        Job<TSelf, TSpot> GetWorkableJob(Agent<TSelf, TSpot> hiree);
        /// <summary>
        /// Removes the specified <see cref="Job"/> from this instance.
        /// </summary>
        void RemoveJob(Job<TSelf, TSpot> job);
    }
    /// <summary>
    /// An object that an Agent can stand on.
    /// </summary>
    /// <typeparam name="T">The type that is implementing this interface.</typeparam>
    public interface IAgentSpot<T> : INode<T>
        where T : IAgentSpot<T>
    {
        int X { get; }
        int Y { get; }
        PointI Index { get; }
    }
}

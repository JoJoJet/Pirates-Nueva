using System;
using Pirates_Nueva.Path;

namespace Pirates_Nueva.Ocean.Agents
{
    /// <summary>
    /// An object that can contain Agents and Jobs.
    /// </summary>
    /// <typeparam name="TSelf">The type that is implementing this container.</typeparam>
    /// <typeparam name="TSpot">The type of spot that this instance contains.</typeparam>
    public interface IAgentContainer<TSelf, TSpot> : IGraph<TSpot>, ISpaceLocus
        where TSelf : class, IAgentContainer<TSelf, TSpot>
        where TSpot : class, IAgentSpot<TSelf, TSpot>
    {
        /// <summary>
        /// Gets a <see cref="Job"/> that can currently be worked on by the specified <see cref="Agent{TC, TSpot}"/>
        /// </summary>
        Job<TSelf, TSpot>? GetWorkableJob(Agent<TSelf, TSpot> hiree);
        /// <summary>
        /// Removes the specified <see cref="Job"/> from this instance.
        /// </summary>
        void RemoveJob(Job<TSelf, TSpot> job);
    }

    /// <summary>
    /// An object that an Agent can stand on.
    /// </summary>
    /// <typeparam name="TC">The container type.</typeparam>
    /// <typeparam name="TSelf">The type that is implementing this interface</typeparam>
    public interface IAgentSpot<TC, TSelf> : INode<TSelf>
        where TC    : class, IAgentContainer<TC, TSelf>
        where TSelf : class, IAgentSpot<TC, TSelf>
    {
        int X { get; }
        int Y { get; }
        PointI Index { get; }
        Stock<TC, TSelf>? Stock { get; set; }
    }
    /// <summary>
    /// An <see cref="IAgentSpot{TC, TSelf}"/> that can be destroyed.
    /// <para />
    /// The blocks of a <see cref="Ship"/> might be destroyable,
    /// whereas the blocks of an <see cref="Island"/> might not be.
    /// </summary>
    /// <typeparam name="TC">The container type.</typeparam>
    /// <typeparam name="TSelf">The type that is implementing this interface.</typeparam>
    public interface IAgentSpotDestroyable<TC, TSelf> : IAgentSpot<TC, TSelf>
        where TC    : class, IAgentContainer<TC, TSelf>
        where TSelf : class, IAgentSpot<TC, TSelf>
    {
        /// <summary> Whether or not this Spot has been destroyed. </summary>
        bool IsDestroyed { get; }

        /// <summary> Subscribes the specified function to be invoked if this Spot is destroyed. </summary>
        void SubscribeOnDestroyed(Action<TSelf> action);
        /// <summary> Unsubscribes the specified <see cref="Action"/> from being invoked if this Spot is destroyed. </summary>
        void UnsubscribeOnDestroyed(Action<TSelf> action);
    }

    public static class AgentSpotExt
    {
        /// <summary> Returns whether or not the current Spot has been destroyed. </summary>
        public static bool IsDestroyed<TC, TSpot>(this IAgentSpot<TC, TSpot> spot)
            where TC    : class, IAgentContainer<TC, TSpot>
            where TSpot : class, IAgentSpot<TC, TSpot>
            => (spot as IAgentSpotDestroyable<TC, TSpot>)?.IsDestroyed ?? false;

        /// <summary> Subscribes the specified function to be invoked if this Spot is destroyed. </summary>
        public static void SubscribeOnDestroyed<TC, TSpot>(this IAgentSpot<TC, TSpot> spot, Action<TSpot> action)
            where TC    : class, IAgentContainer<TC, TSpot>
            where TSpot : class, IAgentSpot<TC, TSpot>
            => (spot as IAgentSpotDestroyable<TC, TSpot>)?.SubscribeOnDestroyed(action);
        /// <summary> Unsubscribes the specified <see cref="Action"/> from being invoked if this Spot is destroyed. </summary>
        public static void UnsubscribeOnDestroyed<TC, TSpot>(this IAgentSpot<TC, TSpot> spot, Action<TSpot> action)
            where TC    : class, IAgentContainer<TC, TSpot>
            where TSpot : class, IAgentSpot<TC, TSpot>
            => (spot as IAgentSpotDestroyable<TC, TSpot>)?.UnsubscribeOnDestroyed(action);
    }
}

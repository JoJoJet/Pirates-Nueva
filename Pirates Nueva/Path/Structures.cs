﻿using System.Collections.Generic;

namespace Pirates_Nueva.Path
{
    /// <summary>
    /// A pathfinding graph.
    /// </summary>
    public interface IGraph<T>
        where T : INode<T>
    {
        /// <summary> Every Node in this pathfinding graph. </summary>
        IEnumerable<T> Nodes { get; }
    }
    /// <summary>
    /// Part of a pathfinding graph.
    /// </summary>
    /// <typeparam name="T">The type implementing <see cref="INode{T}"/></typeparam>
    public interface INode<T>
        where T : INode<T>
    {
        /// <summary> The edges moving FROM this <see cref="INode{T}"/>. </summary>
        IEnumerable<Edge<T>> Edges { get; }
    }
    /// <summary>
    /// End edge between two Nodes.
    /// </summary>
    public struct Edge<T>
        where T : INode<T>
    {
        /// <summary> The cost to move along this <see cref="Edge{T}"/> in euclidean space. </summary>
        public float Cost { get; }

        /// <summary> The node that this <see cref="Edge{T}"/> connects to. </summary>
        public T End { get; }

        /// <summary>
        /// Create an <see cref="Edge{T}"/> connecting to the specified <see cref="INode{T}"/> and with specified <see cref="Cost"/>.
        /// </summary>
        public Edge(float cost, T end) {
            Cost = cost;
            End = end;
        }
    }
}

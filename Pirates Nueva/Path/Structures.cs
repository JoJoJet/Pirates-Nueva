using System.Collections.Generic;

namespace Pirates_Nueva.Path
{
    /// <summary>
    /// A pathfinding graph.
    /// </summary>
    public interface IGraph<T>
    {
        /// <summary> Every <see cref="INode"/> in this pathfinding graph. </summary>
        IEnumerable<INode<T>> Nodes { get; }
    }
    /// <summary>
    /// Part of a pathfinding graph.
    /// </summary>
    public interface INode<T>
    {
        /// <summary> The edges moving FROM this <see cref="INode"/>. </summary>
        IEnumerable<Edge<T>> Edges { get; }
    }
    /// <summary>
    /// End edge between two <see cref="INode"/>s.
    /// </summary>
    public struct Edge<T>
    {
        /// <summary> The squared cost to move along this <see cref="Edge"/>. </summary>
        public float SqrCost { get; }

        /// <summary> The node that this <see cref="Edge"/> connects to. </summary>
        public INode<T> End { get; }

        public Edge(float sqrCost, INode<T> end) {
            SqrCost = sqrCost;
            End = end;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva.Pathfinding
{
    /// <summary>
    /// A pathfinding graph.
    /// </summary>
    public interface IGraph
    {
        /// <summary> Every <see cref="INode"/> in this pathfinding graph. </summary>
        INode[] Nodes { get; }
    }
    /// <summary>
    /// Part of a pathfinding graph.
    /// </summary>
    public interface INode
    {
        /// <summary> The edges moving FROM this <see cref="INode"/>. </summary>
        Edge[] Edges { get; }
    }
    /// <summary>
    /// End edge between two <see cref="INode"/>s.
    /// </summary>
    public struct Edge
    {
        /// <summary> The cost to move along this <see cref="Edge"/>. </summary>
        public float Cost { get; }

        /// <summary> The node that this <see cref="Edge"/> connects to. </summary>
        public INode End { get; }

        public Edge(float cost, INode end) {
            Cost = cost;
            End = end;
        }
    }
}

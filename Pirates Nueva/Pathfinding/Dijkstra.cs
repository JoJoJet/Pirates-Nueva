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
        /// <summary> The squared cost to move along this <see cref="Edge"/>. </summary>
        public float SqrCost { get; }

        /// <summary> The node that this <see cref="Edge"/> connects to. </summary>
        public INode End { get; }

        public Edge(float sqrCost, INode end) {
            SqrCost = sqrCost;
            End = end;
        }
    }
    /// <summary>
    /// Pathfinding with Dijkstra'a algorithm.
    /// <para />
    /// https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm.
    /// </summary>
    public static class Dijkstra
    {
        public static Stack<INode> FindPath(IGraph graph, INode source, INode target) {
            var dist = new Dictionary<INode, float>(); // The squared distance from /source/ for each node.
            var prev = new Dictionary<INode, INode>(); // The node before the key in the optimal path to /source/.
            var Q = new List<INode>();                 // Set of unvisited nodes.

            foreach(var v in graph.Nodes) {
                dist[v] = float.MaxValue;              // Unkown distance from /source/ to /v/.
                Q.Add(v);                              // Add /v/ to the list of unvisited nodes
            }

            dist[source] = 0;                          // Distance from /source/ to itself is always 0;

            while(Q.Count > 0) {                       // While there are still unvisited nodes:
                var u = (from n in Q                   // Node with least distance from /source/.
                         orderby dist[n] ascending
                         select n).First();

                Q.Remove(u);                           // Remove /u/ from /Q/.

                if(u == target)                        // If we are at /target/, we can stop constructing the tree.
                    break;

                var neighbors = from n in u.Edges      // Every neighbor of /u/ that is unvisited.
                                where Q.Contains(n.End)
                                select n;
                foreach(var v in neighbors) {          // For every unvisited neighbor:
                    var alt = dist[u] + v.SqrCost;     //     The distance from /source/ to /v/, going through /u/.
                    if(alt < dist[v.End]) {            //     If the above distance is a shorter than dist[v], we found a shorter path.
                        dist[v.End] = alt;             //         update dist[v]
                        prev[v.End] = u;               //         set a new previous node for /v/.
                    }
                }
            }

            return reverse_iterate();                  // Construct the optimal path by reverse
                                                       // iterating through /prev/ from /target/.
            
            Stack<INode> reverse_iterate() {
                var S = new Stack<INode>();                       // Empty sequence.
                var u = target;
                if(prev.ContainsKey(u) || u == source) {          // If the vertex is reachable:
                    while(u != null) {                            // Construct the shortest path
                        S.Push(u);                                //     push the vertex onto the stack
                        u = prev.ContainsKey(u) ? prev[u] : null; //     set /u/ as the previous node in the optimal path.
                    }                                             //
                    return S;                                     // Return the path we have constructed.
                }
                else {                                            // If the vertex is NOT reachable:
                    return S;                                     // Return an empty stack; let the caller figure it out
                }
            }
        }
    }
}

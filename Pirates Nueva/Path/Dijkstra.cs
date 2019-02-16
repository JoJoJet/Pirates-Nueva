using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva.Path
{
    /// <summary>
    /// Pathfinding with Dijkstra'a algorithm.
    /// <para />
    /// https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm.
    /// </summary>
    public static class Dijkstra
    {
        /// <summary>
        /// Find the shortest distance between nodes /source/ and /target/ on the specified graph.
        /// </summary>
        public static Stack<T> FindPath<T>(IGraph<T> graph, INode<T> source, INode<T> target) where T : INode<T> {
            var dist = new Dictionary<INode<T>, float>();    // The squared distance from /source/ for each node.
            var prev = new Dictionary<INode<T>, INode<T>>(); // The node before the key in the optimal path to /source/.
            var Q = new List<INode<T>>();                    // Set of unvisited nodes.

            foreach(var v in graph.Nodes) { // For every node in the graph:
                dist[v] = float.MaxValue;   // Unkown distance from /source/ to the node.
                Q.Add(v);                   // Add it to the list of unvisited nodes
            }
            dist[source] = 0;               // Distance from /source/ to itself is always 0;

            while(Q.Count > 0) {                        // While there are still unvisited nodes:
                var u = (from n in Q                    // Get the node with lowest distance from /source/.
                         orderby dist[n] ascending
                         select n).First();

                Q.Remove(u);                            // Remove the node from the unvisited nodes.

                if(u == target)                         // If we are at /target/, we can stop looping.
                    break;

                var neighbors = from n in u.Edges       // Every neighbor of /u/ that is unvisited.
                                where Q.Contains(n.End)
                                select n;
                foreach(var v in neighbors) {           // For every unvisited neighbor:
                    var alt = dist[u] + v.SqrCost;      //     The distance from /source/ to /v/, going through /u/.
                    if(alt < dist[v.End]) {             //     If the above distance is a shorter than dist[v], we found a shorter path.
                        dist[v.End] = alt;              //         update dist[v]
                        prev[v.End] = u;                //         set a new previous node for /v/.
                    }
                }
            }

            return reverse_iterate();                  // Construct the optimal path by reverse
                                                       // iterating through /prev/ from /target/.
            
            Stack<T> reverse_iterate() {
                var S = new Stack<T>();                       // Empty sequence.
                var u = target;
                if(prev.ContainsKey(u) || u == source) {          // If the vertex is reachable:
                    while(u != null) {                            // Construct the shortest path
                        S.Push((T)u);                                //     push the vertex onto the stack
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva.Path
{
    /// <summary>
    /// A function that checks whether or not the current node is at the destination.
    /// </summary>
    public delegate bool IsAtDestination<T>(T currentNode);
    /// <summary>
    /// Pathfinding with Dijkstra's algorithm.
    /// <para />
    /// https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm.
    /// </summary>
    public static class Dijkstra
    {
        /// <summary>
        /// Find the shortest distance between nodes /source/ and /target/ on the specified graph.
        /// </summary>
        public static Stack<T> FindPath<T>(IGraph<T> graph, T source, T target) where T : INode<T> {
            return FindPath(graph, source, (n) => n.Equals(target));
        }
        /// <summary>
        /// Find the shortest path between /source/ and the first node to pass the destination check.
        /// </summary>
        /// <param name="dest">Returns whether or not the specified node is the destination.</param>
        public static Stack<T> FindPath<T>(IGraph<T> graph, T source, IsAtDestination<T> dest) where T : INode<T> {
            var dist = new Dictionary<T, float>(); // The squared distance from /source/ for each node.
            var prev = new Dictionary<T, T>();     // The node before the key in the optimal path to /source/.
            var Q = new List<T>();                 // Set of unvisited nodes.

            foreach(var v in graph.Nodes) { // For every node in the graph:
                dist[v] = float.MaxValue;   // Unkown distance from /source/ to the node.
                Q.Add(v);                   // Add it to the list of unvisited nodes
            }
            dist[source] = 0;               // Distance from /source/ to itself is always 0;

            while(Q.Count > 0) {                        // While there are still unvisited nodes:
                var u = (from n in Q                    // Get the node with lowest distance from /source/.
                         where dist[n] != float.MaxValue
                         orderby dist[n] ascending //NOTE: this can be optimized by using a priority queue.
                         select n).First();

                Q.Remove(u);                            // Remove the node from the unvisited nodes.
                
                if(dest(u))                             // If we are at the destination,
                    return reverse_iterate(u);          //     construct the path through reverse iteration, and return it.

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
                                   // If we got this far without leaving the method,
                                   //     that means there there is no possible path.
            return new Stack<T>(); //     Return an empty stack and let the caller figure it out.
            
            Stack<T> reverse_iterate(T target) {
                var S = new Stack<T>(); // Empty sequence.
                var u = target;         // The current node, working backwards from /target/.
                while(prev.ContainsKey(u)) { // If the vertex is reachable,
                    S.Push(u);               //     push the vertex onto the stack, and
                    u = prev[u];             //     set /u/ as the previous node in the optimal path.
                }
                return S;                    // Return the path we just constructed.
                                             // If the target was unreachable,
                                             //     just return an empty stack and let the caller figure it out.
            }
        }

        /// <summary>
        /// Returns whether or not there exists a path between /source/ and /target/.
        /// </summary>
        public static bool IsAccessible<T>(IGraph<T> graph, T source, T target) where T : INode<T> {
            return IsAccessible(graph, source, (n) => n.Equals(target));
        }
        /// <summary>
        /// Returns whether or not there exists a path between /source/ and the first node to pass the destination check.
        /// </summary>
        /// <param name="dest">Returns whether or not the specified node is the destination.</param>
        public static bool IsAccessible<T>(IGraph<T> graph, T source, IsAtDestination<T> dest) where T : INode<T> {
            var unvisited = new List<T>(graph.Nodes);  // All unvisited nodes. Initially contains all nodes in the graph.
            var accessible = new List<T>() { source }; // Nodes accessible from the source. Initiall only contains the source.

            while(accessible.Count > 0) {                       // Loop until there are no accessible nodes:
                var u = (from n in accessible                   // Get the first accessible and unvisited node.
                         where unvisited.Contains(n)
                         select n).FirstOrDefault();

                if(u == null)                                   // If there are no accessible and unvisited nodes,
                    break;                                      //     break from this loop.

                unvisited.Remove(u);                            // Remove that node from the univisited nodes.

                if(dest(u))                                     // If we are at the destination,
                    return true;                                //     return true right now.

                var neighbors = from n in u.Edges               // Every neighbor of the node that is unvisited.
                                where unvisited.Contains(n.End)
                                select n.End;
                foreach(var v in neighbors) {                   // For every unvisited node:
                    accessible.Add(v);                          //     add it to the list of accessible nodes.
                }
            }
                          // If we've visited every accessible node without findint the destination,
            return false; //     return false.
        }
    }
}

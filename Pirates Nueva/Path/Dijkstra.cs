﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva.Path
{
    /// <summary>
    /// A function that checks whether or not the current node is at the destination.
    /// </summary>
    public delegate bool IsAtDestination<T>(INode<T> currentNode);
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
        public static Stack<T> FindPath<T>(IGraph<T> graph, INode<T> source, INode<T> target) where T : INode<T> {
            return FindPath(graph, source, (n) => n == target);
        }
        /// <summary>
        /// Find the shortest path between /source/ and the first node to pass the destination check.
        /// </summary>
        /// <param name="dest">Returns whether or not the specified node is the destination.</param>
        public static Stack<T> FindPath<T>(IGraph<T> graph, INode<T> source, IsAtDestination<T> dest) where T : INode<T> {
            var dist = new Dictionary<INode<T>, float>();    // The squared distance from /source/ for each node.
            var prev = new Dictionary<INode<T>, INode<T>>(); // The node before the key in the optimal path to /source/.
            var Q = new List<INode<T>>();                    // Set of unvisited nodes.

            foreach(var v in graph.Nodes) { // For every node in the graph:
                dist[v] = float.MaxValue;   // Unkown distance from /source/ to the node.
                Q.Add(v);                   // Add it to the list of unvisited nodes
            }
            dist[source] = 0;               // Distance from /source/ to itself is always 0;
            prev[source] = null;            // There should never be node coming before the source.

            while(Q.Count > 0) {                        // While there are still unvisited nodes:
                var u = (from n in Q                    // Get the node with lowest distance from /source/.
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
            
            Stack<T> reverse_iterate(INode<T> target) {
                var S = new Stack<T>();   // Empty sequence.
                var u = target;           // The current node, working backwards from /target/.
                if(prev.ContainsKey(u)) {    // If the vertex is reachable:
                    while(prev[u] != null) { // Construct the shortest path
                        S.Push((T)u);        //     push the vertex onto the stack
                        u = prev[u];         //     set /u/ as the previous node in the optimal path.
                    }
                }
                return S;                    // Return the path we just constructed.
                                             // If the target was unreachable,
                                             //     just return an empty stack and let the caller figure it out.
            }
        }
    }
}

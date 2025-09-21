using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;

namespace GraphImplementationAssignment
{
    public class DijkstraImpl
    {
        // Returns a path, an accumulated "Dijkstra cost", and Found=true if a path was built.
        // If negative edges exist, the cost/path may NOT be globally shortest. We warn via Console.
        public PathResult ExecuteAlgorithm(Graph graph, Vertex start, Vertex goal)
        {
            if (!graph.Vertices.Contains(start) || !graph.Vertices.Contains(goal))
                return PathResult.NotFound();

            // dist[v] ← ∞ ; dist[start] ← 0
            var dist = new Dictionary<Vertex, double>();
            foreach (var v in graph.Vertices) dist[v] = double.PositiveInfinity;
            dist[start] = 0.0;

            // For path reconstruction
            var parent = new Dictionary<string, string>();

            // Min-priority queue by current best dist
            var pq = new PriorityQueue<Vertex, double>();
            pq.Enqueue(start, 0.0);

            // Finalized set (S in the slides)
            var visited = new HashSet<Vertex>();

            // Track if we saw any negative edge (keep running, but warn)
            bool sawNegativeEdge = false;

            while (pq.Count > 0)
            {
                var u = pq.Dequeue();
                if (!visited.Add(u)) continue; // skip stale entries

                // Early exit when we finalize 'goal'
                if (u == goal)
                {
                    if (sawNegativeEdge)
                        Console.WriteLine("WARNING: Negative edge detected; Dijkstra may return a non-shortest path. Prefer Bellman–Ford.");
                    var path = ReconstructPath(start, goal, parent);
                    return new PathResult(path, dist[goal], true);
                }

                // Relax edges (u -> v)
                if (!graph.AdjList.TryGetValue(u, out var edges)) continue;
                foreach (var e in edges)
                {
                    if (e.Weight < 0) sawNegativeEdge = true; // keep going as requested

                    var v = e.To;
                    if (visited.Contains(v)) continue;

                    var alt = dist[u] + e.Weight;
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        parent[v.Name] = u.Name;
                        // no decrease-key; push new better priority
                        pq.Enqueue(v, alt);
                    }
                }
            }

            // goal unreachable
            return PathResult.NotFound();
        }

        private static List<string> ReconstructPath(Vertex start, Vertex goal, Dictionary<string, string> parent)
        {
            if (start == goal) return new List<string> { start.Name };
            if (!parent.ContainsKey(goal.Name)) return new List<string>();

            var path = new List<string>();
            var cur = goal.Name;
            path.Add(cur);
            while (cur != start.Name)
            {
                cur = parent[cur];
                path.Add(cur);
            }
            path.Reverse();
            return path;
        }
    }
}

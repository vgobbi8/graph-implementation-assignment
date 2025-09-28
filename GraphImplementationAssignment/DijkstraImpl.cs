using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;

namespace GraphImplementationAssignment
{
    public class DijkstraAlgorithm
    {
        public PathResult ExecuteAlgorithm(Graph graph, string start, string goal)
        {
            if (!graph.Vertices.Contains(start) || !graph.Vertices.Contains(goal))
                return PathResult.NotFound();

            var dist = new Dictionary<string, double>();
            var parent = new Dictionary<string, string>();
            foreach (var v in graph.Vertices) dist[v] = double.PositiveInfinity;
            dist[start] = 0.0;

            bool sawNegative = false;
            var pq = new PriorityQueue<string, double>();
            pq.Enqueue(start, 0.0);
            var visited = new HashSet<string>();

            while (pq.Count > 0)
            {
                var u = pq.Dequeue();
                if (!visited.Add(u)) continue;

                if (u == goal)
                {
                    if (sawNegative)
                        Console.WriteLine("Negative edge detected");
                    return PathResult.BuildPathResult(start, goal, parent);
                }

                if (!graph.AdjList.TryGetValue(u, out var edges)) continue;
                foreach (var e in edges)
                {
                    if (e.Weight < 0) sawNegative = true;
                    var v = e.To;
                    if (visited.Contains(v)) continue;
                    var alt = dist[u] + e.Weight;
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        parent[v] = u;
                        pq.Enqueue(v, alt);
                    }
                }
            }

            return PathResult.NotFound();
        }
    }
}

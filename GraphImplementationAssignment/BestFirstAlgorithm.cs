using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;

namespace GraphImplementationAssignment
{
    /// <summary>
    /// Greedy Best-First Search (priority = heuristic only).
    /// - Uses a min-priority queue ordered by h(v, goal).
    /// - Returns the first path found (not guaranteed shortest unless h is very special).
    /// - Cost in PathResult remains the number of edges (to match your PathResult type).
    ///
    /// Provide a heuristic function h(current, goal) -> non-negative estimate of distance to goal.
    /// </summary>
    public class BestFirstAlgorithm
    {
        public PathResult ExecuteAlgorithm(
            Graph graph,
            Vertex start,
            Vertex goal,
            Func<Vertex, Vertex, double> heuristic)
        {
            if (!graph.Vertices.Contains(start) || !graph.Vertices.Contains(goal))
                return PathResult.NotFound();

            var open = new PriorityQueue<Vertex, double>();
            var visited = new HashSet<Vertex>();
            var parent = new Dictionary<string, string>();

            open.Enqueue(start, heuristic(start, goal));
            visited.Add(start);

            while (open.Count > 0)
            {
                var u = open.Dequeue();
                if (u == goal)
                    return PathResult.BuildPathResult(start.Name, goal.Name, parent);

                foreach (var v in graph.NeigboorsOf(u))
                {
                    if (visited.Add(v))
                    {
                        parent[v.Name] = u.Name;
                        open.Enqueue(v, heuristic(v, goal));
                    }
                }
            }

            return PathResult.NotFound();
        }
    }
}

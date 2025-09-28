using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;

namespace GraphImplementationAssignment
{
    public class AStarAlgorithm
    {
        public PathResult ExecuteAlgorithm(
            Graph graph,
            string start,
            string goal,
            Func<string, string, double> heuristic)
        {
            if (!graph.Vertices.Contains(start) || !graph.Vertices.Contains(goal))
                return PathResult.NotFound();

            var gScore = new Dictionary<string, double>();
            var fScore = new Dictionary<string, double>();
            var parent = new Dictionary<string, string>();
            var open = new PriorityQueue<string, double>();
            var closed = new HashSet<string>();

            foreach (var v in graph.Vertices)
            {
                gScore[v] = double.PositiveInfinity;
                fScore[v] = double.PositiveInfinity;
            }
            gScore[start] = 0.0;
            fScore[start] = heuristic(start, goal);

            open.Enqueue(start, fScore[start]);

            while (open.Count > 0)
            {
                var u = open.Dequeue();
                if (u == goal)
                {
                    return PathResult.BuildPathResult(start, goal, parent);
                }
                if (!closed.Add(u)) continue;

                if (!graph.AdjList.TryGetValue(u, out var edges)) continue;
                foreach (var e in edges)
                {
                    var v = e.To;
                    if (closed.Contains(v)) continue;

                    var tentativeG = gScore[u] + e.Weight;
                    if (tentativeG < gScore[v])
                    {
                        gScore[v] = tentativeG;
                        fScore[v] = tentativeG + heuristic(v, goal);
                        parent[v] = u;
                        open.Enqueue(v, fScore[v]);
                    }
                }
            }
            return PathResult.NotFound();
        }

    }
}

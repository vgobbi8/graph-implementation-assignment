using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;

namespace GraphImplementationAssignment
{

    public class BestFirstAlgorithm
    {
        public PathResult ExecuteAlgorithm(
            Graph graph,
            string start,
            string goal,
            Func<string, string, double> heuristic)
        {
            var open = new PriorityQueue<string, double>();
            var visited = new HashSet<string>();
            var parent = new Dictionary<string, string>();

            open.Enqueue(start, heuristic(start, goal));
            visited.Add(start);

            while (open.Count > 0)
            {
                var u = open.Dequeue();
                if (u == goal)
                    return PathResult.BuildPathResult(start, goal, parent);

                foreach (var v in graph.NeigboorsOf(u))
                {
                    if (visited.Add(v))
                    {
                        parent[v] = u;
                        open.Enqueue(v, heuristic(v, goal));
                    }
                }
            }
            return PathResult.NotFound();
        }
    }
}

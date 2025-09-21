using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;

namespace GraphImplementationAssignment
{
    /// <summary>
    /// A* Search (priority = g + h)
    /// - g = cost from start to the current node (here we use WEIGHTED g based on Edge.Weight)
    /// - h = heuristic estimate from current to goal
    /// - Returns PathResult with cost measured as number of edges (to match your PathResult type).
    ///   If you want weighted total, either change PathResult.Cost to double or compute the sum
    ///   from the returned path using the helper below.
    ///
    /// Requirements for optimality: non-negative weights and an admissible (and ideally consistent) heuristic.
    /// </summary>
    public class AStarAlgorithm
    {
        public PathResult ExecuteAlgorithm(
            Graph graph,
            Vertex start,
            Vertex goal,
            Func<Vertex, Vertex, double> heuristic)
        {
            if (!graph.Vertices.Contains(start) || !graph.Vertices.Contains(goal))
                return PathResult.NotFound();

            var gScore = new Dictionary<Vertex, double>();
            var fScore = new Dictionary<Vertex, double>();
            var parent = new Dictionary<string, string>();
            var open = new PriorityQueue<Vertex, double>();
            var closed = new HashSet<Vertex>();

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
                    return PathResult.BuildPathResult(start.Name, goal.Name, parent);
                }
                if (!closed.Add(u)) continue; // already expanded

                // Iterate weighted edges from u
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
                        parent[v.Name] = u.Name;
                        open.Enqueue(v, fScore[v]); // no decrease-key; push again
                    }
                }
            }

            return PathResult.NotFound();
        }

        /// <summary>
        /// Optional helper: compute the weighted total of a path using the graph's weights.
        /// </summary>
        public static double ComputeWeightedPathCost(Graph graph, IReadOnlyList<string> path)
        {
            double sum = 0.0;
            for (int i = 0; i + 1 < path.Count; i++)
            {
                var from = new Vertex(path[i]);
                var to = new Vertex(path[i + 1]);
                if (!graph.AdjList.TryGetValue(from, out var edges)) return double.PositiveInfinity;
                bool found = false;
                foreach (var e in edges)
                {
                    if (e.To == to)
                    {
                        sum += e.Weight;
                        found = true;
                        break;
                    }
                }
                if (!found) return double.PositiveInfinity;
            }
            return sum;
        }
    }
}

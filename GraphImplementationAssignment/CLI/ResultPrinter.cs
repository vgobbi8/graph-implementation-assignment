using System;
using System.Collections.Generic;
using System.Linq;
using GraphImplementationAssignment.Models;

namespace GraphImplementationAssignment.CLI
{
    public static class ResultPrinter
    {
        public static void PrintPathResult(string algo, PathResult r, Graph g, bool includeWeighted = false)
        {
            Console.WriteLine($"Algorithm: {algo.ToUpperInvariant()}");
            Console.WriteLine($"Found:     {r.Found}");
            Console.WriteLine($"Path:      {(r.Path == null || r.Path.Count == 0 ? "-" : string.Join(" -> ", r.Path))}");
            Console.WriteLine($"Edges:     {r.Cost}");

            if (includeWeighted && r.Path != null && r.Path.Count > 1)
            {
                double w = WeightedCost(g, r.Path);
                Console.WriteLine($"Weighted:  {w}");
            }
        }

        public static void PrintMst(MSTResult mst)
        {
            Console.WriteLine("Algorithm: MST (Kruskal)");
            Console.WriteLine("Edges:");
            foreach (var (f, t, w) in mst.Edges)
                Console.WriteLine($"  {f} -- {t}  (w={w})");
            Console.WriteLine($"Total: {mst.TotalWeight}");
        }

        public static object ToJsonObject(string algo, PathResult r, Graph g, bool includeWeighted = false)
        {
            var obj = new Dictionary<string, object>
            {
                ["algorithm"] = algo,
                ["found"] = r.Found,
                ["path"] = r.Path,
                ["edgesCost"] = r.Cost,
            };
            if (includeWeighted)
                obj["weightedCost"] = WeightedCost(g, r.Path ?? new List<string>());
            return obj;
        }

        public static object ToJsonObject(string algo, MSTResult mst)
        {
            return new Dictionary<string, object>
            {
                ["algorithm"] = algo,
                ["edges"] = mst.Edges,
                ["totalWeight"] = mst.TotalWeight
            };
        }

        private static double WeightedCost(Graph g, IReadOnlyList<string> names)
        {
            double sum = 0.0;
            for (int i = 0; i + 1 < names.Count; i++)
            {
                var a = new Vertex(names[i]);
                var b = new Vertex(names[i + 1]);
                if (!g.AdjList.TryGetValue(a, out var list)) return double.PositiveInfinity;
                var edge = list.FirstOrDefault(e => e.To.Equals(b));
                if (edge == default) return double.PositiveInfinity;
                sum += edge.Weight;
            }
            return sum;
        }
    }
}

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
            if (!includeWeighted)
            {
                return new
                {
                    algorithm = algo,
                    found = r.Found,
                    path = r.Path,
                    edgesCost = r.Cost
                };

            }
            return new
            {

                algorithm = algo,
                found = r.Found,
                path = r.Path,
                edgesCost = r.Cost,
                weightedCost = WeightedCost(g, r.Path ?? new List<string>())
            };
        }

        public static object ToJsonObject(string algo, MSTResult mst)
        {
            return new
            {
                algorithm = algo,
                edges = mst.Edges,
                totalWeight = mst.TotalWeight
            };
        }

        private static double WeightedCost(Graph g, IReadOnlyList<string> names)
        {
            double sum = 0.0;

            for (int i = 0; i + 1 < names.Count; i++)
            {
                var a = names[i];
                var b = names[i + 1];

                if (!g.AdjList.TryGetValue(a, out var list))
                    return double.PositiveInfinity;

                var edge = list.FirstOrDefault(e => e.To.Equals(b));
                if (edge == default)
                    return double.PositiveInfinity;

                sum += edge.Weight;
            }
            return sum;
        }

        public static void PrintGraphOverview(Graph g)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Graph: {(g.Directed ? "DIRECTED" : "UNDIRECTED")}");
            Console.ResetColor();

            var vertexCount = g.Vertices.Count;
            var edgeCount = 0;

            foreach (var (_, list) in g.AdjList)
                edgeCount += list.Count;

            if (!g.Directed)
                edgeCount /= 2;

            Console.WriteLine($"Vertices: {vertexCount}");
            Console.WriteLine($"Edges:    {edgeCount}");
        }

        public static void PrintAdjacencyList(Graph g, bool showWeights = true)
        {
            Console.WriteLine();
            Console.WriteLine("Adjacency List:");
            foreach (var (u, edges) in g.AdjList)
            {
                var parts = new List<string>();
                foreach (var e in edges)
                {
                    var arrow = g.Directed ? "->" : "--";
                    parts.Add(showWeights ? $"{arrow}{e.To}(w={e.Weight})" : $"{arrow}{e.To}");
                }
                Console.WriteLine($"  {u} {string.Join(" ", parts)}");
            }
        }

        public static void PrintAdjacencyMatrix(Graph g, bool showWeights = true, int width = 5)
        {
            var verts = new List<string>(g.Vertices);

            verts.Sort((a, b) => string.Compare(a, b, StringComparison.Ordinal));

            var index = new Dictionary<string, int>();

            for (int i = 0; i < verts.Count; i++)
                index[verts[i]] = i;

            var n = verts.Count;
            var matrix = new double?[n, n];
            for (int i = 0; i < n; i++)
            {
                if (!g.AdjList.TryGetValue(verts[i], out var list))
                    continue;
                foreach (var e in list)
                {
                    var j = index[e.To];
                    matrix[i, j] = e.Weight;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Adjacency Matrix:");
            Console.Write("     ");
            foreach (var v in verts)
            {
                Console.Write(v.PadLeft(width));
            }

            Console.WriteLine();

            for (int i = 0; i < n; i++)
            {
                Console.Write(verts[i].PadRight(4) + " ");
                for (int j = 0; j < n; j++)
                {
                    if (matrix[i, j].HasValue)
                    {
                        if (showWeights)
                            Console.Write($"{matrix[i, j].Value.ToString("0.##").PadLeft(width)}");
                        else
                            Console.Write($"{("1").PadLeft(width)}");
                    }
                    else
                        Console.Write($"{(".").PadLeft(width)}");
                }
                Console.WriteLine();
            }
            Console.WriteLine(g.Directed ? "rows are FROM, columns are TO" : "Undirected matrix printed as stored.");
        }
    }
}

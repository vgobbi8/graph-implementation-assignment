using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;

namespace GraphImplementationAssignment
{
    public class KruskalMST
    {
        public MSTResult Build(Graph graph)
        {
            var allEdges = new List<(string U, string V, double weight)>();

            foreach (var (u, list) in graph.AdjList)
            {
                foreach (var e in list)
                {
                    var v = e.To;
                    if (u == v) continue; // ignore loops for MST

                    var a = u;
                    var b = v;
                    if (string.Compare(a, b, StringComparison.Ordinal) > 0)
                        (a, b) = (b, a);

                    var U = new string(a);
                    var V = new string(b);
                    allEdges.Add((U, V, e.Weight));
                }
            }

            allEdges.Sort((x, y) =>
            {
                int c = x.weight.CompareTo(y.weight);
                if (c != 0) return c;
                c = string.Compare(x.U, y.U, StringComparison.Ordinal);
                if (c != 0) return c;
                return string.Compare(x.V, y.V, StringComparison.Ordinal);
            });

            var dedup = new List<(string U, string V, double weight)>();
            (string lastU, string lastV, double lastW) prev = ("", "", double.NaN);
            foreach (var e in allEdges)
            {
                if (e.U != prev.lastU || e.V != prev.lastV || e.weight != prev.lastW)
                {
                    dedup.Add(e);
                    prev = (e.U, e.V, e.weight);
                }
            }

            var uf = new UnionFind(graph.Vertices);
            var resultEdges = new List<(string From, string To, double Weight)>();
            double total = 0.0;

            foreach (var (U, V, W) in dedup)
            {
                if (uf.Union(U, V))
                {
                    resultEdges.Add((U, V, W));
                    total += W;
                }
            }

            return new MSTResult(resultEdges, total);
        }

        private sealed class UnionFind
        {
            private readonly Dictionary<string, string> parent = new();
            private readonly Dictionary<string, int> rank = new();

            public UnionFind(IEnumerable<string> vertices)
            {
                foreach (var v in vertices)
                {
                    parent[v] = v;
                    rank[v] = 0;
                }
            }

            private string Find(string x)
            {
                if (!parent[x].Equals(x)) parent[x] = Find(parent[x]);
                return parent[x];
            }

            public bool Union(string a, string b)
            {
                var ra = Find(a);
                var rb = Find(b);
                if (ra.Equals(rb)) return false; // already connected

                var raRank = rank[ra];
                var rbRank = rank[rb];
                if (raRank < rbRank) parent[ra] = rb;
                else if (raRank > rbRank) parent[rb] = ra;
                else { parent[rb] = ra; rank[ra] = raRank + 1; }
                return true;
            }
        }
    }

    public record MSTResult(List<(string From, string To, double Weight)> Edges, double TotalWeight)
    {
        public void Print()
        {
            Console.WriteLine("MST edges:");
            foreach (var (f, t, w) in Edges)
                Console.WriteLine($"  {f} -- {t}  (w={w})");
            Console.WriteLine($"Total weight = {TotalWeight}");
        }
    }
}

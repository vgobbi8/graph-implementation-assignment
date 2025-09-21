using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;

namespace GraphImplementationAssignment
{
    /// <summary>
    /// Kruskal's Minimum Spanning Tree (MST) using Union-Find (Disjoint Set Union).
    /// - Works on undirected weighted graphs; if the Graph is marked Directed, we still
    ///   treat edges as undirected (deduplicated) for the MST build.
    /// - Returns the list of chosen edges (u,v,w) and the total weight.
    /// - If the graph is disconnected, the result is a Minimum Spanning Forest (MSF).
    /// </summary>
    public class KruskalMST
    {
        public MSTResult Build(Graph graph)
        {
            // Collect unique undirected edges as (U, V, W)
            var allEdges = new List<(Vertex U, Vertex V, double W)>();

            foreach (var (u, list) in graph.AdjList)
            {
                foreach (var e in list)
                {
                    var v = e.To;
                    if (u == v) continue; // ignore loops for MST

                    // Deduplicate for undirected: only keep (minName, maxName)
                    // This also handles Directed=true by forcing an undirected view here.
                    var a = u.Name;
                    var b = v.Name;
                    if (string.Compare(a, b, StringComparison.Ordinal) > 0)
                        (a, b) = (b, a);

                    var U = new Vertex(a);
                    var V = new Vertex(b);
                    allEdges.Add((U, V, e.Weight));
                }
            }

            // Remove duplicates that came from symmetric storage
            allEdges.Sort((x, y) =>
            {
                int c = x.W.CompareTo(y.W);
                if (c != 0) return c;
                c = string.Compare(x.U.Name, y.U.Name, StringComparison.Ordinal);
                if (c != 0) return c;
                return string.Compare(x.V.Name, y.V.Name, StringComparison.Ordinal);
            });

            var dedup = new List<(Vertex U, Vertex V, double W)>();
            (string lastU, string lastV, double lastW) prev = ("", "", double.NaN);
            foreach (var e in allEdges)
            {
                if (e.U.Name != prev.lastU || e.V.Name != prev.lastV || e.W != prev.lastW)
                {
                    dedup.Add(e);
                    prev = (e.U.Name, e.V.Name, e.W);
                }
            }

            // Union-Find init
            var uf = new UnionFind(graph.Vertices);
            var resultEdges = new List<(string From, string To, double Weight)>();
            double total = 0.0;

            // Kruskal: scan edges in nondecreasing weight order
            foreach (var (U, V, W) in dedup)
            {
                if (uf.Union(U, V))
                {
                    resultEdges.Add((U.Name, V.Name, W));
                    total += W;
                }
            }

            return new MSTResult(resultEdges, total);
        }

        private sealed class UnionFind
        {
            private readonly Dictionary<Vertex, Vertex> parent = new();
            private readonly Dictionary<Vertex, int> rank = new();

            public UnionFind(IEnumerable<Vertex> vertices)
            {
                foreach (var v in vertices)
                {
                    parent[v] = v;
                    rank[v] = 0;
                }
            }

            private Vertex Find(Vertex x)
            {
                if (!parent[x].Equals(x)) parent[x] = Find(parent[x]);
                return parent[x];
            }

            public bool Union(Vertex a, Vertex b)
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

    /// <summary>
    /// Result of an MST/MSF computation.
    /// </summary>
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

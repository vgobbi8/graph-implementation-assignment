// File: Isomorphism.cs
using System;
using System.Collections.Generic;
using System.Linq;
using GraphImplementationAssignment.Models;

namespace GraphImplementationAssignment
{
    public static class Isomorphism
    {
        // Small graphs: quick degree filters + backtracking on buckets.
        public static (bool ok, Dictionary<string, string> map) AreIsomorphic(Graph g1, Graph g2, int cutoff = 9)
        {
            if (g1.Directed != g2.Directed) return (false, new());
            if (g1.Vertices.Count != g2.Vertices.Count) return (false, new());
            if (EdgeCount(g1) != EdgeCount(g2)) return (false, new());

            // degree signature must match
            var sig1 = Signatures(g1);
            var sig2 = Signatures(g2);
            var groups1 = sig1.GroupBy(kv => kv.Value).Select(g => g.Select(kv => kv.Key).ToList()).ToList();
            var groups2 = sig2.GroupBy(kv => kv.Value).Select(g => g.Select(kv => kv.Key).ToList()).ToList();
            var sizes1 = groups1.Select(x => x.Count).OrderBy(x => x).ToList();
            var sizes2 = groups2.Select(x => x.Count).OrderBy(x => x).ToList();
            if (!sizes1.SequenceEqual(sizes2)) return (false, new());

            if (g1.Vertices.Count > cutoff) return (false, new()); // keep it safe/simple

            var map = new Dictionary<Vertex, Vertex>();
            bool Backtrack(int i, List<List<Vertex>> G1, List<List<Vertex>> G2)
            {
                if (i == G1.Count) return StructureMatches(g1, g2, map);
                var A = G1[i];
                int j = G2.FindIndex(B => B.Count == A.Count);
                if (j < 0) return false;
                var B = G2[j];

                foreach (var perm in Permute(B))
                {
                    // try mapping A[k] -> perm[k]
                    for (int k = 0; k < A.Count; k++) map[A[k]] = perm[k];
                    if (StructureMatches(g1, g2, map) && Backtrack(i + 1, G1, RemoveAt(G2, j))) return true;
                    for (int k = 0; k < A.Count; k++) map.Remove(A[k]);
                }
                return false;
            }

            var ok = Backtrack(0, groups1, groups2);
            return (ok, ok ? map.ToDictionary(k => k.Key.Name, v => v.Value.Name) : new());
        }

        // ===== minimal helpers =====
        private static int EdgeCount(Graph g) => g.AdjList.Sum(p => p.Value.Count) / (g.Directed ? 1 : 2);
        private static Dictionary<Vertex, (int indeg, int outdeg)> Signatures(Graph g)
        {
            var indeg = new Dictionary<Vertex, int>(); var outdeg = new Dictionary<Vertex, int>();
            foreach (var v in g.Vertices) { indeg[v] = 0; outdeg[v] = g.AdjList.TryGetValue(v, out var l) ? l.Count : 0; }
            foreach (var (u, list) in g.AdjList) foreach (var e in list) indeg[e.To] = indeg.GetValueOrDefault(e.To, 0) + 1;
            return g.Vertices.ToDictionary(v => v, v => (indeg[v], outdeg[v]));
        }
        private static bool StructureMatches(Graph g1, Graph g2, Dictionary<Vertex, Vertex> map)
        {
            // Check mapped edges: for each u->v in g1, requires map(u)->map(v) in g2
            foreach (var (u, list) in g1.AdjList)
                foreach (var e in list)
                {
                    if (!map.TryGetValue(u, out var mu) || !map.TryGetValue(e.To, out var mv)) return false;
                    if (!g2.AdjList.TryGetValue(mu, out var l2) || !l2.Any(x => x.To.Equals(mv))) return false;
                }
            return true;
        }

        private static IEnumerable<IReadOnlyList<T>> Permute<T>(IList<T> xs)
        {
            int n = xs.Count; var used = new bool[n]; var cur = new T[n];
            bool Dfs(int d)
            {
                if (d == n) { yield return cur.ToArray(); yield return true; }
                for (int i = 0; i < n; i++) if (!used[i]) { used[i] = true; cur[d] = xs[i]; if (Dfs(d + 1)) { } used[i] = false; }
                yield return false;
            }
            Dfs(0); // iterator trick
            yield break;
        }
        private static List<List<T>> RemoveAt<T>(List<List<T>> L, int j)
        {
            var copy = new List<List<T>>(L); copy.RemoveAt(j); return copy;
        }
    }
}

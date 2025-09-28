// File: Hamiltonian.cs
using System;
using System.Collections.Generic;
using System.Linq;
using GraphImplementationAssignment.Models;

namespace GraphImplementationAssignment
{
    public static class Hamiltonian
    {
        // Backtracking with a tiny prune: no isolated vertices, and close back to start.
        public static List<string> FindCycle(Graph g)
        {
            if (g.Vertices.Count == 0) return new();

            // quick prune: any vertex with outdegree 0 => impossible
            var outdeg = g.Vertices.ToDictionary(v => v, v => g.AdjList.TryGetValue(v, out var l) ? l.Count : 0);
            if (outdeg.Any(kv => kv.Value == 0)) return new();

            var verts = g.Vertices.ToList();
            var start = verts[0];
            var used = new HashSet<Vertex> { start };
            var path = new List<Vertex> { start };

            bool Dfs(int depth, Vertex u)
            {
                if (depth == verts.Count) // try to close the cycle
                    return HasEdge(g, u, start);

                foreach (var v in g.NeigboorsOf(u))
                {
                    if (used.Contains(v)) continue;
                    used.Add(v); path.Add(v);
                    if (Dfs(depth + 1, v)) return true;
                    path.RemoveAt(path.Count - 1); used.Remove(v);
                }
                return false;
            }

            if (!Dfs(1, start)) return new();
            var names = path.Select(p => p.Name).ToList();
            names.Add(start.Name);
            return names;
        }

        private static bool HasEdge(Graph g, Vertex a, Vertex b)
            => g.AdjList.TryGetValue(a, out var list) && list.Any(e => e.To.Equals(b));
    }
}

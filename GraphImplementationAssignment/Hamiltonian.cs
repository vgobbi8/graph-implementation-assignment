// File: Hamiltonian.cs
using System;
using System.Collections.Generic;
using System.Linq;
using GraphImplementationAssignment.Models;

namespace GraphImplementationAssignment
{
    public static class Hamiltonian
    {
        public static List<string> FindCycle(Graph g)
        {
            if (g.Vertices.Count == 0) return new();
            var outdeg = g.Vertices.ToDictionary(v => v, v => g.AdjList.TryGetValue(v, out var l) ? l.Count : 0);
            if (outdeg.Any(kv => kv.Value == 0)) return new();

            var verts = g.Vertices.ToList();
            var start = verts[0];
            var used = new HashSet<string> { start };
            var path = new List<string> { start };

            bool Dfs(int depth, string u)
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
            var names = path.Select(p => p).ToList();
            names.Add(start);
            return names;
        }

        //Check 
        private static bool HasEdge(Graph g, string a, string b)
            => g.AdjList.TryGetValue(a, out var list) && list.Any(e => e.To.Equals(b));
    }
}

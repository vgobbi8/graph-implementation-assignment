// File: Eulerian.cs
using System;
using System.Collections.Generic;
using System.Linq;
using GraphImplementationAssignment.Models;

namespace GraphImplementationAssignment
{
    public static class Eulerian
    {
        // Undirected: all nonzero-degree vertices are connected AND every degree is even
        public static bool HasCircuitUndirected(Graph g)
        {
            if (g.Directed) throw new InvalidOperationException("Use HasCircuitDirected for directed graphs.");
            var deg = DegreeUndirected(g);
            var active = deg.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
            if (active.Count == 0) return true; // no edges -> vacuously Eulerian
            if (!IsConnectedUndirected(g, active[0], v => deg[v] > 0)) return false;
            return active.All(v => deg[v] % 2 == 0);
        }

        // Directed: in(v) == out(v) for all active vertices AND strongly connected on active set
        public static bool HasCircuitDirected(Graph g)
        {
            if (!g.Directed) throw new InvalidOperationException("Use HasCircuitUndirected for undirected graphs.");
            var indeg = InDegree(g); var outdeg = OutDegree(g);
            var active = g.Vertices.Where(v => indeg.GetValueOrDefault(v, 0) + outdeg.GetValueOrDefault(v, 0) > 0).ToList();
            if (active.Count == 0) return true;
            if (active.Any(v => indeg.GetValueOrDefault(v, 0) != outdeg.GetValueOrDefault(v, 0))) return false;
            return StronglyConnectedOnActive(g, active);
        }

        // Build a circuit if possible (Hierholzer)
        public static List<string> BuildCircuit(Graph g)
        {
            if (g.Directed)
                return HasCircuitDirected(g) ? HierholzerDirected(g) : new();
            else
                return HasCircuitUndirected(g) ? HierholzerUndirected(g) : new();
        }

        // ===== Minimal helpers =====
        private static Dictionary<Vertex, int> DegreeUndirected(Graph g)
        {
            var d = new Dictionary<Vertex, int>();
            foreach (var v in g.Vertices) d[v] = 0;
            foreach (var (u, list) in g.AdjList) d[u] += list.Count; // undirected stored twice
            return d;
        }
        private static Dictionary<Vertex, int> OutDegree(Graph g)
        {
            var d = new Dictionary<Vertex, int>();
            foreach (var v in g.Vertices)
                d[v] = g.AdjList.TryGetValue(v, out var list) ? list.Count : 0;
            return d;
        }
        private static Dictionary<Vertex, int> InDegree(Graph g)
        {
            var d = new Dictionary<Vertex, int>();
            foreach (var v in g.Vertices) d[v] = 0;
            foreach (var (u, list) in g.AdjList)
                foreach (var e in list) d[e.To] = d.GetValueOrDefault(e.To, 0) + 1;
            return d;
        }
        private static bool IsConnectedUndirected(Graph g, Vertex start, Func<Vertex, bool> active)
        {
            var st = new Stack<Vertex>(); var seen = new HashSet<Vertex>();
            st.Push(start); seen.Add(start);
            while (st.Count > 0)
            {
                var u = st.Pop();
                foreach (var v in g.NeigboorsOf(u))
                    if (active(v) && seen.Add(v)) st.Push(v);
            }
            foreach (var v in g.Vertices) if (active(v) && !seen.Contains(v)) return false;
            return true;
        }
        private static bool StronglyConnectedOnActive(Graph g, List<Vertex> active)
        {
            bool ReachAllFrom(Vertex s, Func<Vertex, bool> dirOK)
            {
                var st = new Stack<Vertex>(); var seen = new HashSet<Vertex>(); st.Push(s); seen.Add(s);
                while (st.Count > 0)
                {
                    var u = st.Pop();
                    if (!g.AdjList.TryGetValue(u, out var edges)) continue;
                    foreach (var e in edges) if (active.Contains(e.To) && seen.Add(e.To)) st.Push(e.To);
                }
                return active.All(seen.Contains);
            }
            // check from one active vertex in G and in transpose(G)
            var s0 = active[0];
            if (!ReachAllFrom(s0, _ => true)) return false;
            // transpose walk:
            var st = new Stack<Vertex>(); var seen2 = new HashSet<Vertex>(); st.Push(s0); seen2.Add(s0);
            while (st.Count > 0)
            {
                var u = st.Pop();
                foreach (var (x, list) in g.AdjList)
                    if (list.Any(e => e.To.Equals(u)) && active.Contains(x) && seen2.Add(x)) st.Push(x);
            }
            return active.All(seen2.Contains);
        }

        // ===== Tiny Hierholzer implementations =====
        private static List<string> HierholzerUndirected(Graph g)
        {
            var mult = new Dictionary<(string A, string B), int>(); // canonical (min,max)
            foreach (var (u, list) in g.AdjList)
                foreach (var e in list)
                {
                    var a = u.Name; var b = e.To.Name; if (a == b) continue;
                    if (string.Compare(a, b, StringComparison.Ordinal) > 0) (a, b) = (b, a);
                    mult[(a, b)] = mult.GetValueOrDefault((a, b), 0) + 1;
                }

            var start = g.Vertices.FirstOrDefault(v => g.AdjList[v].Count > 0) ?? g.Vertices.First();
            var stack = new Stack<string>(); var circuit = new List<string>(); var cur = start.Name;
            bool HasOut(string x) => mult.Any(kv => kv.Value > 0 && (kv.Key.A == x || kv.Key.B == x));
            string Next(string x) { foreach (var kv in mult) if (kv.Value > 0) { if (kv.Key.A == x) return kv.Key.B; if (kv.Key.B == x) return kv.Key.A; } return x; }
            void Use(string a, string b) { if (string.Compare(a, b, StringComparison.Ordinal) > 0) (a, b) = (b, a); if (mult.TryGetValue((a, b), out var c) && c > 0) mult[(a, b)] = c - 1; }

            while (stack.Count > 0 || HasOut(cur))
            {
                if (!HasOut(cur)) { circuit.Add(cur); cur = stack.Count > 0 ? stack.Pop() : cur; }
                else { stack.Push(cur); var nxt = Next(cur); Use(cur, nxt); cur = nxt; }
            }
            circuit.Add(cur); circuit.Reverse(); return circuit;
        }

        private static List<string> HierholzerDirected(Graph g)
        {
            var mult = new Dictionary<(string U, string V), int>();
            foreach (var (u, list) in g.AdjList)
                foreach (var e in list) mult[(u.Name, e.To.Name)] = mult.GetValueOrDefault((u.Name, e.To.Name), 0) + 1;

            var start = g.Vertices.FirstOrDefault(v => g.AdjList[v].Count > 0) ?? g.Vertices.First();
            var stack = new Stack<string>(); var circuit = new List<string>(); var cur = start.Name;
            bool HasOut(string u) => mult.Any(kv => kv.Value > 0 && kv.Key.U == u);
            string Next(string u) { foreach (var kv in mult) if (kv.Value > 0 && kv.Key.U == u) return kv.Key.V; return u; }
            void Use(string u, string v) { var k = (u, v); if (mult.TryGetValue(k, out var c) && c > 0) mult[k] = c - 1; }

            while (stack.Count > 0 || HasOut(cur))
            {
                if (!HasOut(cur)) { circuit.Add(cur); cur = stack.Count > 0 ? stack.Pop() : cur; }
                else { stack.Push(cur); var nxt = Next(cur); Use(cur, nxt); cur = nxt; }
            }
            circuit.Add(cur); circuit.Reverse(); return circuit;
        }
    }
}

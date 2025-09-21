using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphImplementationAssignment
{
    /// <summary>
    /// Milestone 4 utilities:
    /// - Eulerian circuit checks (undirected & directed) + optional Hierholzer circuit builder
    /// - Hamiltonian circuit (backtracking with pruning)
    /// - Graph isomorphism check (small graphs): fast filters + backtracking mapping
    ///
    /// All methods use the project's Graph/Vertex/Edge types.
    /// </summary>
    public static class EulerHamiltonIsomorphism
    {
        // ============================
        // Eulerian (Undirected)
        // ============================
        public static bool HasEulerianCircuitUndirected(Graph g)
        {
            if (g.Directed) throw new InvalidOperationException("Use HasEulerianCircuitDirected for directed graphs.");

            // Consider only vertices with degree > 0
            var deg = DegreeUndirected(g);
            var nonZero = deg.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
            if (nonZero.Count == 0) return true; // Trivial: no edges → vacuously Eulerian circuit

            // Connectedness on the induced subgraph of non-zero-degree vertices
            if (!IsConnectedUndirected(g, nonZero[0], v => deg[v] > 0)) return false;

            // All degrees even
            return nonZero.All(v => deg[v] % 2 == 0);
        }

        // ============================
        // Eulerian (Directed)
        // ============================
        public static bool HasEulerianCircuitDirected(Graph g)
        {
            if (!g.Directed) throw new InvalidOperationException("Use HasEulerianCircuitUndirected for undirected graphs.");

            var inDeg = InDegree(g);
            var outDeg = OutDegree(g);

            // Vertices with nonzero (in+out)
            var active = g.Vertices.Where(v => inDeg.GetValueOrDefault(v, 0) + outDeg.GetValueOrDefault(v, 0) > 0).ToList();
            if (active.Count == 0) return true; // no edges

            // in == out for every vertex
            if (active.Any(v => inDeg.GetValueOrDefault(v, 0) != outDeg.GetValueOrDefault(v, 0))) return false;

            // Strongly connected on active set
            return IsStronglyConnectedOnActive(g, active);
        }

        // ============================
        // Hierholzer's Algorithm (build a circuit if it exists)
        // ============================
        public static List<string> BuildEulerianCircuit(Graph g)
        {
            if (g.Directed)
            {
                if (!HasEulerianCircuitDirected(g)) return new List<string>();
                return HierholzerDirected(g);
            }
            else
            {
                if (!HasEulerianCircuitUndirected(g)) return new List<string>();
                return HierholzerUndirected(g);
            }
        }

        // Undirected Hierholzer: treat edges as undirected with multiplicity
        private static List<string> HierholzerUndirected(Graph g)
        {
            // Count undirected edges using canonical key (minName, maxName)
            var count = new Dictionary<(string A, string B), int>();
            foreach (var (u, edges) in g.AdjList)
            {
                foreach (var e in edges)
                {
                    var a = u.Name; var b = e.To.Name;
                    if (a == b) continue; // ignore self-loops in Euler circuit
                    if (string.Compare(a, b, StringComparison.Ordinal) > 0) (a, b) = (b, a);
                    var key = (a, b);
                    count[key] = count.GetValueOrDefault(key, 0) + 1;
                }
            }

            // pick a start with deg>0
            var start = g.Vertices.FirstOrDefault(v => g.AdjList[v].Count > 0) ?? g.Vertices.First();

            var stack = new Stack<string>();
            var circuit = new List<string>();
            string cur = start.Name;

            while (stack.Count > 0 || HasAvailableUndirected(cur, count))
            {
                if (!HasAvailableUndirected(cur, count))
                {
                    circuit.Add(cur);
                    cur = stack.Count > 0 ? stack.Pop() : cur;
                }
                else
                {
                    stack.Push(cur);
                    // choose any available neighbor
                    var next = NextUndirectedNeighbor(cur, count);
                    // consume one undirected edge (cur,next)
                    ConsumeUndirected(cur, next, count);
                    cur = next;
                }
            }
            circuit.Add(cur);
            circuit.Reverse();
            return circuit;
        }

        private static bool HasAvailableUndirected(string a, Dictionary<(string A, string B), int> count)
        {
            foreach (var kv in count)
            {
                if (kv.Value <= 0) continue;
                if (kv.Key.A == a || kv.Key.B == a) return true;
            }
            return false;
        }
        private static string NextUndirectedNeighbor(string a, Dictionary<(string A, string B), int> count)
        {
            foreach (var kv in count)
            {
                if (kv.Value <= 0) continue;
                if (kv.Key.A == a) return kv.Key.B;
                if (kv.Key.B == a) return kv.Key.A;
            }
            return a; // shouldn't happen if caller checks availability
        }
        private static void ConsumeUndirected(string a, string b, Dictionary<(string A, string B), int> count)
        {
            if (string.Compare(a, b, StringComparison.Ordinal) > 0) (a, b) = (b, a);
            var key = (a, b);
            if (count.TryGetValue(key, out var c) && c > 0) count[key] = c - 1;
        }

        // Directed Hierholzer
        private static List<string> HierholzerDirected(Graph g)
        {
            // Use counts for directed arcs (u,v)
            var count = new Dictionary<(string U, string V), int>();
            foreach (var (u, edges) in g.AdjList)
                foreach (var e in edges)
                    count[(u.Name, e.To.Name)] = count.GetValueOrDefault((u.Name, e.To.Name), 0) + 1;

            // pick a start with outdegree>0
            var start = g.Vertices.FirstOrDefault(v => g.AdjList[v].Count > 0) ?? g.Vertices.First();

            var stack = new Stack<string>();
            var circuit = new List<string>();
            string cur = start.Name;

            while (stack.Count > 0 || HasAvailableOut(cur, count))
            {
                if (!HasAvailableOut(cur, count))
                {
                    circuit.Add(cur);
                    cur = stack.Count > 0 ? stack.Pop() : cur;
                }
                else
                {
                    stack.Push(cur);
                    var next = NextOutNeighbor(cur, count);
                    ConsumeDirected(cur, next, count);
                    cur = next;
                }
            }
            circuit.Add(cur);
            circuit.Reverse();
            return circuit;
        }

        private static bool HasAvailableOut(string u, Dictionary<(string U, string V), int> count)
        {
            foreach (var kv in count)
                if (kv.Value > 0 && kv.Key.U == u) return true;
            return false;
        }
        private static string NextOutNeighbor(string u, Dictionary<(string U, string V), int> count)
        {
            foreach (var kv in count)
                if (kv.Value > 0 && kv.Key.U == u) return kv.Key.V;
            return u;
        }
        private static void ConsumeDirected(string u, string v, Dictionary<(string U, string V), int> count)
        {
            var key = (u, v);
            if (count.TryGetValue(key, out var c) && c > 0) count[key] = c - 1;
        }

        // ============================
        // Hamiltonian circuit (backtracking with pruning)
        // ============================
        public static List<string> HamiltonianCircuit(Graph g)
        {
            if (g.Vertices.Count == 0) return new List<string>();

            // quick prune: any vertex isolated? (deg 0)
            var degOut = OutDegree(g);
            var degIn = InDegree(g);
            foreach (var v in g.Vertices)
            {
                if (!g.Directed && degOut.GetValueOrDefault(v, 0) == 0) return new List<string>();
                if (g.Directed && degOut.GetValueOrDefault(v, 0) == 0) return new List<string>();
            }

            var vertices = g.Vertices.ToList();
            var start = vertices[0];
            var path = new List<Vertex> { start };
            var used = new HashSet<Vertex> { start };

            bool DFS(int depth, Vertex u)
            {
                if (depth == vertices.Count)
                {
                    // close the cycle
                    return HasEdge(g, u, start);
                }

                // order candidates by degree (heuristic)
                var nbrs = g.NeigboorsOf(u)
                              .Where(v => !used.Contains(v))
                              .OrderByDescending(v => degOut.GetValueOrDefault(v, 0) + degIn.GetValueOrDefault(v, 0))
                              .ToList();

                foreach (var v in nbrs)
                {
                    used.Add(v);
                    path.Add(v);
                    if (DFS(depth + 1, v)) return true;
                    path.RemoveAt(path.Count - 1);
                    used.Remove(v);
                }
                return false;
            }

            if (DFS(1, start))
            {
                var cycle = path.Select(p => p.Name).ToList();
                cycle.Add(start.Name); // close
                return cycle;
            }
            return new List<string>();
        }

        // ============================
        // Isomorphism (small graphs)
        // ============================
        public static (bool Isomorphic, Dictionary<string, string> Mapping) IsIsomorphic(Graph g1, Graph g2, int backtrackCutoff = 9)
        {
            // quick filters
            if (g1.Directed != g2.Directed) return (false, new());
            if (g1.Vertices.Count != g2.Vertices.Count) return (false, new());

            var e1 = EdgeCount(g1);
            var e2 = EdgeCount(g2);
            if (e1 != e2) return (false, new());

            // degree signatures must match
            if (!g1.Directed)
            {
                var d1 = DegreeSequenceUndirected(g1);
                var d2 = DegreeSequenceUndirected(g2);
                if (!d1.SequenceEqual(d2)) return (false, new());
            }
            else
            {
                var (in1, out1) = DegreeSequenceDirected(g1);
                var (in2, out2) = DegreeSequenceDirected(g2);
                if (!in1.SequenceEqual(in2) || !out1.SequenceEqual(out2)) return (false, new());
            }

            var n = g1.Vertices.Count;
            if (n > backtrackCutoff)
            {
                // Avoid factorial blow-up by default
                return (false, new());
            }

            // Build candidate buckets by degree signature to prune permutations
            var sig1 = VertexSignatures(g1);
            var sig2 = VertexSignatures(g2);

            // Group vertices by signature
            var groups1 = sig1.GroupBy(kv => kv.Value)
                              .Select(g => g.Select(kv => kv.Key).ToList()).ToList();
            var groups2 = sig2.GroupBy(kv => kv.Value)
                              .Select(g => g.Select(kv => kv.Key).ToList()).ToList();

            // The multisets of group sizes must match
            var sizes1 = groups1.Select(x => x.Count).OrderBy(x => x).ToList();
            var sizes2 = groups2.Select(x => x.Count).OrderBy(x => x).ToList();
            if (!sizes1.SequenceEqual(sizes2)) return (false, new());

            // Map group-by-group with backtracking
            var map = new Dictionary<Vertex, Vertex>();
            var used = new HashSet<Vertex>();

            bool Backtrack(int gi, List<List<Vertex>> G1, List<List<Vertex>> G2)
            {
                if (gi == G1.Count) return CheckStructure(g1, g2, map);

                var A = G1[gi];
                // find a matching group in G2 with same size
                int targetIndex = -1;
                for (int j = 0; j < G2.Count; j++)
                {
                    if (G2[j].Count == A.Count) { targetIndex = j; break; }
                }
                if (targetIndex < 0) return false;

                var B = G2[targetIndex];

                // try all bijections between A and B
                foreach (var perm in Permute(B))
                {
                    bool ok = true;
                    for (int i = 0; i < A.Count; i++)
                    {
                        if (used.Contains(perm[i])) { ok = false; break; }
                        map[A[i]] = perm[i];
                    }
                    if (ok && Backtrack(gi + 1, G1, RemoveAt(G2, targetIndex))) return true;

                    // undo
                    for (int i = 0; i < A.Count; i++) map.Remove(A[i]);
                }
                return false;
            }

            var okAll = Backtrack(0, groups1, groups2);
            var mapping = okAll ? map.ToDictionary(k => k.Key.Name, v => v.Value.Name) : new Dictionary<string, string>();
            return (okAll, mapping);
        }

        // ============================
        // Helpers
        // ============================
        private static Dictionary<Vertex, int> DegreeUndirected(Graph g)
        {
            var d = new Dictionary<Vertex, int>();
            foreach (var v in g.Vertices) d[v] = 0;
            foreach (var (u, edges) in g.AdjList)
                d[u] += edges.Count; // because g stores both directions for undirected
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
            foreach (var (u, edges) in g.AdjList)
                foreach (var e in edges)
                    d[e.To] = d.GetValueOrDefault(e.To, 0) + 1;
            return d;
        }

        private static bool IsConnectedUndirected(Graph g, Vertex start, Func<Vertex, bool> active)
        {
            var vis = new HashSet<Vertex>();
            var st = new Stack<Vertex>();
            st.Push(start); vis.Add(start);
            while (st.Count > 0)
            {
                var u = st.Pop();
                foreach (var v in g.NeigboorsOf(u))
                {
                    if (active(v) && vis.Add(v)) st.Push(v);
                }
            }
            // every active vertex must be visited
            foreach (var v in g.Vertices)
                if (active(v) && !vis.Contains(v)) return false;
            return true;
        }

        private static bool IsStronglyConnectedOnActive(Graph g, List<Vertex> active)
        {
            // DFS from any active vertex in the directed graph
            var start = active[0];
            var vis = new HashSet<Vertex>();
            var st = new Stack<Vertex>(); st.Push(start); vis.Add(start);
            while (st.Count > 0)
            {
                var u = st.Pop();
                foreach (var v in g.NeigboorsOf(u))
                    if (vis.Add(v)) st.Push(v);
            }
            foreach (var v in active) if (!vis.Contains(v)) return false;

            // DFS on transpose
            var visT = new HashSet<Vertex>();
            st.Clear(); st.Push(start); visT.Add(start);
            // build reverse adjacency on the fly
            var rev = BuildTranspose(g);
            while (st.Count > 0)
            {
                var u = st.Pop();
                if (!rev.TryGetValue(u, out var back)) continue;
                foreach (var v in back)
                    if (visT.Add(v)) st.Push(v);
            }
            foreach (var v in active) if (!visT.Contains(v)) return false;
            return true;
        }

        private static Dictionary<Vertex, List<Vertex>> BuildTranspose(Graph g)
        {
            var rev = new Dictionary<Vertex, List<Vertex>>();
            foreach (var v in g.Vertices) rev[v] = new List<Vertex>();
            foreach (var (u, edges) in g.AdjList)
                foreach (var e in edges)
                    rev[e.To].Add(u);
            return rev;
        }

        private static bool HasEdge(Graph g, Vertex a, Vertex b)
        {
            if (!g.AdjList.TryGetValue(a, out var list)) return false;
            foreach (var e in list) if (e.To.Equals(b)) return true;
            return false;
        }

        private static int EdgeCount(Graph g)
        {
            int m = 0;
            foreach (var (_, list) in g.AdjList) m += list.Count;
            return g.Directed ? m : m / 2;
        }
        private static IEnumerable<int> DegreeSequenceUndirected(Graph g)
            => DegreeUndirected(g).Values.OrderBy(x => x);
        private static (IEnumerable<int> In, IEnumerable<int> Out) DegreeSequenceDirected(Graph g)
            => (InDegree(g).Values.OrderBy(x => x), OutDegree(g).Values.OrderBy(x => x));

        private static Dictionary<Vertex, string> VertexSignatures(Graph g)
        {
            var inD = InDegree(g);
            var outD = OutDegree(g);
            var sig = new Dictionary<Vertex, string>();
            foreach (var v in g.Vertices)
            {
                if (!g.Directed) sig[v] = $"d:{outD.GetValueOrDefault(v, 0)}";
                else sig[v] = $"in:{inD.GetValueOrDefault(v, 0)}|out:{outD.GetValueOrDefault(v, 0)}";
            }
            return sig;
        }

        private static bool CheckStructure(Graph g1, Graph g2, Dictionary<Vertex, Vertex> map)
        {
            // For every mapped pair (u -> f(u)), ensure adjacency is preserved
            foreach (var (u, fu) in map)
            {
                var uNbrs = g1.NeigboorsOf(u);
                foreach (var v in uNbrs)
                {
                    if (!map.TryGetValue(v, out var fv)) continue; // skip until v is mapped
                    // g1 has edge u->v, then g2 must have fu->fv
                    if (!HasEdge(g2, fu, fv)) return false;
                }
            }
            return true;
        }

        // permutations helper
        private static IEnumerable<List<T>> Permute<T>(IList<T> items)
        {
            int n = items.Count;
            var arr = items.ToArray();
            var used = new bool[n];
            var cur = new T[n];
            foreach (var p in PermuteRec(0)) yield return new List<T>(p);

            IEnumerable<T[]> PermuteRec(int i)
            {
                if (i == n) { yield return cur.ToArray(); yield break; }
                for (int k = 0; k < n; k++)
                {
                    if (used[k]) continue;
                    used[k] = true; cur[i] = arr[k];
                    foreach (var r in PermuteRec(i + 1)) yield return r;
                    used[k] = false;
                }
            }
        }

        private static List<List<Vertex>> RemoveAt(List<List<Vertex>> groups, int idx)
        {
            var copy = new List<List<Vertex>>();
            for (int i = 0; i < groups.Count; i++) if (i != idx) copy.Add(groups[i]);
            return copy;
        }
    }
}

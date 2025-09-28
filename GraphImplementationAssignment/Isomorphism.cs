// File: IsomorphismSimple.cs
using System;
using System.Collections.Generic;
using System.Linq;
using GraphImplementationAssignment.Models;

namespace GraphImplementationAssignment
{
    public static class Isomorphism
    {
        public static (bool ok, Dictionary<string, string> map) AreIsomorphic(Graph g1, Graph g2)
        {
            //Simple graph - can't be directed
            if (g1.Directed || g2.Directed)
                return (false, new Dictionary<string, string>());

            if (g1.Vertices.Count != g2.Vertices.Count)
                return (false, new Dictionary<string, string>());

            if (g1.EdgeCount() != g2.EdgeCount())
                return (false, new Dictionary<string, string>());

            var deg1 = BuildDegrees(g1);
            var deg2 = BuildDegrees(g2);

            if (!SameDegreeMultiset(g1, g2, deg1, deg2))
                return (false, new Dictionary<string, string>());


            var v1 = g1.Vertices.ToList(); // order can be arbitrary
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            var used = new HashSet<string>(StringComparer.Ordinal);

            bool ok = Backtrack(g1, g2, v1, 0, map, used, deg1, deg2);

            if (!ok)
                return (false, new Dictionary<string, string>());

            return (true, new Dictionary<string, string>(map));
        }

        private static bool Backtrack(
            Graph g1,
            Graph g2,
            List<string> v1,
            int index,
            Dictionary<string, string> map,
            HashSet<string> used,
            Dictionary<string, int> deg1,
            Dictionary<string, int> deg2)
        {
            if (index == v1.Count)
                return true;

            string u = v1[index];
            int d = deg1[u];

            // Candidates in g2: unused vertices with the same degree
            foreach (var v in g2.Vertices)
            {
                if (used.Contains(v))
                    continue;


                if (deg2[v] != d)
                    continue;


                if (!ConsistentWithMappedNeighbors(g1, g2, u, v, map))
                    continue;


                map[u] = v;
                used.Add(v);

                bool deeper = Backtrack(g1, g2, v1, index + 1, map, used, deg1, deg2);

                if (deeper)
                    return true;

                map.Remove(u);
                used.Remove(v);
            }

            return false;
        }

        private static bool ConsistentWithMappedNeighbors(
            Graph g1,
            Graph g2,
            string u1,
            string u2,
            IReadOnlyDictionary<string, string> map)
        {
            // For each already-mapped vertex w1, edge presence must match between (u1, w1) and (u2, map[w1]).
            foreach (var w1 in map.Keys)
            {
                string w2 = map[w1];

                bool e1 = g1.HasEdge(u1, w1);
                bool e2 = g2.HasEdge(u2, w2);

                if (e1 != e2)
                    return false;

            }

            return true;
        }

        private static Dictionary<string, int> BuildDegrees(Graph g)
        {
            var deg = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var v in g.Vertices)
            {
                if (g.AdjList.TryGetValue(v, out var list))
                    deg[v] = list.Count;
                else
                    deg[v] = 0;

            }

            return deg;
        }

        private static bool SameDegreeMultiset(
            Graph g1,
            Graph g2,
            Dictionary<string, int> deg1,
            Dictionary<string, int> deg2)
        {
            var count1 = new Dictionary<int, int>();
            var count2 = new Dictionary<int, int>();

            foreach (var v in g1.Vertices)
            {
                int d = deg1[v];

                if (!count1.ContainsKey(d))
                    count1[d] = 0;


                count1[d] = count1[d] + 1;
            }

            foreach (var v in g2.Vertices)
            {
                int d = deg2[v];

                if (!count2.ContainsKey(d))
                    count2[d] = 0;


                count2[d] = count2[d] + 1;
            }

            if (count1.Count != count2.Count)
                return false;


            foreach (var kv in count1)
            {
                if (!count2.TryGetValue(kv.Key, out var c))
                    return false;

                if (c != kv.Value)
                    return false;

            }

            return true;
        }
    }
}

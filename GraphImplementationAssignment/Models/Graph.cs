using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphImplementationAssignment.Models
{
    public class Graph
    {
        public bool Directed { get; set; }
        public HashSet<string> Vertices { get; set; } = new HashSet<string>();
        public Dictionary<string, List<(string to, double w)>> AdjList { get; set; } = new();

        public Graph(bool directed = false)
        {
            this.Directed = directed;
        }


        public void AddVertex(string id)
        {
            if (Vertices.Add(id)) AdjList[id] = new List<(string to, double w)>();
        }

        public void AddEdge(string from, string to, double w = 1.0)
        {
            AddVertex(from);
            AddVertex(to);
            AdjList[from].Add((to, w));
            //If we are not working with a directed graph, we should add the reverse connection too
            if (!Directed && from != to) AdjList[to].Add((from, w));
        }
        public int EdgeCount()
        {
            // For undirected graphs, each edge is stored twice (unless loop)
            var total = 0;
            foreach (var (_, list) in AdjList) total += list.Count;
            return Directed ? total : total / 2 + LoopCount();
        }

        private int LoopCount()
        {
            // Count loops in undirected graphs once
            var loops = 0;
            foreach (var (u, list) in AdjList)
                foreach (var (v, _) in list)
                    if (u == v) loops++;
            return loops; // stored once per endpoint; OK to return as-is
        }

        // Degree for undirected; for directed you typically use In/Out
        public int Degree(string v)
        {
            if (Directed) throw new InvalidOperationException("Use InDegree/OutDegree for directed graphs.");
            return AdjList[v].Count; // includes loops once here; adjust if you want to count loop as 2
        }

        public int OutDegree(string v) => AdjList[v].Count;

        public int InDegree(string v)
        {
            var indeg = 0;
            foreach (var (u, list) in AdjList)
                foreach (var (to, _) in list)
                    if (to == v) indeg++;
            return indeg;
        }

        public int GraphDegree() // max vertex degree (undirected)
        {
            if (Directed) throw new InvalidOperationException("GraphDegree is for undirected graphs. Use max(In/Out).");
            var max = 0;
            foreach (var v in Vertices)
                if (AdjList[v].Count > max) max = AdjList[v].Count;
            return max;
        }
    }
}

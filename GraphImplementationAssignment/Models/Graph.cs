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
        public HashSet<Vertex> Vertices { get; set; } = new HashSet<Vertex>();
        public Dictionary<Vertex, List<Edge>> AdjList { get; set; } = new();

        public Graph(bool directed = false)
        {
            this.Directed = directed;
        }


        public void AddVertex(Vertex vertex)
        {
            if (Vertices.Add(vertex)) AdjList[vertex] = new();
        }

        public void AddEdge(Vertex from, Vertex to, double w = 1.0)
        {
            AddVertex(from);
            AddVertex(to);
            AdjList[from].Add(new Edge(to, w));
            //If we are not working with a directed graph, we should add the reverse connection too
            if (!Directed && from != to) AdjList[to].Add(new Edge(from, w));
        }

        public void AddEdge(string from, string to, double w = 1.0)
        {
            var vFrom = new Vertex(from);
            var vTo = new Vertex(to);
            AddEdge(vFrom, vTo, w);
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
        public int Degree(Vertex v)
        {
            if (Directed) throw new InvalidOperationException("Use InDegree/OutDegree for directed graphs.");
            return AdjList[v].Count; // includes loops once here; adjust if you want to count loop as 2
        }

        public int OutDegree(Vertex v) => AdjList[v].Count;

        public int InDegree(Vertex v)
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

        public List<Vertex> NeigboorsOf(Vertex v)
        {
            var list = new List<Vertex>();
            return AdjList[v].Select(x => x.To).ToList();
        }
    }
}

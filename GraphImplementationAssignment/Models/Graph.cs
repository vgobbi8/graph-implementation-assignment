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
        public Dictionary<string, List<Edge>> AdjList { get; set; } = new();
        public Dictionary<string, (double x, double y)> Coords { get; set; } = new();

        public Graph(bool directed = false)
        {
            this.Directed = directed;
        }


        public void AddVertex(string vertex)
        {
            if (Vertices.Add(vertex)) AdjList[vertex] = new();
        }

        public void AddEdge(string from, string to, double w = 1.0)
        {
            AddVertex(from);
            AddVertex(to);
            AdjList[from].Add(new Edge(to, w));
            //If we are not working with a directed graph, we should add the reverse connection too
            if (!Directed && from != to) AdjList[to].Add(new Edge(from, w));
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
            var loops = 0;
            foreach (var (u, list) in AdjList)
                foreach (var (v, _) in list)
                    if (u == v) loops++;
            return loops;
        }

        public int Degree(string v)
        {
            if (Directed) throw new InvalidOperationException("Use InDegree/OutDegree for directed graphs.");
            return AdjList[v].Count; 
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

        public int GraphDegree()
        {
            if (Directed) throw new InvalidOperationException("GraphDegree is for undirected graphs. Use max(In/Out).");
            var max = 0;
            foreach (var v in Vertices)
                if (AdjList[v].Count > max) max = AdjList[v].Count;
            return max;
        }

        public List<string> NeigboorsOf(string v)
        {
            var list = new List<string>();
            return AdjList[v].Select(x => x.To).ToList();
        }

        public bool HasEdge(string from, string to)
        {
            bool forwardExists = false;

            if (AdjList.TryGetValue(from, out var list))
            {
                forwardExists = list.Any(e => e.To == to);
            }

            if (forwardExists)
            {
                return true;
            }

            bool reverseNeeded = !Directed && from != to;

            if (reverseNeeded)
            {
                if (AdjList.TryGetValue(to, out var reverseList))
                {
                    bool reverseExists = reverseList.Any(e => e.To == from);

                    if (reverseExists)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}

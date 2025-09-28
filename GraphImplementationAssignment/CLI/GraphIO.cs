using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using GraphImplementationAssignment.Models;

namespace GraphImplementationAssignment.CLI
{
    public static class GraphIO
    {
        public static Graph LoadFromJson(string path)
        {
            var strText = File.ReadAllText(path);
            var objGraph = JsonSerializer.Deserialize<GraphJson>(strText);
            var graph = new Graph(objGraph.directed);
            foreach (var vertex in objGraph.vertices)
            {
                graph.AddVertex(vertex.name);
                if (vertex.x != null && vertex.y != null)
                    graph.Coords.Add(vertex.name, (vertex.x.Value, vertex.y.Value));

                foreach (var edge in vertex.adj)
                    graph.AddEdge(vertex.name, edge.To, edge.weight);
            }
            return graph;
        }



        #region JSON Classes

        public class GraphJson
        {
            public bool directed { get; set; }
            public List<VertexJson> vertices { get; set; }
        }

        public class VertexJson
        {
            public string name { get; set; }
            public double? x { get; set; }
            public double? y { get; set; }
            public List<EdgeJson> adj { get; set; }
        }


        public class EdgeJson
        {
            public string To { get; set; }
            public double weight { get; set; }
        }

        #endregion
    }
}

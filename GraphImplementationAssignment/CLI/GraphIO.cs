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
        public static (Graph graph, Dictionary<string, (double x, double y)> coords) LoadGraph(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".json" => LoadFromJson(path),
                ".csv" => LoadFromCsv(path),
                _ => throw new InvalidOperationException("Unsupported file type. Use .json or .csv")
            };
        }

        // JSON schema:
        // {
        //   "directed": false,
        //   "vertices": [{"name":"A","x":0,"y":0}, ...],
        //   "edges":    [{"from":"A","to":"B","weight":2}, ...]
        // }
        private static (Graph, Dictionary<string, (double, double)>) LoadFromJson(string path)
        {
            using var stream = File.OpenRead(path);
            var doc = JsonDocument.Parse(stream);

            bool directed = doc.RootElement.TryGetProperty("directed", out var dEl) && dEl.GetBoolean();
            var g = new Graph(directed);
            var coords = new Dictionary<string, (double, double)>();

            var nameToV = new Dictionary<string, Vertex>(StringComparer.Ordinal);

            if (doc.RootElement.TryGetProperty("vertices", out var vArr))
            {
                foreach (var v in vArr.EnumerateArray())
                {
                    var name = v.GetProperty("name").GetString();
                    var vx = new Vertex(name);
                    g.AddVertex(vx);
                    if (v.TryGetProperty("x", out var x) && v.TryGetProperty("y", out var y))
                        coords[name] = (x.GetDouble(), y.GetDouble());
                    nameToV[name] = vx;
                }
            }

            if (doc.RootElement.TryGetProperty("edges", out var eArr))
            {
                foreach (var e in eArr.EnumerateArray())
                {
                    var from = e.GetProperty("from").GetString();
                    var to = e.GetProperty("to").GetString();
                    double w = e.TryGetProperty("weight", out var wEl) ? wEl.GetDouble() : 1.0;

                    if (!nameToV.TryGetValue(from, out var u))
                    {
                        u = new Vertex(from); nameToV[from] = u; g.Vertices.Add(u); g.AdjList[u] = new List<Edge>();
                    }
                    if (!nameToV.TryGetValue(to, out var vtx))
                    {
                        vtx = new Vertex(to); nameToV[to] = vtx; g.Vertices.Add(vtx); g.AdjList[vtx] = new List<Edge>();
                    }
                    g.AddEdge(u, vtx, w);
                }
            }
            return (g, coords);
        }

        // CSV schema: header expected: from,to,weight  (weight optional)
        private static (Graph, Dictionary<string, (double, double)>) LoadFromCsv(string path)
        {
            var g = new Graph(false); // CSV has no directed flag; assume undirected
            var coords = new Dictionary<string, (double, double)>();
            var nameToV = new Dictionary<string, Vertex>();

            using var sr = new StreamReader(path);
            string header = sr.ReadLine();
            if (header == null) throw new InvalidOperationException("Empty CSV");
            var cols = header.Split(',');

            int idxFrom = Array.FindIndex(cols, c => c.Trim().Equals("from", StringComparison.OrdinalIgnoreCase));
            int idxTo = Array.FindIndex(cols, c => c.Trim().Equals("to", StringComparison.OrdinalIgnoreCase));
            int idxW = Array.FindIndex(cols, c => c.Trim().Equals("weight", StringComparison.OrdinalIgnoreCase));
            if (idxFrom < 0 || idxTo < 0) throw new InvalidOperationException("CSV must have 'from,to[,weight]' header");

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                string from = parts[idxFrom].Trim();
                string to = parts[idxTo].Trim();
                double w = 1.0;
                if (idxW >= 0 && idxW < parts.Length && double.TryParse(parts[idxW], NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    w = parsed;

                if (!nameToV.TryGetValue(from, out var u)) { u = new Vertex(from); nameToV[from] = u; g.Vertices.Add(u); g.AdjList[u] = new List<Edge>(); }
                if (!nameToV.TryGetValue(to, out var v)) { v = new Vertex(to); nameToV[to] = v; g.Vertices.Add(v); g.AdjList[v] = new List<Edge>(); }

                g.AdjList[u].Add(new Edge(v, w));
                if (!g.Directed) g.AdjList[v].Add(new Edge(u, w));
            }

            foreach (var v in g.Vertices)
                if (!g.AdjList.ContainsKey(v)) g.AdjList[v] = new List<Edge>();

            return (g, coords);
        }
    }
}

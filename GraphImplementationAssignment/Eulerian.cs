using System;
using System.Collections.Generic;
using GraphImplementationAssignment.Models;

namespace GraphImplementationAssignment
{
    public static class Eulerian
    {
        public static bool HasCircuitUndirected(Graph graph)
        {
            if (graph.Directed)
                throw new InvalidOperationException("Use HasCircuitDirected for directed graphs.");


            Dictionary<string, int> degree = CalculateUndirectedDegrees(graph);

            List<string> activeVertices = new List<string>();
            foreach (var vertex in graph.Vertices)
            {
                if (degree[vertex] > 0)
                    activeVertices.Add(vertex);
            }

            if (activeVertices.Count == 0)
                return true;

            string startVertex = activeVertices[0];
            bool isConnected = IsConnectedOnActiveUndirected(graph, startVertex, degree);

            if (!isConnected)
                return false;

            foreach (var vertex in activeVertices)
            {
                if ((degree[vertex] % 2) != 0)
                    return false;
            }

            return true;
        }
        public static bool HasCircuitDirected(Graph graph)
        {
            if (!graph.Directed)
                throw new InvalidOperationException("Use HasCircuitUndirected for undirected graphs.");


            Dictionary<string, int> indegree = ComputeInDegrees(graph);
            Dictionary<string, int> outdegree = ComputeOutDegrees(graph);

            List<string> activeVertices = new List<string>();
            foreach (var vertex in graph.Vertices)
            {
                int deg = indegree.GetValueOrDefault(vertex, 0) + outdegree.GetValueOrDefault(vertex, 0);
                if (deg > 0)
                    activeVertices.Add(vertex);

            }

            if (activeVertices.Count == 0)
                return true;


            foreach (var vertex in activeVertices)
            {
                if (indegree.GetValueOrDefault(vertex, 0) != outdegree.GetValueOrDefault(vertex, 0))
                    return false;
            }

            bool stronglyConnected = IsStronglyConnectedOnActive(graph, activeVertices);
            return stronglyConnected;
        }

        public static List<string> BuildCircuit(Graph graph)
        {
            if (graph.Directed)
            {
                if (HasCircuitDirected(graph))
                    return BuildCircuitHierholzerDirected(graph);
            }
            else
            {
                if (HasCircuitUndirected(graph))
                    return BuildCircuitHierholzerUndirected(graph);
            }
            return new List<string>();

        }


        private static Dictionary<string, int> CalculateUndirectedDegrees(Graph graph)
        {
            Dictionary<string, int> degree = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var vertex in graph.Vertices)
            {
                degree[vertex] = 0;
            }

            foreach (var pair in graph.AdjList)
            {
                string from = pair.Key;
                List<Edge> edges = pair.Value;

                degree[from] = degree[from] + edges.Count;
            }

            return degree;
        }

        private static Dictionary<string, int> ComputeOutDegrees(Graph graph)
        {
            Dictionary<string, int> outdegree = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var vertex in graph.Vertices)
            {
                if (graph.AdjList.TryGetValue(vertex, out var edges))
                    outdegree[vertex] = edges.Count;
                else
                    outdegree[vertex] = 0;

            }

            return outdegree;
        }

        private static Dictionary<string, int> ComputeInDegrees(Graph graph)
        {
            Dictionary<string, int> indegree = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var vertex in graph.Vertices)
                indegree[vertex] = 0;


            foreach (var pair in graph.AdjList)
            {
                List<Edge> edges = pair.Value;

                for (int i = 0; i < edges.Count; i++)
                {
                    string to = edges[i].To;

                    if (!indegree.ContainsKey(to))
                        indegree[to] = 0;

                    indegree[to] = indegree[to] + 1;
                }
            }

            return indegree;
        }

        private static bool IsConnectedOnActiveUndirected(Graph graph, string startVertex, Dictionary<string, int> degree)
        {
            HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);
            Stack<string> stack = new Stack<string>();

            stack.Push(startVertex);
            visited.Add(startVertex);

            while (stack.Count > 0)
            {
                string current = stack.Pop();
                List<string> neighbors = graph.NeigboorsOf(current);

                for (int i = 0; i < neighbors.Count; i++)
                {
                    string neighbor = neighbors[i];

                    if (degree[neighbor] > 0)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            stack.Push(neighbor);
                        }
                    }
                }
            }

            foreach (var vertex in graph.Vertices)
            {
                if (degree[vertex] > 0 && !visited.Contains(vertex))
                    return false;

            }

            return true;
        }

        private static bool IsStronglyConnectedOnActive(Graph graph, List<string> activeVertices)
        {
            string start = activeVertices[0];

            // DFS on original graph
            HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);
            Stack<string> stack = new Stack<string>();

            stack.Push(start);
            visited.Add(start);

            while (stack.Count > 0)
            {
                string current = stack.Pop();
                List<string> neighbors = graph.NeigboorsOf(current);

                for (int i = 0; i < neighbors.Count; i++)
                {
                    string neighbor = neighbors[i];

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        stack.Push(neighbor);
                    }
                }
            }

            foreach (var vertex in activeVertices)
            {
                if (!visited.Contains(vertex))
                    return false;

            }

            // DFS on transpose graph
            Dictionary<string, List<string>> transpose = BuildTranspose(graph);

            HashSet<string> visitedTranspose = new HashSet<string>(StringComparer.Ordinal);
            stack.Clear();
            stack.Push(start);
            visitedTranspose.Add(start);

            while (stack.Count > 0)
            {
                string current = stack.Pop();

                if (transpose.TryGetValue(current, out var backNeighbors))
                {
                    for (int i = 0; i < backNeighbors.Count; i++)
                    {
                        string neighbor = backNeighbors[i];

                        if (!visitedTranspose.Contains(neighbor))
                        {
                            visitedTranspose.Add(neighbor);
                            stack.Push(neighbor);
                        }
                    }
                }
            }

            foreach (var vertex in activeVertices)
            {
                if (!visitedTranspose.Contains(vertex))
                    return false;

            }

            return true;
        }

        private static Dictionary<string, List<string>> BuildTranspose(Graph graph)
        {
            Dictionary<string, List<string>> reverse = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (var vertex in graph.Vertices)
            {
                reverse[vertex] = new List<string>();
            }

            foreach (var pair in graph.AdjList)
            {
                string from = pair.Key;
                List<Edge> edges = pair.Value;

                for (int i = 0; i < edges.Count; i++)
                {
                    string to = edges[i].To;
                    reverse[to].Add(from);
                }
            }

            return reverse;
        }


        public static List<string> BuildCircuitHierholzerUndirected(Graph graph)
        {

            Dictionary<(string A, string B), int> multiplicity = new Dictionary<(string A, string B), int>();

            foreach (var pair in graph.AdjList)
            {
                string from = pair.Key;
                List<Edge> edges = pair.Value;

                for (int i = 0; i < edges.Count; i++)
                {
                    string to = edges[i].To;

                    if (from == to)
                        continue;

                    string a = from;
                    string b = to;

                    if (string.Compare(a, b, StringComparison.Ordinal) > 0)
                    {
                        string tmp = a;
                        a = b;
                        b = tmp;
                    }

                    (string A, string B) key = (a, b);
                    int current = multiplicity.GetValueOrDefault(key, 0);
                    multiplicity[key] = current + 1;
                }
            }

            string start = FindFirstVertexWithEdges(graph);
            Stack<string> pathStack = new Stack<string>();
            List<string> circuit = new List<string>();
            string currentVertex = start;

            while (pathStack.Count > 0 || UndirectedHasAvailableEdge(multiplicity, currentVertex))
            {
                bool hasOut = UndirectedHasAvailableEdge(multiplicity, currentVertex);

                if (!hasOut)
                {
                    circuit.Add(currentVertex);

                    if (pathStack.Count > 0)
                    {
                        currentVertex = pathStack.Pop();
                    }
                }
                else
                {
                    pathStack.Push(currentVertex);

                    string nextVertex = UndirectedPickNextNeighbor(multiplicity, currentVertex);
                    UndirectedConsumeEdge(multiplicity, currentVertex, nextVertex);
                    currentVertex = nextVertex;
                }
            }

            circuit.Add(currentVertex);
            circuit.Reverse();
            return circuit;
        }

        private static string FindFirstVertexWithEdges(Graph graph)
        {
            foreach (var vertex in graph.Vertices)
            {
                if (graph.AdjList.TryGetValue(vertex, out var edges) && edges.Count > 0)
                    return vertex;

            }

            // If there are no edges, return the first vertex (if any)
            foreach (var vertex in graph.Vertices)
                return vertex;


            return string.Empty;
        }

        private static bool UndirectedHasAvailableEdge(Dictionary<(string A, string B), int> multiplicity, string vertex)
        {
            foreach (var entry in multiplicity)
            {
                (string A, string B) key = entry.Key;
                int count = entry.Value;

                if (count > 0 && (key.A == vertex || key.B == vertex))
                    return true;
            }

            return false;
        }

        private static string UndirectedPickNextNeighbor(Dictionary<(string A, string B), int> multiplicity, string vertex)
        {
            foreach (var entry in multiplicity)
            {
                (string A, string B) key = entry.Key;
                int count = entry.Value;

                if (count > 0)
                {
                    if (key.A == vertex)
                        return key.B;

                    if (key.B == vertex)
                        return key.A;

                }
            }

            return vertex;
        }

        private static void UndirectedConsumeEdge(Dictionary<(string A, string B), int> multiplicity, string u, string v)
        {
            string a = u;
            string b = v;

            if (string.Compare(a, b, StringComparison.Ordinal) > 0)
            {
                string tmp = a;
                a = b;
                b = tmp;
            }

            (string A, string B) key = (a, b);

            if (multiplicity.TryGetValue(key, out int count) && count > 0)
                multiplicity[key] = count - 1;

        }

        
        public static List<string> BuildCircuitHierholzerDirected(Graph graph)
        {
            Dictionary<(string U, string V), int> multiplicity = new Dictionary<(string U, string V), int>();

            foreach (var pair in graph.AdjList)
            {
                string from = pair.Key;
                List<Edge> edges = pair.Value;

                for (int i = 0; i < edges.Count; i++)
                {
                    string to = edges[i].To;
                    (string U, string V) key = (from, to);
                    int current = multiplicity.GetValueOrDefault(key, 0);
                    multiplicity[key] = current + 1;
                }
            }

            string start = FindFirstVertexWithEdges(graph);
            Stack<string> pathStack = new Stack<string>();
            List<string> circuit = new List<string>();
            string currentVertex = start;

            while (pathStack.Count > 0 || DirectedHasOutgoingEdge(multiplicity, currentVertex))
            {
                bool hasOut = DirectedHasOutgoingEdge(multiplicity, currentVertex);

                if (!hasOut)
                {
                    circuit.Add(currentVertex);

                    if (pathStack.Count > 0)
                        currentVertex = pathStack.Pop();

                }
                else
                {
                    pathStack.Push(currentVertex);

                    string nextVertex = DirectedPickNextNeighbor(multiplicity, currentVertex);
                    DirectedConsumeEdge(multiplicity, currentVertex, nextVertex);
                    currentVertex = nextVertex;
                }
            }

            circuit.Add(currentVertex);
            circuit.Reverse();
            return circuit;
        }

        private static bool DirectedHasOutgoingEdge(Dictionary<(string U, string V), int> multiplicity, string vertex)
        {
            foreach (var entry in multiplicity)
            {
                (string U, string V) key = entry.Key;
                int count = entry.Value;

                if (count > 0 && key.U == vertex)
                    return true;
            }

            return false;
        }

        private static string DirectedPickNextNeighbor(Dictionary<(string U, string V), int> multiplicity, string vertex)
        {
            foreach (var entry in multiplicity)
            {
                (string U, string V) key = entry.Key;
                int count = entry.Value;

                if (count > 0 && key.U == vertex)
                    return key.V;

            }

            return vertex;
        }

        private static void DirectedConsumeEdge(Dictionary<(string U, string V), int> multiplicity, string from, string to)
        {
            (string U, string V) key = (from, to);

            if (multiplicity.TryGetValue(key, out int count) && count > 0)
                multiplicity[key] = count - 1;

        }
    }
}

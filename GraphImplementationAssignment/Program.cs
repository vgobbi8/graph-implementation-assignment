// See https://aka.ms/new-console-template for more information
using GraphImplementationAssignment;
using GraphImplementationAssignment.Models;

Console.WriteLine("Hello, World!");


var g = new Graph();
g.AddEdge("A", "B");
g.AddEdge("A", "D");

g.AddEdge("B", "C");

g.AddEdge("D", "E");
g.AddEdge("D", "F");

g.AddEdge("E", "F");
g.AddEdge("E", "G");

g.AddEdge("F", "H");


var start = g.Vertices.First(x => x.Name == "A");
var goal = g.Vertices.First(x => x.Name == "H");

var bfs = new BreadthFirstAlgorithm();
var dfs = new DepthFirstAlgorithm();

var path = bfs.ExecuteAlgorithm(g, start, goal);
var pathDFS = dfs.ExecuteAlgorithm(g, start, goal);

path.Print();
pathDFS.Print();

// Heuristic example (plug your own):
double H(Vertex v, Vertex goal) => 0; // admissible but uninformed

var bestFirst = new BestFirstAlgorithm().ExecuteAlgorithm(g, start, goal, H);
bestFirst.Print();

var aStar = new AStarAlgorithm().ExecuteAlgorithm(g, start, goal, H);
aStar.Print();

// If you want the weighted total for the A* path *now*:
var weighted = AStarAlgorithm.ComputeWeightedPathCost(g, aStar.Path);
Console.WriteLine($"Weighted cost: {weighted}");


var mst = new KruskalMST().Build(g);
mst.Print();  // shows edges and total weight


Console.WriteLine($"Directed: {g.Directed}");
Console.WriteLine($"|V|={g.Vertices.Count}");
Console.WriteLine($"~E={g.EdgeCount()}");


foreach (var v in g.Vertices)
{
    Console.WriteLine($"{v}: degree={g.Degree(v)}");
}
Console.WriteLine($"Graph degree (Δ) = {g.GraphDegree()}");



// Eulerian (undirected)
var eulerOK = EulerHamiltonIsomorphism.HasEulerianCircuitUndirected(g);
var circuit = EulerHamiltonIsomorphism.BuildEulerianCircuit(g);
Console.WriteLine($"Eulerian? {eulerOK}");
Console.WriteLine(string.Join(" -> ", circuit));

// Eulerian (directed)
//var eulerDirOK = EulerHamiltonIsomorphism.HasEulerianCircuitDirected(digraph);

// Hamiltonian
var hcycle = EulerHamiltonIsomorphism.HamiltonianCircuit(g);
Console.WriteLine(hcycle.Count == 0 ? "No Hamiltonian cycle" : string.Join(" -> ", hcycle));

// Isomorphism
var (iso, map) = EulerHamiltonIsomorphism.IsIsomorphic(g1, g2);
Console.WriteLine($"Isomorphic? {iso}");
if (iso)
{
    foreach (var kv in map) Console.WriteLine($"{kv.Key} -> {kv.Value}");
}




Console.ReadLine();

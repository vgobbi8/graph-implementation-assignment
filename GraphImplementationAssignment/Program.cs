// See https://aka.ms/new-console-template for more information
using GraphImplementationAssignment.Models;

Console.WriteLine("Hello, World!");


var g = new Graph();
g.AddEdge("A", "B", 2);
g.AddEdge("B", "C", 3);
g.AddEdge("C", "A", 1);

Console.WriteLine($"Directed: {g.Directed}");
Console.WriteLine($"|V|={g.Vertices.Count}");
Console.WriteLine($"~E={g.EdgeCount()}");


foreach (var v in g.Vertices)
{
    Console.WriteLine($"{v}: degree={g.Degree(v)}");
}
Console.WriteLine($"Graph degree (Δ) = {g.GraphDegree()}");

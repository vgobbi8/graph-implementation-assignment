// Top-level Program for the Graph CLI (interactive, no classes)
// Run the exe, choose options in a loop, press 'q' anytime to quit.
// Examples you can type after starting the program are shown in prompts.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GraphImplementationAssignment;             // algorithms namespace
using GraphImplementationAssignment.CLI;         // GraphIO, ResultPrinter
using GraphImplementationAssignment.Models;      // Graph, Vertex, Edge, PathResult

Console.Title = "Graph CLI — Interactive";

string lastFile = "sample.json";  // defaults persist across runs
string lastAlgo = "bfs";
string lastStart = "A";
string lastGoal = "D";
string lastHeuristic = "euclid";  // for best-first / a*

PrintBanner();

while (true)
{
    Console.WriteLine();
    Console.WriteLine("Choose an option (type the number or name):");
    Console.WriteLine("  1) bfs         — Breadth-First Search (edges)");
    Console.WriteLine("  2) dfs         — Depth-First Search (edges)");
    Console.WriteLine("  3) dijkstra    — Shortest path (weights)");
    Console.WriteLine("  4) bestfirst   — Greedy Best-First (heuristic only)");
    Console.WriteLine("  5) astar       — A* (g + h)");
    Console.WriteLine("  6) mst         — Minimum Spanning Tree (Kruskal)");
    Console.WriteLine("  q) quit");


    var algoInput = ReadConsoleLine($"algo", lastAlgo).ToLowerInvariant();
    if (algoInput == "q" || algoInput == "quit") break;

    string algo = algoInput switch
    {
        "1" or "bfs" => "bfs",
        "2" or "dfs" => "dfs",
        "3" or "dijkstra" => "dijkstra",
        "4" or "bestfirst" or "gbfs" => "bestfirst",
        "5" or "astar" or "a*" => "astar",
        "6" or "mst" or "kruskal" => "mst",
        _ => algoInput
    };
    lastAlgo = algo;

    // Gather inputs per algorithm
    Graph graph;
    Dictionary<string, (double x, double y)> coords;

    while (true)
    {
        var file = ReadConsoleLine($"file (.json or .csv)", lastFile);
        if (file.Equals("q", StringComparison.OrdinalIgnoreCase)) goto Quit;
        try
        {
            (graph, coords) = GraphIO.LoadGraph(file);
            lastFile = file;
            break;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Failed to load '{file}': {ex.Message}");
            Console.ResetColor();
        }
    }

    var vByName = graph.Vertices.ToDictionary(v => v.Name, v => v);

    bool needsStartGoal = algo is "bfs" or "dfs" or "dijkstra" or "bestfirst" or "astar";
    Vertex start = null!, goal = null!;

    if (needsStartGoal)
    {
        while (true)
        {
            var s = ReadConsoleLine("start vertex", lastStart);
            if (s.Equals("q", StringComparison.OrdinalIgnoreCase)) goto Quit;
            if (vByName.TryGetValue(s, out start)) { lastStart = s; break; }
            Warn($"Start vertex '{s}' not found. Available: {string.Join(", ", vByName.Keys)}");
        }
        while (true)
        {
            var g = ReadConsoleLine("goal vertex", lastGoal);
            if (g.Equals("q", StringComparison.OrdinalIgnoreCase)) goto Quit;
            if (vByName.TryGetValue(g, out goal)) { lastGoal = g; break; }
            Warn($"Goal vertex '{g}' not found. Available: {string.Join(", ", vByName.Keys)}");
        }
    }

    // Heuristic for GBFS/A*
    Func<Vertex, Vertex, double> H = (v, g) => 0.0; // default
    if (algo is "bestfirst" or "astar")
    {
        var choice = ReadConsoleLine("heuristic [zero|deg|euclid]", lastHeuristic).ToLowerInvariant();
        if (choice == "q") goto Quit;
        lastHeuristic = choice;
        H = choice switch
        {
            "zero" => (v, g) => 0.0,
            "deg" or "degree" => (v, g) => -(graph.AdjList.TryGetValue(v, out var list) ? list.Count : 0),
            "euclid" => (v, g) =>
            {
                if (!coords.TryGetValue(v.Name, out var a) || !coords.TryGetValue(g.Name, out var b)) return 0.0;
                var dx = a.x - b.x; var dy = a.y - b.y; return Math.Sqrt(dx * dx + dy * dy);
            }
            ,
            _ => (v, g) => 0.0
        };
    }

    // Dispatch
    object output;
    try
    {
        switch (algo)
        {
            case "bfs":
                {
                    var r = new BreadthFirstAlgorithm().ExecuteAlgorithm(graph, start, goal);
                    ResultPrinter.PrintPathResult("bfs", r, graph);
                    output = ResultPrinter.ToJsonObject("bfs", r, graph);
                    break;
                }
            case "dfs":
                {
                    var r = new DepthFirstAlgorithm().ExecuteAlgorithm(graph, start, goal);
                    ResultPrinter.PrintPathResult("dfs", r, graph);
                    output = ResultPrinter.ToJsonObject("dfs", r, graph);
                    break;
                }
            case "dijkstra":
                {
                    var r = new DijkstraAlgorithm().ExecuteAlgorithm(graph, start, goal);
                    ResultPrinter.PrintPathResult("dijkstra", r, graph, includeWeighted: true);
                    output = ResultPrinter.ToJsonObject("dijkstra", r, graph, includeWeighted: true);
                    break;
                }
            case "bestfirst":
            case "gbfs":
                {
                    var r = new BestFirstAlgorithm().ExecuteAlgorithm(graph, start, goal, H);
                    ResultPrinter.PrintPathResult("bestfirst", r, graph);
                    output = ResultPrinter.ToJsonObject("bestfirst", r, graph);
                    break;
                }
            case "astar":
            case "a*":
                {
                    var r = new AStarAlgorithm().ExecuteAlgorithm(graph, start, goal, H);
                    ResultPrinter.PrintPathResult("astar", r, graph, includeWeighted: true);
                    output = ResultPrinter.ToJsonObject("astar", r, graph, includeWeighted: true);
                    break;
                }
            case "mst":
            case "kruskal":
                {
                    var mst = new KruskalMST().Build(graph);
                    ResultPrinter.PrintMst(mst);
                    output = ResultPrinter.ToJsonObject("mst", mst);
                    break;
                }
            default:
                Warn($"Unknown algorithm '{algo}'. Try bfs|dfs|dijkstra|bestfirst|astar|mst");
                continue; // back to menu
        }
    }
    catch (Exception ex)
    {
        Warn($"Error while running '{algo}': {ex.Message}");
        continue; // back to menu
    }

    // Save?
    var save = ReadConsoleLine("Save output JSON? [y/N]", "n").ToLowerInvariant();
    if (save is "y" or "yes")
    {
        var outPath = ReadConsoleLine("output file", $"{algo}_result.json");
        if (!outPath.Equals("q", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(output, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(outPath, json);
                Console.WriteLine($"Saved output to {outPath}");
            }
            catch (Exception ex)
            {
                Warn($"Failed to save output: {ex.Message}");
            }
        }
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("Press ENTER to continue or 'q' to quit...");
    Console.ResetColor();
    var cont = Console.ReadLine();
    if (string.Equals(cont, "q", StringComparison.OrdinalIgnoreCase)) break;
}

Quit:
Console.WriteLine("Bye!");
return 0;

// ----------------- helpers -----------------
static string ReadConsoleLine(string label, string? def = null)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write(def == null ? $"> {label}: " : $"> {label} [{def}]: ");
    Console.ResetColor();
    var s = Console.ReadLine();
    return string.IsNullOrWhiteSpace(s) && def != null ? def : (s ?? string.Empty);
}

static void Warn(string msg)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(msg);
    Console.ResetColor();
}

static void PrintBanner()
{
    Console.WriteLine("============================================");
    Console.WriteLine("          Graph CLI — Interactive");
    Console.WriteLine("============================================");
    Console.WriteLine("Type 'q' at any prompt to quit. Defaults are in [brackets].");
}
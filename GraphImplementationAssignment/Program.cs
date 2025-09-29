// Top-level Program for the Graph CLI (interactive, no classes)
// Run the exe, choose options in a loop, press 'q' anytime to quit.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GraphImplementationAssignment;             // algorithms namespace
using GraphImplementationAssignment.CLI;         // GraphIO, ResultPrinter
using GraphImplementationAssignment.Models;      // Graph, Vertex, Edge, PathResult

Console.Title = "Graph CLI — Interactive";

string lastFile = "sample.json";  // persist last choice
string lastAlgo = "bfs";
string lastStart = "A";
string lastGoal = "H";
string lastHeuristic = "euclid";  // for best-first / a*

string DataPath(string fileName)
{
    var baseDir = AppContext.BaseDirectory;

    return Path.Combine(baseDir, "graphs", fileName);
}
string DataOutputPath(string fileName)
{
    var baseDir = AppContext.BaseDirectory;
    var outputDir = Path.Combine(baseDir, "graphs_outputs");
    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }

    return Path.Combine(baseDir, "graphs_outputs", fileName);
}


PrintBanner();

while (true)
{
    bool chooseAlgorithm = true;
    string chosenAlgo = "null";
    while (chooseAlgorithm)
    {
        Console.WriteLine();
        Console.WriteLine("Choose an option (type the number or name):");
        Console.WriteLine("  1) bfs         — Breadth-First Search (edges)");
        Console.WriteLine("  2) dfs         — Depth-First Search (edges)");
        Console.WriteLine("  3) dijkstra    — Shortest path (weights)");
        Console.WriteLine("  4) bestfirst   — Greedy Best-First (heuristic only)");
        Console.WriteLine("  5) astar       — A* (g + h)");
        Console.WriteLine("  6) mst         — Minimum Spanning Tree (Kruskal)");
        Console.WriteLine("  7) euleru     — Eulerian circuit? (Undirected) + optional circuit");
        Console.WriteLine("  8) eulerd     — Eulerian circuit? (Directed) + optional circuit");
        Console.WriteLine("  9) hamilton   — Hamiltonian cycle (backtracking)");
        Console.WriteLine(" 10) iso        — Graph isomorphism (g1 vs g2)");
        Console.WriteLine("  q) quit");

        var algoInput = ReadConsoleLine($"algo", lastAlgo).ToLowerInvariant();
        if (algoInput == "q" || algoInput == "quit") break;
        chooseAlgorithm = false;
        chosenAlgo = algoInput switch
        {
            "1" or "bfs" => "bfs",
            "2" or "dfs" => "dfs",
            "3" or "dijkstra" => "dijkstra",
            "4" or "bestfirst" or "gbfs" => "bestfirst",
            "5" or "astar" or "a*" => "astar",
            "6" or "mst" or "kruskal" => "mst",
            "7" or "euleru" => "euleru",
            "8" or "eulerd" => "eulerd",
            "9" or "hamilton" => "hamilton",
            "10" or "iso" or "isomorphic" => "iso",
            _ => "invalid"
        };
        if (string.Equals(chosenAlgo, "invalid", StringComparison.OrdinalIgnoreCase))
        {
            Warn("Invalid algorithm chosen!");
            chooseAlgorithm = true;
        }
    }

    lastAlgo = chosenAlgo;



    string suggestedFile = chosenAlgo switch
    {
        "bfs" or "dfs" => DataPath("bfs_dfs.json"),
        "dijkstra" or "bestfirst" or "astar" => DataPath("dijkstra_astar_bestfirst.json"),
        "mst" => DataPath("mst_kruskal.json"),
        "euleru" => DataPath("euler_undirected.json"),
        "eulerd" => DataPath("euler_directed.json"),
        "hamilton" => DataPath("hamiltonian.json"),
        "iso" => DataPath("iso_g1.json"),
        _ => lastFile
    };

    string defaultFile = suggestedFile ?? lastFile;

    Graph graph;
    Dictionary<string, (double x, double y)> coords;
    while (true)
    {
        string defaultFileName = Path.GetFileName(defaultFile);
        string suggestedFileName = Path.GetFileName(suggestedFile);

        var fileName = ReadConsoleLine($"file (.json or .csv) [suggested for {chosenAlgo}: {suggestedFileName}]", defaultFileName);
        var file = DataPath(fileName);
        if (file.Equals("q", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
        try
        {
            graph = GraphIO.LoadFromJson(file); // <— your loader
            lastFile = file;
            break;
        }
        catch (Exception ex)
        {
            Warn($"Failed to load '{file}': {ex.Message}");
        }
    }

    var vByName = graph.Vertices.ToDictionary(v => v, v => v);


    ResultPrinter.PrintGraphOverview(graph);
    ResultPrinter.PrintAdjacencyList(graph, showWeights: true);

    bool needsStartGoal = chosenAlgo is "bfs" or "dfs" or "dijkstra" or "bestfirst" or "astar";
    string? start = null!;
    string? goal = null!;

    if (needsStartGoal)
    {
        while (true)
        {
            var s = ReadConsoleLine("start vertex", lastStart);
            if (s.Equals("q", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
            if (vByName.TryGetValue(s, out start)) { lastStart = s; break; }
            Warn($"Start vertex '{s}' not found. Available: {string.Join(", ", vByName.Keys)}");
        }
        while (true)
        {
            var g = ReadConsoleLine("goal vertex", lastGoal);
            if (g.Equals("q", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
            if (vByName.TryGetValue(g, out goal)) { lastGoal = g; break; }
            Warn($"Goal vertex '{g}' not found. Available: {string.Join(", ", vByName.Keys)}");
        }
    }

    // Heuristic for GBFS/A*
    Func<string, string, double> H = (v, g) => 0.0; // default
    if (chosenAlgo is "bestfirst" or "astar")
    {
        var choice = ReadConsoleLine("heuristic [zero|deg|euclid]", lastHeuristic).ToLowerInvariant();
        if (choice == "q") Environment.Exit(0);
        lastHeuristic = choice;
        H = choice switch
        {
            "zero" => (v, g) => 0.0,
            "deg" or "degree" => (v, g) => -(graph.AdjList.TryGetValue(v, out var list) ? list.Count : 0),
            "euclid" => (v, g) =>
            {
                if (!graph.Coords.TryGetValue(v, out var a) || !graph.Coords.TryGetValue(g, out var b)) return 0.0;
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
        switch (chosenAlgo)
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
            case "euleru":
                {
                    if (graph.Directed)
                        Warn("Graph is directed; load an UNDIRECTED graph for euleru.");
                    else
                    {
                        bool ok = Eulerian.HasCircuitUndirected(graph);
                        Console.WriteLine("Algorithm: EULERIAN (Undirected)");
                        Console.WriteLine($"Eulerian circuit exists: {ok}");
                        if (ok)
                        {
                            var circuit = Eulerian.BuildCircuit(graph);
                            Console.WriteLine("Circuit:");
                            Console.WriteLine("  " + string.Join(" -> ", circuit));
                        }
                    }
                    output = new { algorithm = "euleru" };
                    break;
                }
            case "eulerd":
                {
                    if (!graph.Directed)
                        Warn("Graph is undirected; load a DIRECTED graph for eulerd.");
                    else
                    {
                        bool ok = Eulerian.HasCircuitDirected(graph);
                        Console.WriteLine("Algorithm: EULERIAN (Directed)");
                        Console.WriteLine($"Eulerian circuit exists: {ok}");
                        if (ok)
                        {
                            var circuit = Eulerian.BuildCircuit(graph);
                            Console.WriteLine("Circuit:");
                            Console.WriteLine("  " + string.Join(" -> ", circuit));
                        }
                    }
                    output = new { algorithm = "eulerd" };
                    break;
                }
            case "hamilton":
                {
                    Console.WriteLine("Algorithm: HAMILTONIAN");
                    var cycle = Hamiltonian.FindCycle(graph);
                    Console.WriteLine($"Found: {(cycle.Count > 0)}");
                    Console.WriteLine("Cycle: " + (cycle.Count == 0 ? "-" : string.Join(" -> ", cycle)));
                    output = new { algorithm = "hamilton", found = cycle.Count > 0, cycle };
                    break;
                }
            case "iso" or "isomorphic":
                {
                    Graph g2; Dictionary<string, (double, double)> coords2;
                    while (true)
                    {
                        var filename2 = ReadConsoleLine("second file for isomorphism (g2)", "iso_g2.json");
                        var file2 = DataPath(filename2);
                        if (file2.Equals("q", StringComparison.OrdinalIgnoreCase)) return 0;
                        try { g2 = GraphIO.LoadFromJson(file2); break; }
                        catch (Exception ex) { Warn($"Failed to load '{file2}': {ex.Message}"); }
                    }

                    var (ok, mapping) = Isomorphism.AreIsomorphic(graph, g2);
                    Console.WriteLine("Algorithm: ISOMORPHISM");
                    Console.WriteLine($"Isomorphic: {ok}");
                    if (ok && mapping.Count > 0)
                    {
                        Console.WriteLine("Mapping (g1 -> g2):");
                        foreach (var kv in mapping) Console.WriteLine($"  {kv.Key} -> {kv.Value}");
                    }
                    output = new { algorithm = "iso", isomorphic = ok, mapping };
                    break;
                }
            default:
                Warn($"Unknown algorithm '{chosenAlgo}'. Try bfs|dfs|dijkstra|bestfirst|astar|mst");
                continue; // back to menu
        }
    }
    catch (Exception ex)
    {
        Warn($"Error while running '{chosenAlgo}': {ex.Message}");
        continue; // back to menu
    }

    // Save?
    var save = ReadConsoleLine("Save output JSON? [y/N]", "n").ToLowerInvariant();
    if (save is "y" or "yes")
    {
        var outFileName = ReadConsoleLine("output file", $"{chosenAlgo}_result.json");
        var outPath = DataOutputPath(outFileName);
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
    if (string.Equals(cont, "q", StringComparison.OrdinalIgnoreCase))
        break;
}

Console.WriteLine("Bye!");
return 0;

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

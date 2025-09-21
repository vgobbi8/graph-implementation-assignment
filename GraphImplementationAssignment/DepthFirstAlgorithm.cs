using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphImplementationAssignment
{
    public class DepthFirstAlgorithm
    {

        public PathResult ExecuteAlgorithm(Graph graph, Vertex start, Vertex goal)
        {
            HashSet<Vertex> visited = [];
            Stack<Vertex> stack = new();

            visited.Add(start);
            stack.Push(start);
            var parent = new Dictionary<string, string>();
            while (stack.Count > 0)
            {
                Vertex v = stack.Pop();
                if (v == goal)
                    return PathResult.BuildPathResult(start.Name, goal.Name, parent); //Build path result after
                foreach (var neighboor in graph.NeigboorsOf(v))
                {
                    if (visited.Add(neighboor))
                    {
                        parent[neighboor.Name] = v.Name;
                        stack.Push(neighboor);
                    }
                }
            }
            return PathResult.NotFound();
        }
    }
}

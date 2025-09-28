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

        public PathResult ExecuteAlgorithm(Graph graph, string start, string goal)
        {
            HashSet<string> visited = [];
            Stack<string> stack = new();

            visited.Add(start);
            stack.Push(start);
            var parent = new Dictionary<string, string>();
            while (stack.Count > 0)
            {
                string v = stack.Pop();
                if (v == goal)
                    return PathResult.BuildPathResult(start, goal, parent); //Build path result after
                foreach (var neighboor in graph.NeigboorsOf(v))
                {
                    if (visited.Add(neighboor))
                    {
                        parent[neighboor] = v;
                        stack.Push(neighboor);
                    }
                }
            }
            return PathResult.NotFound();
        }
    }
}

using GraphImplementationAssignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphImplementationAssignment
{
    public class BreadthFirstAlgorithm
    {



        public PathResult ExecuteAlgorithm(Graph graph, string start, string goal)
        {
            HashSet<string> visited = [];
            Queue<string> queue = new Queue<string>();

            visited.Add(start);
            queue.Enqueue(start);
            var parent = new Dictionary<string, string>();
            while (queue.Count > 0)
            {
                string v = queue.Dequeue();
                if (v == goal)
                    return PathResult.BuildPathResult(start, goal, parent); //Build path result after
                foreach (var neighboor in graph.NeigboorsOf(v))
                {
                    if (visited.Add(neighboor))
                    {
                        parent[neighboor] = v;
                        queue.Enqueue(neighboor);
                    }
                }
            }
            return PathResult.NotFound();
        }



    }
}

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



        public PathResult ExecuteAlgorithm(Graph graph, Vertex start, Vertex goal)
        {
            HashSet<Vertex> visited = [];
            Queue<Vertex> queue = new Queue<Vertex>();

            visited.Add(start);
            queue.Enqueue(start);
            var parent = new Dictionary<string, string>();
            while (queue.Count > 0)
            {
                Vertex v = queue.Dequeue();
                if (v == goal)
                    return PathResult.BuildPathResult(start.Name, goal.Name, parent); //Build path result after
                foreach (var neighboor in graph.NeigboorsOf(v))
                {
                    if (visited.Add(neighboor))
                    {
                        parent[neighboor.Name] = v.Name;
                        queue.Enqueue(neighboor);
                    }
                }
            }
            return PathResult.NotFound();
        }



    }
}

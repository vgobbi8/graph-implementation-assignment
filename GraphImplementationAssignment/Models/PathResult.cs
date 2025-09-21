using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphImplementationAssignment.Models
{
    public record PathResult(List<string> Path, double Cost, bool Found)
    {
        public static PathResult NotFound() => new(new List<string>(), int.MaxValue, false);

        public static PathResult BuildPathResult(string start, string goal, Dictionary<string, string> parent)
        {
            if (start == goal) return new PathResult(new List<string> { start }, 0, true);

            if (!parent.ContainsKey(goal)) return PathResult.NotFound();


            var path = new List<string>();
            var cur = goal;
            path.Add(cur);

            while (cur != start)
            {
                cur = parent[cur];
                path.Add(cur);
            }

            path.Reverse();
            var cost = path.Count - 1;
            return new PathResult(path, cost, true);
        }

        public void Print()
        {
            Console.WriteLine($"Path: {string.Join("-->", Path)}");
            Console.WriteLine($"Cost: {Cost}");
            Console.WriteLine($"Found: {Found}");
        }
    }
}

using System.Collections;
using System.Collections.Generic;

namespace Evade.Pathfinding
{
    public class Path<node> : IEnumerable<node>
    {
        private Path(node lastStep, Path<node> previousSteps, double totalCost)
        {
            LastStep = lastStep;
            PreviousSteps = previousSteps;
            TotalCost = totalCost;
        }

        public Path(node start) : this(start, null, 0)
        {
        }

        public node LastStep { get; private set; }
        public Path<node> PreviousSteps { get; private set; }
        public double TotalCost { get; private set; }

        public IEnumerator<node> GetEnumerator()
        {
            for (var p = this; p != null; p = p.PreviousSteps)
                yield return p.LastStep;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Path<node> AddStep(node step, double stepCost)
        {
            return new Path<node>(step, this, TotalCost + stepCost);
        }
    }
}
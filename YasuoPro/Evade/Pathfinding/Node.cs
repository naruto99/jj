using System.Collections.Generic;
using SharpDX;

namespace Evade.Pathfinding
{
    public class Node
    {
        public List<Node> Neightbours;
        public Vector2 Point;

        public Node(Vector2 point)
        {
            Point = point;
            Neightbours = new List<Node>();
        }
    }
}
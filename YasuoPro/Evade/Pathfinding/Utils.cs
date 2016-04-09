﻿using System.Collections.Generic;
using LeagueSharp.Common;
using SharpDX;

namespace Evade.Pathfinding
{
    internal class Utils
    {
        public static bool IsVertexConcave(List<Vector2> vertices, int vertex)
        {
            var current = vertices[vertex];
            var next = vertices[(vertex + 1)%vertices.Count];
            var previous = vertices[vertex == 0 ? vertices.Count - 1 : vertex - 1];

            var left = new Vector2(current.X - previous.X, current.Y - previous.Y);
            var right = new Vector2(next.X - current.X, next.Y - current.Y);

            var cross = (left.X*right.Y) - (left.Y*right.X);

            return cross < 0;
        }

        public static bool CanReach(Vector2 start, Vector2 end, List<Geometry.Polygon> polygons, bool checkWalls = false)
        {
            if (start == end)
            {
                return false;
            }

            //TODO Disable if the distance to the start position is high
            if (checkWalls)
            {
                var nPoints = 2;
                var step = start.LSDistance(end)/nPoints;

                var direction = (end - start).LSNormalized();
                for (var i = 0; i <= nPoints; i++)
                {
                    var p = start + i*step*direction;
                    if (p.IsWall())
                    {
                        return false;
                    }
                }
            }


            foreach (var polygon in polygons)
            {
                for (var i = 0; i < polygon.Points.Count; i++)
                {
                    var a = polygon.Points[i];
                    var b = polygon.Points[i == polygon.Points.Count - 1 ? 0 : i + 1];

                    if (Evade.Utils.LineSegmentsCross(start, end, a, b))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
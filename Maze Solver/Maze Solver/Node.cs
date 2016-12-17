using System;
using System.Collections.Generic;
using System.Drawing;

namespace Maze_Solver
{
    class Node : IEquatable<Node>
    {
        public Point point = new Point();
        public int gValue;
        public int hValue;
        public Node parent;

        public Node(int x, int y)
        {
            point.X = x;
            point.Y = y;
        }

        public int fValue()
        {
            return gValue + hValue;
        }

        public bool Equals(Node other)
        {
            return this.point.X == other.point.X &&
                   this.point.Y == other.point.Y;
        }
    }
}

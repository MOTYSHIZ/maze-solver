/** Author: Justin Ordonez 
 * 
 * This is the Node class that forms the Node objects that represent each evaluated point on the maze.
 * 
 * The fValue, hValue, and gValues are all for A* pathfinding algorithm, and a Node's parent is
 * tracked so that a final solution path can be retraced once the maze is solved. The class extends
 * IEquatable so that the Equals function for List.Contains can be specified.
 * 
 * **/

using System;
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

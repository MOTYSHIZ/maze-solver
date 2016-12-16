﻿using System;
using System.Drawing;
using System.Collections.Generic;

namespace Maze_Solver
{
    public class MazeSolveMain
    {
        private static string path = Environment.CurrentDirectory + @"\input-mazes\maze3.png";
        private static Bitmap image = new Bitmap(path, true);
        private static Graphics gManipulator = Graphics.FromImage(image);
        private static Pen greenPen = new Pen(Color.Green, 2);
        private static int blockSize = 0;

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

        static void Main(string[] args)
        {
            Console.WriteLine("Image size: " + image.PhysicalDimension);
            
            Node start = findStart();
            Node goal = findGoal();
            if (start == null) Console.WriteLine("Unable to find Start Location.");
            else if (goal == null) Console.WriteLine("Unable to find Goal Location.");
            else if (findPath(start, goal) == true) Console.WriteLine("Maze Solved!");
            else Console.WriteLine("Maze not Solved... D:");

            clearTracers();
            image.Save(Environment.CurrentDirectory + @"\solved-mazes\maze1solved.png");
            Console.ReadLine();
        }

        static Node findStart()
        {
            Node coord = new Node(0, 0);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if(image.GetPixel(x, y) == Color.FromArgb(255, 255, 0,0))
                    {
                        findBlockSize(x, y);
                        x += (blockSize / 2) - 1;
                        y += (blockSize / 2) - 1;
                        coord.point.X = x;
                        coord.point.Y = y;
                        return coord;
                    }
                }
            }
            return null;
        }

        //Finds the block size based on the width of the starting point.
        static void findBlockSize(int currX, int currY)
        {
            while(image.GetPixel(currX, currY) == Color.FromArgb(255, 255, 0, 0))
            {
                blockSize++;
                currX++;
            }
            Console.WriteLine("block size = " + blockSize);
        }

        //Currently only used to calculate for A*
        static Node findGoal()
        {
            Node goal = new Node(0,0);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y) == Color.FromArgb(255, 0, 0, 255))
                    {
                        x += (blockSize / 2) - 1;
                        y += (blockSize / 2) - 1;
                        goal.point.X = x;
                        goal.point.Y = y;
                        return goal;
                    }
                }
            }
            return null;
        }

        //The main path finding function.
        static bool findPath(Node start, Node target)
        {
            List<Node> frontier = new List<Node>();
            frontier.Add(start);
            
            while (frontier.Count > 0)
            {
                Node current = frontier[0];
                for (int i = 1; i < frontier.Count; i++)
                {
                    if (frontier[i].fValue() < current.fValue() 
                        || frontier[i].fValue() == current.fValue() && frontier[i].hValue < current.hValue)
                    {
                        current = frontier[i];
                    }
                }
                frontier.Remove(current);
                
                //If at goal
                if (current == target || image.GetPixel(current.point.X, current.point.Y) == Color.FromArgb(255,0,0,255))
                {
                    List<Node> path = retrace(start, current);
                    Point[] nodes = new Point[path.Count];
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        nodes[i] = path[i].point;
                    }

                    gManipulator.DrawLines(greenPen, nodes);
                    return true;
                }

                image.SetPixel(current.point.X, current.point.Y, Color.FromArgb(255, 255, 0, 255));

                foreach (Node neighbor in getNeighbors(current))
                {
                    if (image.GetPixel(neighbor.point.X, neighbor.point.Y) == Color.FromArgb(255, 255, 0, 255)) continue;

                    int newMovementCostToNeighbor = current.gValue + findDistance(current, neighbor);
                    if(newMovementCostToNeighbor < neighbor.gValue || !frontier.Contains(neighbor))
                    {
                        neighbor.gValue = newMovementCostToNeighbor;
                        neighbor.hValue = findDistance(neighbor, target);
                        if (neighbor.hValue < 50) blockSize = 1;
                        neighbor.parent = current;

                        if (!frontier.Contains(neighbor)) frontier.Add(neighbor);
                    }
                }
            }
            return false;
        }

        /*Used to find neighbors of the current node, so that they can be added to the
          frontier heap for the A* pathfinding algorithm. */
        static List<Node> getNeighbors(Node current)
        {
            List<Node> neighbors = new List<Node>();

            for (int x = -blockSize; x <= blockSize; x += blockSize)
            {
                for (int y = -blockSize; y <= blockSize; y += blockSize)
                {
                    //Don't consider own point, only neighbors.
                    if (x == 0 && y == 0) continue;

                    int xCoord = current.point.X + x;
                    int yCoord = current.point.Y + y;

                    if (xCoord >= 0 && xCoord < image.Width && yCoord >= 0 && yCoord < image.Height
                        && checkIfClear(current.point.X, current.point.Y, x / blockSize, y / blockSize))
                    {
                        neighbors.Add(new Node(xCoord, yCoord));
                    }
                }
            }
            return neighbors;
        }

        //checks if the way is clear for traversal.
        static bool checkIfClear(int currX, int currY, int expandX, int expandY)
        {
            int blockSizeTemp = blockSize;

            while (blockSizeTemp >= 0 && currX >= 0 && currX < image.Width && currY >= 0 && currY < image.Height)
            {
                if (image.GetPixel(currX, currY).GetBrightness() < .5) return false;

                if(expandX != 0 && expandY != 0)
                {
                    if (image.GetPixel(currX + expandX, currY).GetBrightness() < .5 && image.GetPixel(currX, currY + expandY).GetBrightness() < .5) return false;
                }

                currX += expandX;
                currY += expandY;
                blockSizeTemp--;
            }
            return true;
        }

        static int findDistance(Node current, Node target)
        {
            int xDistance = Math.Abs(current.point.X - target.point.X);
            int yDistance = Math.Abs(current.point.Y - target.point.Y);

            if (xDistance > yDistance) return 14 * yDistance + 10 * (xDistance - yDistance);
            return 14 * xDistance + 10 * (yDistance - xDistance);
        }

        static List<Node> retrace(Node start, Node target)
        {
            List<Node> path = new List<Node>();
            Node current = target;

            while(current != start)
            {
                path.Add(current);
                current = current.parent;
            }
            path.Add(start);
            path.Reverse();
            return path;
        }

        //Clears the purple pixels from the map.
        static void clearTracers()
        {
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    if (image.GetPixel(i, j) == Color.FromArgb(255, 255, 0, 255))
                    {
                        image.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                }
            }
        }
    }
}

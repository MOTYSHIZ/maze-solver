using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;

namespace Maze_Solver
{
    public class MazeSolveMain
    {
        private static string path = Environment.CurrentDirectory + @"\input-mazes\maze1.png";
        private static Bitmap image = new Bitmap(path, true);
        private static Graphics gManipulator = Graphics.FromImage(image);
        private static Pen greenPen = new Pen(Color.Green, 1);
        private static Point goal;
        private static int blockSize = 0;
        private static List<Node> points = new List<Node>();
        enum Directions{UP, UPLEFT, UPRIGHT, DOWN, DOWNLEFT, DOWNRIGHT, LEFT,RIGHT};

        class Node : IEquatable<Node>
        {
            public Point point = new Point();
            public int gValue;
            public int hValue;
            public Point parent;

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

            Node coord = findStart();
            //findGoal();
            findPath(coord);
            image.Save(Environment.CurrentDirectory + @"\solved-mazes\maze1solved.png");
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
            return coord;
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
        static void findGoal()
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y) == Color.FromArgb(255, 0, 0, 255))
                    {
                        x += (blockSize / 2) - 1;
                        y += (blockSize / 2) - 1;
                        goal.X = x;
                        goal.Y = y;
                        return;
                    }
                }
            }
        }

        //The main path finding function.
        static Color findPath(Node current)
        {
            Color currentPixel = image.GetPixel(current.point.X, current.point.Y);
            Color tempPixel;

            //If black, Blue, or if has already been visited.
            if (currentPixel == Color.FromArgb(255, 0, 0, 0)
                || currentPixel == Color.FromArgb(255, 0, 0, 255)
                || points.Contains(current)) return currentPixel;

            //image.SetPixel(current.point.X, current.point.Y, Color.FromArgb(255, 255, 0, 255));
            points.Add(current);

            foreach(Node neighbor in getNeighbors(current))
            {
                tempPixel = findPath(neighbor);
                if (tempPixel == Color.FromArgb(255, 0, 0, 255))
                {
                    gManipulator.DrawLine(greenPen, current.point, neighbor.point);
                    currentPixel = tempPixel;
                }
            }

            return currentPixel;
        }

        //static List<Point> getNeighbors(Point current)
        //{
        //    List<Point> neighbors = new List<Point>();

        //    for (int x = -1; x <= 1; x++)
        //    {
        //        for (int y = -1; y <= 1; y++)
        //        {
        //            if (x == 0 && y == 0) continue;

        //            int xCoord = current.X + x;
        //            int yCoord = current.Y + y;

        //            if (xCoord >= 0 && xCoord < image.Width && yCoord >= 0 && yCoord < image.Height)
        //            {
        //                neighbors.Add(new Point(xCoord, yCoord));
        //            }
        //        }
        //    }

        //    return neighbors;
        //}

        static List<Node> getNeighbors(Node current)
        {
            List<Node> neighbors = new List<Node>();

            for (int x = -blockSize; x <= blockSize; x += blockSize)
            {
                for (int y = -blockSize; y <= blockSize; y += blockSize)
                {
                    //Don't consider own point or diagonal neighbors.
                    if (x == 0 && y == 0) continue;
                    if (x == -blockSize && y == -blockSize) continue;
                    if (x == blockSize && y == -blockSize) continue;
                    if (x == -blockSize && y == blockSize) continue;
                    if (x == blockSize && y == blockSize) continue;

                    int xCoord = current.point.X + x;
                    int yCoord = current.point.Y + y;

                    if (xCoord >= 0 && xCoord < image.Width && yCoord >= 0 && yCoord < image.Height
                        && checkIfClear(current.point.X, current.point.Y , x/blockSize, y/blockSize))
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
                if (image.GetPixel(currX, currY) == Color.FromArgb(255, 0, 0, 0)) return false;

                currX += expandX;
                currY += expandY;
                blockSizeTemp--;
            }
            return true;
        }

        static double findDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2) );
        }
    }
}

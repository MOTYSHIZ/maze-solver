using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;

namespace Maze_Solver
{
    public class MazeSolveMain
    {
        private static string path = Environment.CurrentDirectory + @"\input-mazes\maze4.png";
        private static Bitmap image = new Bitmap(path, true);
        private static Graphics gManipulator = Graphics.FromImage(image);
        private static Pen greenPen = new Pen(Color.Green, 1);
        private static Point goal;
        private static int blockSize = 0;
        private static List<Point> points = new List<Point>();
        enum Directions{UP, UPLEFT, UPRIGHT, DOWN, DOWNLEFT, DOWNRIGHT, LEFT,RIGHT};

        static void Main(string[] args)
        {
            Console.WriteLine("Image size: " + image.PhysicalDimension);

            Point coord = findStart();
            findGoal();
            findPath(coord);
            image.Save(Environment.CurrentDirectory + @"\solved-mazes\maze1solved.png");
        }
         
        static Point findStart()
        {
            Point coord = new Point(0, 0);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if(image.GetPixel(x, y) == Color.FromArgb(255, 255, 0,0))
                    {
                        findBlockSize(x, y);
                        x += (blockSize / 2) - 1;
                        y += (blockSize / 2) - 1;
                        coord.X = x;
                        coord.Y = y;
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
        static Color findPath(Point current)
        {
            Color currentPixel = image.GetPixel(current.X, current.Y);
            Color tempPixel;

            //If black, Blue, or if has already been visited.
            if (currentPixel == Color.FromArgb(255, 0, 0, 0)
                || currentPixel == Color.FromArgb(255, 0, 0, 255)
                || points.Contains(current)) return currentPixel;

            //image.SetPixel(current.X, current.Y, Color.FromArgb(255, 255, 0, 255));
            points.Add(current);

            //aStarEval(currX, currY);

            foreach(Point neighbor in getNeighbors(current))
            {
                tempPixel = findPath(neighbor);
                if (tempPixel == Color.FromArgb(255, 0, 0, 255))
                {
                    gManipulator.DrawLine(greenPen, current, neighbor);
                    currentPixel = tempPixel;
                }
            }

            //The 4 functions below will recursively call findpath, with different starting point values 
            // if breadth-first, should probably perform check on each child before recurs call
            // This could probably be done with Booleans to trigger the recurs calls after the initial children have been checked.
            // 

            //Up
            //Go Deeper into up move, set base case as hit wall or goal, so base case will be in findpath()

            //Down
            //Left
            //Right

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

        static List<Point> getNeighbors(Point current)
        {
            List<Point> neighbors = new List<Point>();

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

                    int xCoord = current.X + x;
                    int yCoord = current.Y + y;

                    if (xCoord >= 0 && xCoord < image.Width && yCoord >= 0 && yCoord < image.Height
                        && checkIfClear(current.X, current.Y , x/blockSize, y/blockSize))
                    {
                        neighbors.Add(new Point(xCoord, yCoord));
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

        //Incomplete, have to find a way to add this distance formula to the amount traversed already.
        static double aStarEval(int currX, int currY)
        {
            return Math.Sqrt(Math.Pow(goal.X - currX, 2) + Math.Pow(goal.Y - currY, 2) );
        }

    }
}

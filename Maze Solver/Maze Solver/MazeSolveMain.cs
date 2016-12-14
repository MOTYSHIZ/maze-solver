using System;
using System.Drawing;
using System.Collections;

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
        private static ArrayList points = new ArrayList();
        enum Directions{UP,DOWN,LEFT,RIGHT};

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

        //checks if the way is clear for traversal.
        static bool checkIfClear(int currX, int currY, int direction)
        {
            int blockSizeTemp = blockSize;

            if(direction == (int)Directions.UP)
            {
                while(blockSizeTemp > 0)
                {
                    if (image.GetPixel(currX, currY) == Color.FromArgb(255, 0, 0, 0)) return false;
                    currY--;
                    blockSizeTemp--;
                }
            }
            else if(direction == (int)Directions.DOWN)
            {
                while (blockSizeTemp > 0)
                {
                    if (image.GetPixel(currX, currY) == Color.FromArgb(255, 0, 0, 0)) return false;
                    currY++;
                    blockSizeTemp--;
                }
            }
            else if (direction == (int)Directions.LEFT)
            {
                while (blockSizeTemp > 0)
                {
                    if (image.GetPixel(currX, currY) == Color.FromArgb(255, 0, 0, 0)) return false;
                    currX--;
                    blockSizeTemp--;
                }
            }
            else if (direction == (int)Directions.RIGHT)
            {
                while (blockSizeTemp > 0)
                {
                    if (image.GetPixel(currX, currY) == Color.FromArgb(255, 0, 0, 0)) return false;
                    currX++;
                    blockSizeTemp--;
                }
            }

            return true;
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
            Point tempPoint;

            //If black, Blue, or if has already been visited.
            if (currentPixel == Color.FromArgb(255, 0, 0, 0)
                || currentPixel == Color.FromArgb(255, 0, 0, 255)
                || points.Contains(current)) return currentPixel;

            image.SetPixel(current.X, current.Y, Color.FromArgb(255, 255, 0, 255));
            points.Add(current);

            //aStarEval(currX, currY);

            //up
            if (current.Y - blockSize > 0 && checkIfClear(current.X, current.Y, (int)Directions.UP))
            {
                tempPoint = new Point(current.X, current.Y - blockSize);
                tempPixel = findPath(tempPoint);
                if (tempPixel == Color.FromArgb(255, 0, 0, 255))
                {
                    gManipulator.DrawLine(greenPen, current, tempPoint);
                    currentPixel = tempPixel;
                }
            }
            //down
            if (current.Y + blockSize < image.Height - 1
                && checkIfClear(current.X, current.Y, (int)Directions.DOWN))
            {
                tempPoint = new Point(current.X, current.Y + blockSize);
                tempPixel = findPath(tempPoint);
                if (tempPixel == Color.FromArgb(255, 0, 0, 255))
                {
                    gManipulator.DrawLine(greenPen, current, tempPoint);
                    currentPixel = tempPixel;
                }
            }
            //left
            if (current.X - blockSize > 0 && checkIfClear(current.X, current.Y, (int)Directions.LEFT))
            {
                tempPoint = new Point(current.X - blockSize, current.Y);
                tempPixel = findPath(tempPoint);
                if (tempPixel == Color.FromArgb(255, 0, 0, 255))
                {
                    gManipulator.DrawLine(greenPen, current, tempPoint);
                    currentPixel = tempPixel;
                }
            }
            //right
            if (current.X + blockSize < image.Width && checkIfClear(current.X, current.Y, (int)Directions.RIGHT))
            {
                tempPoint = new Point(current.X + blockSize, current.Y);
                tempPixel = findPath(tempPoint);
                if (tempPixel == Color.FromArgb(255, 0, 0, 255))
                {
                    gManipulator.DrawLine(greenPen, current, tempPoint);
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

        static void fillLine(int fromx, int fromy, int toX, int toY, Color color)
        {
            for (int i = fromx - (blockSize / 2 + 1); i < fromx + (blockSize / 2 + 1); i++)
            {
                for (int j = fromy - (blockSize / 2 + 1); j < fromy + (blockSize / 2 + 1); j++)
                {
                    if (i > 0 && j > 0 && i < image.Width && j < image.Height
                        && image.GetPixel(i, j) == Color.FromArgb(255, 255, 255, 255)) image.SetPixel(i, j, color);
                }
            }
        }

        //Incomplete, have to find a way to add this distance formula to the amount traversed already.
        static double aStarEval(int currX, int currY)
        {
            return Math.Sqrt(Math.Pow(goal.X - currX, 2) + Math.Pow(goal.Y - currY, 2) );
        }

    }
}

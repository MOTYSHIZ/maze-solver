﻿using System;
using System.Drawing;

namespace Maze_Solver
{
    public class MazeSolveMain
    {
        private static string path = Environment.CurrentDirectory + @"\input-mazes\maze1.png";
        private static Bitmap image = new Bitmap(path, true);
        private static Graphics gManipulator = Graphics.FromImage(image);
        private static Pen greenPen = new Pen(Color.Green, blockSize);
        private static Coordinates goal;
        private static int blockSize = 0;
        enum Directions : int {UP,DOWN,LEFT,RIGHT};

        struct Coordinates
        {
            private int x;
            private int y;
            public void setX(int xnum) { x = xnum; }
            public void setY(int ynum) { y = ynum; }
            public int getX() { return x; }
            public int getY() { return y; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine(image.PhysicalDimension);

            Coordinates coord = findStart();
            findGoal();
            findPath(coord.getX(), coord.getY());
            image.Save(Environment.CurrentDirectory + @"\solved-mazes\maze1solved.png");
        }
         
        static Coordinates findStart()
        {
            Coordinates coord = new Coordinates();
            coord.setX(0);
            coord.setY(0);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if(image.GetPixel(x, y) == Color.FromArgb(255, 255, 0,0))
                    {
                        findBlockSize(x, y);
                        x += (blockSize / 2) - 1;
                        y += (blockSize / 2) - 1;
                        coord.setX(x);
                        coord.setY(y);
                        return coord;
                    }
                }
            }
            return coord;
        }

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
                        goal.setX(x);
                        goal.setY(y);
                        return;
                    }
                }
            }
        }

        //The main path finding function.
        static Color findPath(int currX, int currY)
        {
            Color currentPixel = image.GetPixel(currX, currY);
            Color tempPixel;

            //If black/wall
            if (currentPixel == Color.FromArgb(255, 0, 0, 0)
                || currentPixel == Color.FromArgb(255, 0, 0, 255)
                || currentPixel == Color.FromArgb(255, 255, 0, 255)
                || currentPixel == Color.FromArgb(255, 0, 255, 0)) return currentPixel;

            //fillBlock(currX, currY, Color.FromArgb(255, 255, 0, 255));
            image.SetPixel(currX, currY, Color.FromArgb(255, 255, 0, 255));

            //aStarEval(currX, currY);

            //up
            if (currY - blockSize > 0 && checkIfClear(currX, currY, (int)Directions.UP))
            {
                tempPixel = findPath(currX, currY - blockSize);
                if (tempPixel == Color.FromArgb(255, 0, 0, 255))
                {
                    //image.SetPixel(currX, currY, Color.FromArgb(255, 0, 255, 0));
                    gManipulator.DrawLine(greenPen, currX, currY, currX, currY - blockSize);
                    currentPixel = tempPixel;
                }
            }
            //down
            if (currY + blockSize < image.Height - 1 && checkIfClear(currX, currY, (int)Directions.DOWN))
            {
                tempPixel = findPath(currX, currY + blockSize);
                if (tempPixel == Color.FromArgb(255, 0, 0, 255))
                {
                    //image.SetPixel(currX, currY, Color.FromArgb(255, 0, 255, 0));
                    gManipulator.DrawLine(greenPen, currX, currY, currX, currY + blockSize);
                    currentPixel = tempPixel;
                }
            }
            //left
            if (currX - blockSize > 0 && checkIfClear(currX, currY, (int)Directions.LEFT))
            {
                tempPixel = findPath(currX - blockSize, currY);
                if (tempPixel == Color.FromArgb(255, 0, 0, 255))
                {
                    //image.SetPixel(currX, currY, Color.FromArgb(255, 0, 255, 0));
                    gManipulator.DrawLine(greenPen, currX, currY, currX - blockSize, currY);
                    currentPixel = tempPixel;
                }
            }
            //right
            if (currX + blockSize < image.Width && checkIfClear(currX, currY, (int)Directions.RIGHT))
            {
                tempPixel = findPath(currX + blockSize, currY);
                if (tempPixel == Color.FromArgb(255, 0, 0, 255))
                {
                    //image.SetPixel(currX, currY, Color.FromArgb(255, 0, 255, 0));
                    gManipulator.DrawLine(greenPen, currX, currY, currX + blockSize, currY);
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

        static void fillBlock(int x, int y, Color color)
        {
            for (int i = x - (blockSize/2 + 1); i < x + (blockSize/2 + 1); i++)
            {
                for (int j = y - (blockSize/2 + 1); j < y + (blockSize/2 + 1); j++)
                {
                    if(i > 0 && j > 0 && i < image.Width && j < image.Height
                        && image.GetPixel(i, j) == Color.FromArgb(255,255,255,255))image.SetPixel(i, j, color);
                }
            }
        }

        //Incomplete, have to find a way to add this distance formula to the amount traversed already.
        static double aStarEval(int currX, int currY)
        {
            return Math.Sqrt(Math.Pow(goal.getX() - currX, 2) + Math.Pow(goal.getY() - currY, 2) );
        }

    }
}

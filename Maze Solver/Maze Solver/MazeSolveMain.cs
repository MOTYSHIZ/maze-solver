/** Author: Justin Ordonez 
 * 
 * This is a maze-solving program that solves images taken in as jpgs, bmps, or pngs.
 * 
 * **/

using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Maze_Solver
{
    public class MazeSolveMain
    { 
        private static Bitmap image;
        private static Graphics gManipulator;
        private static Pen greenPen = new Pen(Color.Green, 2);
        private static int blockSize = 0;
        private static int threshold = 100;
        private static Stopwatch timer;

        static void Main(string[] args)
        {
            //Check if proper amount of arguments passed and attempt to load image file.
            if (args.Length == 2) loadImage(args[0]);   
            else
            {
                Console.WriteLine("Invalid number of arguments. \nPlease follow the format: maze.exe \"source.[bmp,png,jpg]\" \"destination.[bmp,png,jpg]\"");
                Environment.Exit(1);
            }

            Console.WriteLine("Image size: " + image.PhysicalDimension);  
            Node start = findColorCenter(Color.FromArgb(255, 255, 0, 0));
            Node goal = findColorCenter(Color.FromArgb(255, 0, 0, 175));

            if (start == null)
            {
                Console.WriteLine("Unable to find Start Location.");
                Environment.Exit(1);
            }
            else if (goal == null)
            {
                Console.WriteLine("Unable to find Goal Location.");
                Environment.Exit(1);
            }
            else if (algorithmChoose(start, goal) == true) Console.WriteLine("Maze Solved!");
            else Console.WriteLine("Maze not Solved.");

            timer.Stop();
            TimeSpan timespan = timer.Elapsed;
            Console.WriteLine(String.Format(String.Format("{0:00}:{1:00}:{2:00} (in Minutes, Seconds, ms)", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10)));

            clearTracers();     
            saveImage(args[1]);

            Console.WriteLine("Press Enter key to exit...");
            Console.ReadLine();
        }

        static void loadImage(String imageName)
        {
            String path = Environment.CurrentDirectory + @"\input-mazes\" + imageName;

            if (imageName.Contains(".bmp"))
            {
                path = convertImage(imageName);
            } 

            try
            {
                image = new Bitmap(path, true);
                gManipulator = Graphics.FromImage(image);
            }
            catch (Exception ex)
            {
                Console.Write("Source image is corrupt, unreadable, or nonexistent.");
                Environment.Exit(1);
            }
        }

        /* This method handles converting images from bitmaps. The original image is copied to another 
         * bitmap because Graphics objects can not be created from images with indexed pixel formats. 
         */
        static String convertImage(String imageName)
        {
            String path = Environment.CurrentDirectory + @"\input-mazes\" + imageName;

            try
            {
                //Clears the temp-conversions folder.
                string[] filePaths = Directory.GetFiles(Environment.CurrentDirectory + @"\temp-conversions\");
                foreach (string filePath in filePaths)
                    File.Delete(filePath);

                image = new Bitmap(path, true);
                Bitmap tempBitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(tempBitmap);

                g.DrawImage(image, new Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height));
                path = Environment.CurrentDirectory + @"\temp-conversions\" + imageName + ".png";
                tempBitmap.Save(path, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Console.Write("Source image is corrupt, unreadable, or nonexistent.");
                Environment.Exit(1);
            }
            return path;
        }

        static bool colorEquals(Color c1, Color c2)
        {
            if (Math.Abs(c1.R - c2.R) <= threshold
                && Math.Abs(c1.G - c2.G) <= threshold
                && Math.Abs(c1.B - c2.B) <= threshold) return true;

            return false;
        }

        static void saveImage(String imageName)
        {
            if (imageName.Contains(".png")
                || imageName.Contains(".jpg")
                || imageName.Contains(".bmp")) image.Save(Environment.CurrentDirectory + @"\solved-mazes\" + imageName);
            else image.Save(Environment.CurrentDirectory + @"\solved-mazes\" + imageName + ".png");
        }

        static bool algorithmChoose(Node start, Node target)
        {
            Console.WriteLine("Would you like to use: \n1) Wavefront Propagation (Faster for large mazes.) \n2) A-Star Algorithm to solve the maze?");
            int input;
            bool solved;
             
            if (int.TryParse(Console.ReadLine(), out input))
            {
                if (input == 1)
                {
                    timer = Stopwatch.StartNew();
                    solved = findPathWaveFront(start, target);
                }
                else if (input == 2)
                {
                    timer = Stopwatch.StartNew();
                    solved = findPathAStar(start, target);
                }
                else
                {
                    Console.WriteLine("Please provide valid input.");
                    solved = algorithmChoose(start, target);
                }
            }
            else
            {
                Console.WriteLine("Please provide valid input.");
                solved = algorithmChoose(start, target);
            }

            return solved;
        }

        static Node findColorCenter(Color color)
        {
            Node coord = new Node(0, 0);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if(colorEquals(image.GetPixel(x, y) , color)) 
                    {
                        if(blockSize == 0)findBlockSize(x, y);
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
            while(colorEquals(image.GetPixel(currX, currY) , Color.FromArgb(255, 155, 0, 0)))
            {
                blockSize++;
                currX++;
            }
            Console.WriteLine("block size = " + blockSize);
        }

        //Pathfinding function that uses WaveFront algorithm.
        static bool findPathWaveFront(Node start, Node target) {
            List<Node> frontier = new List<Node>();
            List<Node> explored = new List<Node>();

            Console.WriteLine("Solving Maze...");
            explored.Add(start);

            do
            {
                if(explored.Count > 1)explored.Clear();
                explored.AddRange(frontier);
                frontier.Clear();

                foreach (Node node in explored)
                {
                    List<Node> neighbors = getNeighbors(node);
                    foreach (Node node2 in neighbors)
                    {
                        node2.parent = node;
                        if (findDistance(node2, target) < 50) blockSize = 1;
                        if (colorEquals(image.GetPixel(node2.point.X, node2.point.Y), Color.FromArgb(255, 0, 0,155)))
                        {
                            retrace(start, node2);
                            return true;
                        }
                        image.SetPixel(node2.point.X, node2.point.Y, Color.FromArgb(255, 255, 0, 255));
                    }
                    frontier.AddRange(neighbors);
                }
            } while (frontier.Count > 0);

            return false;
        }

        //Pathfinding function that uses A* algorithm.
        static bool findPathAStar(Node start, Node target)
        {
            Console.WriteLine("Solving Maze...");
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
                if (colorEquals(image.GetPixel(current.point.X, current.point.Y) , Color.FromArgb(255,0,0,155)))
                {
                    retrace(start, current);
                    return true;
                }

                image.SetPixel(current.point.X, current.point.Y, Color.FromArgb(255, 255, 0, 255));
                
                foreach (Node neighbor in getNeighbors(current))
                {
                    bool frontierContains = frontier.Contains(neighbor);
                    if (frontierContains) continue;

                    int newMovementCostToNeighbor = current.gValue + findDistance(current, neighbor);
                    if(newMovementCostToNeighbor < neighbor.gValue || !frontierContains)
                    {
                        neighbor.gValue = newMovementCostToNeighbor;
                        neighbor.hValue = findDistance(neighbor, target);
                        if (neighbor.hValue < 50) blockSize = 1;
                        neighbor.parent = current;

                        if (!frontierContains) frontier.Add(neighbor);
                    }
                }
            }
            return false;
        }

        /*Used to find neighbors of the current node, so that they can be added to the
          frontier heap for the Wavefront and A* pathfinding algorithms. */
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

                    Node temp = new Node(xCoord, yCoord);

                    if (xCoord >= 0 && xCoord < image.Width && yCoord >= 0 && yCoord < image.Height
                        && checkIfClear(current.point.X, current.point.Y, x / blockSize, y / blockSize)
                        && image.GetPixel(temp.point.X, temp.point.Y) != Color.FromArgb(255, 255, 0, 255))
                    {
                        neighbors.Add(temp);
                    }
                }
            }
            return neighbors;
        }

        //checks if a direction is clear for traversal.
        static bool checkIfClear(int currX, int currY, int expandX, int expandY)
        {
            int blockSizeTemp = blockSize;

            while (blockSizeTemp >= 0 && currX >= 0 && currX + expandX < image.Width && currY >= 0 && currY+expandY < image.Height)
            {
                if (colorEquals(image.GetPixel(currX, currY), Color.FromArgb(255,0,0,0))) return false;

                if(expandX != 0 && expandY != 0 
                    && currX + expandX >= 0 && currX + expandX < image.Width 
                    && currY + expandY >= 0 && currY + expandY < image.Width)
                {
                    if (colorEquals(image.GetPixel(currX + expandX, currY), Color.FromArgb(255, 0, 0, 0)) 
                        && colorEquals(image.GetPixel(currX, currY + expandY), Color.FromArgb(255,0,0,0))) return false;
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

        static void retrace(Node start, Node target)
        {
            List<Node> path = new List<Node>();
            Node current = target;

            while (current != start)
            {
                path.Add(current);
                current = current.parent;
            }
            path.Add(start);
            path.Reverse();

            Point[] nodes = new Point[path.Count];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = path[i].point;
            }
            gManipulator.DrawLines(greenPen, nodes);
        }

        //Clears the purple pixels from the map.
        static void clearTracers()
        {
            Console.WriteLine("Clear propagation tracers? (Input 'Y' to clear purple map tracers.)");
            String input;
            input = Console.ReadLine();
            if (input.ToUpper().Contains("Y"))
            {
                Console.WriteLine("Clearing Tracers...");
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
}

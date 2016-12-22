/** Author: Justin Ordonez 
 * 
 * This is a maze-solving program that solves images taken in as jpgs, bmps, or pngs.
 * It utilizes either Wavefront Propagation (Concurrent Djikstra) or A* Pathfinding Algorithm, 
 * depending on user input.
 * 
 * Maze input files are taken from the folder /input-mazes/, and solved mazes are written to /solved-mazes/.
 * The /temp-conversions/ folder holds bitmap conversions for if an indexed pixel-format .bmp file is set as
 * an input maze.
 * 
 * The image rules are as follows:
 * 1) Maze starts at red.
 * 2) Maze finishes at blue.
 * 3) All walls are black.
 * 4) Maze is completely surrounded by black walls.
 * 5) The maze solution will be drawn in green.
 * 
 **/

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

        /*The blocksize is essentially the amount of pixels that each algorithm can skip over during each
        * evaluation cycle. This optimization makes it so that not every pixel needs to be evaluated, and
        * thus greatly improves the speed of each algorithm. */
        private static int blockSize = 0;
        private const int COLOR_THRESHOLD = 100;
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

            //Look for start and goal positions after printing to console the image size.
            Console.WriteLine("Image name: " + args[0] + "\nImage size: " + image.PhysicalDimension);  
            Node start = findColorCenter(Color.FromArgb(255, 255, 0, 0));
            Node goal = findColorCenter(Color.FromArgb(255, 0, 0, 175));

            /*Check if start and goal positions were found. Then ask user what algorithm they want to use.
            * If the goal state could not be found, inform user that maze is not solved. */
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

            //Stop the timer and print execution time.
            timer.Stop();
            TimeSpan timespan = timer.Elapsed;
            Console.WriteLine(String.Format(String.Format("{0:00}:{1:00}:{2:00} (in Minutes, Seconds, ms)", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10)));

            //Ask user if purple propagation tracers should be cleared, behave accordingly, and then save image.
            clearTracers();     
            saveImage(args[1]);

            Console.WriteLine("Press Enter key to exit...");
            Console.ReadLine();
        }

        /* This method loads the image specified from the input string. It loads from the folder
         * "input-mazes", and does simple exception handling. */
        static void loadImage(String imageName)
        {
            String path = Environment.CurrentDirectory + @"\input-mazes\" + imageName;

            if (imageName.Contains(".bmp"))         //Handle .bmp files with indexed pixel formats.
            {
                path = convertImage(imageName);     //Sets path to that of the convertedImage
            } 

            try
            {
                image = new Bitmap(path, true);     //Load in maze image from given filepath.
                gManipulator = Graphics.FromImage(image);   //graphics manipulator to draw solution path lines.
            }
            catch (Exception ex)
            {
                Console.Write("Source image is corrupt, unreadable, or nonexistent.");
                Environment.Exit(1);
            }
        }

        /* This method handles converting images from bitmaps. The original image is copied to another 
         * bitmap because Graphics objects can not be created from images with indexed pixel formats. */
        static String convertImage(String imageName)
        {
            String path = Environment.CurrentDirectory + @"\input-mazes\" + imageName;

            try
            {
                //Clears the temp-conversions folder.
                string[] filePaths = Directory.GetFiles(Environment.CurrentDirectory + @"\temp-conversions\");
                foreach (string filePath in filePaths)
                    File.Delete(filePath);

                //Creates a tempBitmap, and copies the former bitmap to it.
                image = new Bitmap(path, true);
                Bitmap tempBitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                gManipulator = Graphics.FromImage(tempBitmap);
                gManipulator.DrawImage(image, new Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height));

                //Updates path and saves the image to it.
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

        /* This function evaluates whether or not two colors are equal by comparing their
         * RGB channels and seeing if they fit within a certain threshold. */
        static bool colorEquals(Color c1, Color c2)
        {
            if (Math.Abs(c1.R - c2.R) <= COLOR_THRESHOLD
                && Math.Abs(c1.G - c2.G) <= COLOR_THRESHOLD
                && Math.Abs(c1.B - c2.B) <= COLOR_THRESHOLD) return true;

            return false;
        }

        /* Saves the resulting maze image with the given filename. If no type is specificed in
         * the name, it is assigned the PNG format. */
        static void saveImage(String imageName)
        {
            String path = Environment.CurrentDirectory + @"\solved-mazes\" + imageName;

            if (imageName.Contains(".png") || imageName.Contains(".jpg") || imageName.Contains(".bmp"))
            {
                image.Save(path);
                Console.WriteLine("Saving solved maze to " + Environment.CurrentDirectory + @"\solved-mazes\" + imageName);
            }
            else
            {
                image.Save(Environment.CurrentDirectory + @"\solved-mazes\" + imageName + ".png");
                Console.WriteLine("Saving solved maze to " + path + ".png");
            }
        }

        /* Handles user input to determine which search algorithm to use. If input is neither 1 or 2, requests valid input. */
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
                else  //Handle ints that are not 1 or 2.
                {
                    Console.WriteLine("Please provide valid input.");
                    solved = algorithmChoose(start, target);
                }
            }
            else  //Inputs that are not ints.
            {
                Console.WriteLine("Please provide valid input.");
                solved = algorithmChoose(start, target);
            }

            return solved;
        }

        // Finds the center of a block of color. used to locate the start and goal points.
        static Node findColorCenter(Color color)
        {
            Node coord = new Node(0, 0);

            /*Loop through every pixel of the image, looking for given color.
            * When first pixel of the given block of color is found, call findBlockSize to find its width.
            * Returns the center point of the color block. */
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

        /*Finds the block size by taking a starting pixel and iterating while counting through
        * subsequent pixels until the current pixel is no longer the red starting point color. */
        static void findBlockSize(int currX, int currY)
        {
            while(colorEquals(image.GetPixel(currX, currY) , Color.FromArgb(255, 155, 0, 0)))
            {
                blockSize++;
                currX++;
            }
            Console.WriteLine("block size = " + blockSize);
        }

        /*Pathfinding function that uses WaveFront algorithm, which is essentially a concurrent 
        * form of Djikstra's pathfinding algorithm. */
        static bool findPathWaveFront(Node start, Node target) {
            /*These lists actually behave more like two frontiers, since they are essentially two
            * layers at the wavefront. */
            List<Node> frontier = new List<Node>();
            List<Node> explored = new List<Node>();

            //Notification to user and adding of initial point.
            Console.WriteLine("Solving Maze...");
            explored.Add(start);

            do
            {
                /*Clears the explored set, unless it is only the starting point. This is done because the frontier
                * will become the new explored set, and there is no need to keep track of the center nodes.
                * All visited points are colored on the map in purple, so the majority do not need to be tracked in a list. */
                if(explored.Count > 1)explored.Clear();
                explored.AddRange(frontier);        // Explored set because the former frontier
                frontier.Clear();                   // Frontier cleared to become new wavefront.

                foreach (Node node in explored)
                {
                    List<Node> neighbors = getNeighbors(node);
                    foreach (Node neighborNode in neighbors)
                    {
                        neighborNode.parent = node;  //Set parent of neighborNode so path can be retraced.
                        if (findDistance(neighborNode, target) < 50) blockSize = 1; //If near the goal state, move by spaces of 1 pixel.

                        // This is the base case that checks for the goal state. If the color is close to blue, at goal.
                        if (colorEquals(image.GetPixel(neighborNode.point.X, neighborNode.point.Y), Color.FromArgb(255, 0, 0,155)))
                        {
                            retrace(start, neighborNode);  //Draw the path.
                            return true;
                        }
                        //Place a purple propagation tracer to mark point as visited.
                        image.SetPixel(neighborNode.point.X, neighborNode.point.Y, Color.FromArgb(255, 255, 0, 255));
                    }
                    frontier.AddRange(neighbors);
                }
            } while (frontier.Count > 0); //If this line is reached, the maze was not solved.

            return false;
        }

        /*Pathfinding function that uses A* pathfinding algorithm. This was my first implementation, but I figured that
        wavefront would be faster. This version of A* is novel in the way that it requires no explored set.
        Since visited points are colored in purple, the lookup to see if a node is visited is in constant time,
        and is even faster than if explored set is a HashSet.*/
        static bool findPathAStar(Node start, Node target)
        {
            //Create frontier List, which is a list of nodes to be explored, then add starting point to it.
            Console.WriteLine("Solving Maze...");
            List<Node> frontier = new List<Node>();
            frontier.Add(start);

            while (frontier.Count > 0)
            {
                /*Grab the smallest fvalue Node in the frontier. If two values are equal, their hvalues are compared.
                * For most cases, the frontier doesn't get unnecessarily large, so a heap implementation's 
                * performance gains were actually negligible when tested. */
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
                
                //If at goal, retrace path and draw in green
                if (colorEquals(image.GetPixel(current.point.X, current.point.Y) , Color.FromArgb(255,0,0,155)))
                {
                    retrace(start, current);
                    return true;
                }

                //Sets purple propagation tracer at current point. This is the subtitute for the explored set.
                image.SetPixel(current.point.X, current.point.Y, Color.FromArgb(255, 255, 0, 255));


                foreach (Node neighbor in getNeighbors(current))
                {
                    /*Performance drops from frontier.Contains are negligible because the frontier size
                    * is kept relatively small by checks made in the getNeighbors function.
                    * If the current neighbor node exists in the frontier, it is skipped over. */
                    bool frontierContains = frontier.Contains(neighbor);
                    if (frontierContains) continue;

                    /*Check if the neighbor node is existent in the frontier or if the evaluated cost of a neighbor node is 
                    * less than a cost evaluated for the node previously. If so, either evaluate and add to the frontier
                    * or only update the nodes values if it is already in the frontier. */ 
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

        /*Used to find neighbors of the current node, so that they can be added to the frontier heap for 
         * the Wavefront and A* pathfinding algorithms. Neighbors are checked to the top, bottom, left, 
         * and right of the current node. Each neighbor is the distance of "blocksize" away from the current.*/
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

                    //Range check to not include neighbors that are out of bounds, have maze walls blocking
                    //the way, or have been visited already.
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

        //Checks if a direction is clear for traversal, up to a distance equal to the blocksize.
        static bool checkIfClear(int currX, int currY, int expandX, int expandY)
        {
            int blockSizeTemp = blockSize;

            //While in range, or have checked as many pixels out as blockSizeTemp
            while (blockSizeTemp >= 0 && currX >= 0 && currX + expandX < image.Width && currY >= 0 && currY+expandY < image.Height)
            {
                //If encounted a wall or black pixel, return false to indicate that the direction is not clear.
                if (colorEquals(image.GetPixel(currX, currY), Color.FromArgb(255,0,0,0))) return false;
                                
                /*Handles diagonal cases where the diagonal pixel is clear, but not its adjacents.
                * Prior to this, non-orthogonal mazes would have issues with the path running through walls. */
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

        /* Evaluates distance between the points of two nodes. Up, Down, Left, and Right have a
         * cost of 10, while diagonal moves have a cost of 14. */
        static int findDistance(Node current, Node target)
        {
            int xDistance = Math.Abs(current.point.X - target.point.X);
            int yDistance = Math.Abs(current.point.Y - target.point.Y);

            if (xDistance > yDistance) return 14 * yDistance + 10 * (xDistance - yDistance);
            return 14 * xDistance + 10 * (yDistance - xDistance);
        }

        //Retraces the path from the target to the start node, then draws the path in green.
        static void retrace(Node start, Node target)
        {
            List<Node> path = new List<Node>();     //The node list that will hold the nodes along the path.
            Node current = target;              //Set current node to target node.

            //Retraces by following each node along the path via node parents. 
            while (current != start)
            {
                path.Add(current);
                current = current.parent;
            }
            path.Add(start);
            path.Reverse();

            //Convert Node List to Pointf array so that Graphics.DrawLines will work with it.
            Point[] nodes = new Point[path.Count];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = path[i].point;
            }
            gManipulator.DrawLines(greenPen, nodes);
        }

        /*Clears the purple propagation tracers from the map. The tracers are great for debugging, and 
        * they improve performance greatly. */ 
        static void clearTracers()
        {
            //Check if user wants the tracers removed.
            Console.WriteLine("Clear propagation tracers? (Input 'Y' to clear purple map tracers.)");
            String input;
            input = Console.ReadLine();

            //If user wants tracers remove, loop through image and replace all tracer instances with white.
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

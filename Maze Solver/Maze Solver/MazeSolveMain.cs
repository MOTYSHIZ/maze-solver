using System;
using System.Drawing;
using System.Collections.Generic;

namespace Maze_Solver
{
    public class MazeSolveMain
    {
        private static string path = Environment.CurrentDirectory + @"\input-mazes\maze7.png";
        private static Bitmap image = new Bitmap(path, true);
        private static Graphics gManipulator = Graphics.FromImage(image);
        private static Pen greenPen = new Pen(Color.Green, 2);
        private static int blockSize = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Image size: " + image.PhysicalDimension);
            
            Node start = findColorCenter(Color.FromArgb(255, 255, 0, 0));
            Node goal = findColorCenter(Color.FromArgb(255, 0, 0, 255));

            if (start == null) Console.WriteLine("Unable to find Start Location.");
            else if (goal == null) Console.WriteLine("Unable to find Goal Location.");
            else if (findPathWaveFront(start, goal) == true) Console.WriteLine("Maze Solved!");
            else Console.WriteLine("Maze not Solved.");

            clearTracers();
            image.Save(Environment.CurrentDirectory + @"\solved-mazes\maze1solved.png");
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        static Node findColorCenter(Color color)
        {
            Node coord = new Node(0, 0);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if(image.GetPixel(x, y) == color)
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
            while(image.GetPixel(currX, currY) == Color.FromArgb(255, 255, 0, 0))
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
                        if (image.GetPixel(node2.point.X, node2.point.Y) == Color.FromArgb(255, 0, 0, 255))
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
                Console.WriteLine(current.fValue());
                
                //If at goal
                if (image.GetPixel(current.point.X, current.point.Y) == Color.FromArgb(255,0,0,255))
                {
                    retrace(start, current);
                    return true;
                }

                image.SetPixel(current.point.X, current.point.Y, Color.FromArgb(255, 255, 0, 255));

                foreach (Node neighbor in getNeighbors(current))
                {
                    if (image.GetPixel(neighbor.point.X, neighbor.point.Y) == Color.FromArgb(255, 255, 0, 255)) continue;
                    bool frontierContains = image.GetPixel(neighbor.point.X, neighbor.point.Y) == Color.FromArgb(255, 255, 0, 255);

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{

    // For more detail on the A* algorithm follow this link http://blog.two-cats.com/2014/06/a-star-example/ 

    //TODO adjust these automatically for TerrainMap
    public int MaxWidth = 7;
    public int MaxHeight = 7;

    private Node[,] nodes;
    private Node startNode;
    private Node endNode;
    private List<Node> OpenNodes;
    private List<Node> ClosedNodes;


    public bool PathAvailable(Node Begin, Node End, Node[,] Map)
    {
        startNode = Begin;
        endNode = End;
        nodes = Map;
        foreach (Node node in nodes)
        {
            node.CalculateG(startNode.Location);
            node.CalculateH(endNode.Location);
            node.State = NodeState.Untested;
            node.ParentNode = null;

            if (node.GetComponent<Hex>().EntityHolding == null || node == endNode)
            {
                node.IsWalkable = true;
            }
            else
            {
                node.IsWalkable = false;
            }

        }
        OpenNodes = new List<Node>();
        ClosedNodes = new List<Node>();

        // Add Starting point into list to become closed
        OpenNodes.Add(startNode);
        return (Search(startNode));
    }

    public List<Node> GetRawPath(Node Begin, Node End, Node[,] Map)
    {
        // Initialize values and nodes
        startNode = Begin;
        endNode = End;
        nodes = Map;
        foreach (Node node in nodes)
        {
            node.CalculateG(startNode.Location);
            node.CalculateH(endNode.Location);
            node.State = NodeState.Untested;
            node.ParentNode = null;
            node.IsWalkable = true;

        }
        OpenNodes = new List<Node>();
        ClosedNodes = new List<Node>();
        List<Node> path = new List<Node>();

        // Add Starting point into list to become closed
        OpenNodes.Add(startNode);
        // Search for a path between start point and end point and return true if possible 
        bool success = Search(startNode);
        if (success)
        {
            // Reverse the path list for path from start to end
            Node node = this.endNode;
            while (node.ParentNode != null)
            {
                path.Add(node);
                node = node.ParentNode;
            }
            path.Reverse();
        }
        return path;
    }


    public List<Node> ShowPathDistance(Node Begin, Node End, Node[,] Map)
    {
        // Initialize values and nodes
        startNode = Begin;
        endNode = End;
        nodes = Map;
        foreach (Node node in nodes)
        {
            node.CalculateG(startNode.Location);
            node.CalculateH(endNode.Location);
            node.State = NodeState.Untested;
            node.ParentNode = null;

            if (node.GetComponent<Hex>().EntityHolding == null || node == endNode)
            {
                node.IsWalkable = true;
            }
            else
            {
                node.IsWalkable = false;
            }

        }
        OpenNodes = new List<Node>();
        ClosedNodes = new List<Node>();
        List<Node> path = new List<Node>();

        // Add Starting point into list to become closed
        OpenNodes.Add(startNode);
        // Search for a path between start point and end point and return true if possible 
        bool success = Search(startNode);
        if (success)
        {
            // Reverse the path list for path from start to end
            Node node = this.endNode;
            while (node.ParentNode != null)
            {
                path.Add(node);
                node = node.ParentNode;
            }
            path.Reverse();
        }
        return path;
    }



    // Finds a path between Tile A and B using the A* algorithm
    public List<Node> FindPath(Node Begin, Node End, Node[,] Map, Character.CharacterType CT)
    {
        // Initialize values and nodes
        startNode = Begin;
        endNode = End;
        nodes = Map;
        foreach (Node node in nodes)
        {
            node.CalculateG(startNode.Location);
            node.CalculateH(endNode.Location);
            node.State = NodeState.Untested;
            node.ParentNode = null;

            if (node.GetComponent<Hex>().EntityHolding == null || node.GetComponent<Hex>().HasSameType(CT))
            {
                node.IsWalkable = true;
            }
            else
            {
                node.IsWalkable = false;
            }

        }
        OpenNodes = new List<Node>();
        ClosedNodes = new List<Node>();
        List<Node> path = new List<Node>();

        // Add Starting point into list to become closed
        OpenNodes.Add(startNode);
        // Search for a path between start point and end point and return true if possible 
        bool success = Search(startNode);
        if (success)
        {
            // Reverse the path list for path from start to end
            Node node = this.endNode;
            while (node.ParentNode != null)
            {
                path.Add(node);
                node = node.ParentNode;
            }
            path.Reverse();
        }
        return path;
    }



    private bool Search(Node currentNode)
    {
        //Make the current node closed since its being evaluated
        currentNode.State = NodeState.Closed;
        OpenNodes.Remove(currentNode);
        ClosedNodes.Add(currentNode);

        // Add the viable nodes that are adjacent to the current one to the Open list
        OpenNodes.AddRange(GetAdjacentWalkableNodes(currentNode));

        // Check if any open nodes available, if not then no path possible 
        if (OpenNodes.Count > 0)
        {
            // Find the next node with the lowest F value
            Node minFNode = OpenNodes[0];
            foreach (var nextNode in OpenNodes)
            {
                if (nextNode.F < minFNode.F)
                {
                    minFNode = nextNode;
                }
            }

            // If its the end then your done 
            if (minFNode.Location == this.endNode.Location)
            {
                return true;
            }

            // If not repeat the process
            else
            {
                if (Search(minFNode)) // Note: Recurses back into Search(Node)
                    return true;
            }
        }
        return false;
    }


    private Point[] GetAdjacentLocations(Point location)
    {
        // Find the four adjacent nodes UP Down Left Right
        // Six for hex

        //Point[] AdjacentPoints = new Point[4];
        //AdjacentPoints[0] = new Point(location.X, location.Y + 1);
        //AdjacentPoints[1] = new Point(location.X, location.Y - 1);
        //AdjacentPoints[2] = new Point(location.X + 1, location.Y);
        //AdjacentPoints[3] = new Point(location.X - 1, location.Y);


        Point[] AdjacentPoints = new Point[6];
        // x - 1 and x
        if (location.Y % 2 == 0)
        {
            AdjacentPoints[0] = new Point(location.X, location.Y + 1);
            AdjacentPoints[1] = new Point(location.X - 1, location.Y + 1); //additional one with a hex
            AdjacentPoints[2] = new Point(location.X, location.Y - 1);
            AdjacentPoints[3] = new Point(location.X - 1, location.Y - 1); //additional one with a hex
            AdjacentPoints[4] = new Point(location.X + 1, location.Y);
            AdjacentPoints[5] = new Point(location.X - 1, location.Y);
        }
        // x+ 1 and x
        else
        {
            AdjacentPoints[0] = new Point(location.X, location.Y + 1);
            AdjacentPoints[1] = new Point(location.X + 1, location.Y + 1); //additional one with a hex
            AdjacentPoints[2] = new Point(location.X, location.Y - 1);
            AdjacentPoints[3] = new Point(location.X + 1, location.Y - 1); //additional one with a hex
            AdjacentPoints[4] = new Point(location.X + 1, location.Y);
            AdjacentPoints[5] = new Point(location.X - 1, location.Y);
        }

        return (AdjacentPoints);
    }

    private List<Node> GetAdjacentWalkableNodes(Node fromNode)
    {
        List<Node> walkableNodes = new List<Node>();
        IEnumerable<Point> nextLocations = GetAdjacentLocations(fromNode.Location);

        foreach (var location in nextLocations)
        {
            int x = location.X;
            int y = location.Y;

            // Stay within the grid's boundaries
            if (x < 0 || x >= this.MaxWidth || y < 0 || y >= this.MaxHeight)
                continue;

            Node node = this.nodes[x, y];
            // Ignore non-walkable nodes
            if (!node.IsWalkable)
                continue;

            // Ignore already-closed nodes
            if (node.State == NodeState.Closed)
                continue;

            if (!fromNode.isConnectedToRoom(node, false))
                continue;

            // Already-open nodes are only added to the list if their G-value is lower going via this route.
            if (node.State == NodeState.Open)
            {
                float traversalCost = Node.GetTraversalCost(node.Location, node.ParentNode.Location);
                float gTemp = fromNode.G + traversalCost;
                if (gTemp < node.G)
                {
                    node.ParentNode = fromNode;
                    walkableNodes.Add(node);
                }
            }
            else
            {
                // If it's untested, set the parent and flag it as 'Open' for consideration
                node.ParentNode = fromNode;
                node.State = NodeState.Open;
                walkableNodes.Add(node);
            }
        }

        return walkableNodes;
    }


}

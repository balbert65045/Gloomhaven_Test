using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{

    // For more detail on the A* algorithm follow this link http://blog.two-cats.com/2014/06/a-star-example/ 

    HexMapController controller;

    private Node[] nodes;
    private Node startNode;
    private Node endNode;
    private List<Node> OpenNodes;
    private List<Node> ClosedNodes;

    private void Start()
    {
        controller = GetComponent<HexMapController>();
        nodes = GetComponentsInChildren<Node>();
    }

    public bool PathAvailable(Node Begin, Node End)
    {
        startNode = Begin;
        endNode = End;
        foreach (Node node in nodes)
        {
            node.CalculateG(startNode);
            node.CalculateH(endNode);
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

    public List<Node> GetRawPath(Node Begin, Node End)
    {
        // Initialize values and nodes
        startNode = Begin;
        endNode = End;
        foreach (Node node in nodes)
        {
            node.CalculateG(startNode);
            node.CalculateH(endNode);
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


    public List<Node> ShowPathDistance(Node Begin, Node End)
    {
        // Initialize values and nodes
        startNode = Begin;
        endNode = End;
        foreach (Node node in nodes)
        {
            node.CalculateG(startNode);
            node.CalculateH(endNode);
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
    public List<Node> FindPath(Node Begin, Node End, Character.CharacterType CT)
    {
        List<Node> path = new List<Node>();
        // Initialize values and nodes
        if (Begin == End) { return null; }

        startNode = Begin;
        endNode = End;
        foreach (Node node in nodes)
        {
            node.CalculateG(startNode);
            node.CalculateH(endNode);
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
            if (minFNode == this.endNode)
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

    private List<Node> GetAdjacentWalkableNodes(Node fromNode)
    {
        List<Node> walkableNodes = new List<Node>();
        List<Node> nextLocations = controller.GetRealNeighbors(fromNode);

        foreach (Node location in nextLocations)
        {
            Node node = location;
            if (node == null)
                continue;
            // Ignore non-walkable nodes
            if (!node.IsWalkable)
                continue;

            if (!node.isAvailable)
                continue;

            if (node.edge)
                continue;
            // Ignore already-closed nodes
            if (node.State == NodeState.Closed)
                continue;

            if (!fromNode.isConnectedToRoom(node))
                continue;

            // Already-open nodes are only added to the list if their G-value is lower going via this route.
            if (node.State == NodeState.Open)
            {
                float traversalCost = 1;
                float gTemp = fromNode.G + traversalCost;
                if (gTemp <= node.G)
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

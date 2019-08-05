using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Direction
{
    North = 1,
    NorthEast = 2,
    SouthEast = 3,
    South = 4,
    SouthWest = 5,
    NorthWest = 6,
}

public class HexMapController : MonoBehaviour {

    // Use this for initialization
    int gridHeight;
    int gridWidth;
    HexMapBuilder hexBuilder;
    public LayerMask HexLayer;

    public Node[,] Map;
        //= new Node[11, 11];


    void Awake () {
        hexBuilder = FindObjectOfType<HexMapBuilder>();
        gridHeight = hexBuilder.gridHeight;
        gridWidth = hexBuilder.gridWidth;
        Map = new Node[gridWidth, gridHeight];
    }



    public void SetNodeMap(Node node, int X, int Y)
    {
        Map[X, Y] = node;
    }

    public List<Node> GetDistanceRange(Node StartNode, int MoveDistance, Character.CharacterType CT)
    {
        List<Node> NodesInDistance = new List<Node>(); ;
        List<Node> NodesToCheck = new List<Node>();
        List<Node> NextNodesToCheck = new List<Node>();
        NodesToCheck.Add(StartNode);
        NodesInDistance.Add(StartNode);
        while (MoveDistance > 0)
        {
            foreach (Node node in NodesToCheck)
            {
                List<Node> AdjacentNodes = GetNodesAdjacent(node);
                foreach (Node N in AdjacentNodes)
                {
                    if (!NodesInDistance.Contains(N))
                    {
                        NodesInDistance.Add(N);
                        NextNodesToCheck.Add(N);
                    }
                }
            }
            NodesToCheck.Clear();
            foreach (Node node in NextNodesToCheck) {
                if (node.NodeHex.EntityHolding == null || (node.NodeHex.EntityHolding.GetComponent<Character>() != null && node.NodeHex.EntityHolding.GetComponent<Character>().myCT == CT))
                {
                    NodesToCheck.Add(node);
                }
            }
            NextNodesToCheck.Clear();
            MoveDistance--;
        }
        return NodesInDistance;
    }


    public List<Node> GetNodesAtDistanceFromNode(Node StartNode, int distance)
    {
        List<Node> NodesChecked = new List<Node>();
        List<Node> NodesToCheckInDistance = new List<Node>();
        List<Node> NodesinInDistance = new List<Node>();
        NodesToCheckInDistance.Add(StartNode);
        for (int i = 0; i < distance; i++)
        {
            int CurrentAmountToCheck = NodesToCheckInDistance.Count;
            for (int j = 0; j < CurrentAmountToCheck; j++)
            {
                List<Node> AdjacentNodes = GetNodesAdjacent(NodesToCheckInDistance[0]);
                foreach (Node node in AdjacentNodes)
                {
                    if (NodesChecked.Contains(node) || NodesToCheckInDistance.Contains(node)) { continue; }
                    NodesinInDistance.Add(node);
                    NodesToCheckInDistance.Add(node);
                }
                NodesChecked.Add(NodesToCheckInDistance[0]);
                NodesToCheckInDistance.Remove(NodesToCheckInDistance[0]);
            }
        }
        return NodesinInDistance;
    }


    public List<Node> GetNodesAdjacent(Node node)
    {
        List<Node> AdjacentNodes = new List<Node>();

        if (node.Y + 1 <= gridHeight - 1) { AdjacentNodes.Add(Map[node.X, node.Y + 1]); }
        if (node.Y - 1 >= 0) { AdjacentNodes.Add(Map[node.X, node.Y - 1]); }
        if (node.X + 1 <= gridWidth - 1) { AdjacentNodes.Add(Map[node.X + 1, node.Y]); }
        if (node.X - 1 >= 0) { AdjacentNodes.Add(Map[node.X - 1, node.Y]); }
        // x - 1 and x
        if (node.Y % 2 == 0)
        {
            if (node.Y + 1 <= gridHeight - 1 && node.X - 1 >= 0) { AdjacentNodes.Add(Map[node.X - 1, node.Y + 1]); }
            if (node.Y - 1 >= 0 && node.X - 1 >= 0) { AdjacentNodes.Add(Map[node.X - 1, node.Y - 1]); }
        }
        else
        {
            if (node.Y + 1 <= gridHeight - 1 && node.X + 1 <= gridWidth -1) { AdjacentNodes.Add(Map[node.X + 1, node.Y + 1]); }
            if (node.Y - 1 >= 0 && node.X + 1 <= gridWidth - 1) { AdjacentNodes.Add(Map[node.X + 1, node.Y - 1]); }
        }

        List<Node> AdjacentNodesAvailable = new List<Node>();
        foreach(Node aNode in AdjacentNodes)
        {
            if (node.isConnectedToRoom(aNode)) { AdjacentNodesAvailable.Add(aNode); }
        }

        return AdjacentNodesAvailable;
    }

    public List<Node> GetAOE(AOEType aoeType, Node OriginNode, Node StartNode)
    {
        List<Node> NodesinAOE = new List<Node>();
        switch (aoeType)
        {
            case AOEType.Cleave:
                Direction direction = FindDirection(OriginNode, StartNode);
                Node nodeInCleave = GetNextCounterClockwizeNode(OriginNode, StartNode, direction);
                NodesinAOE.Add(StartNode);
                NodesinAOE.Add(nodeInCleave);
                break;
            case AOEType.Line:
                Direction lineDirection = FindDirection(OriginNode, StartNode);
                Node node = GetNextNodeInDirection(StartNode, lineDirection);
                NodesinAOE.Add(StartNode);
                NodesinAOE.Add(node);
                break;
            case AOEType.SingleTarget:
                NodesinAOE.Add(StartNode);
                break;
            case AOEType.Surounding:
                List<Node> nodes = GetNodesSurrounding(OriginNode);
                foreach(Node myNode in nodes) { NodesinAOE.Add(myNode); }
                break;
            case AOEType.Triangle:

                break;
        }


        return NodesinAOE;
    }

    public List<Node> LineOfSight(int Range, Hex hexFrom)
    {
        List<Node> NodesChecked = new List<Node>();
        List<Node> NodesToCheckLOS = new List<Node>();
        List<Node> NodesinLOS = new List<Node>();
        NodesToCheckLOS.Add(hexFrom.HexNode);
        for (int i = 0; i < Range; i++)
        {
            int CurrentAmountToCheck = NodesToCheckLOS.Count;
            for (int j = 0; j < CurrentAmountToCheck; j++)
            {
                List<Node> AdjacentNodes = GetNodesAdjacent(NodesToCheckLOS[0]);
                foreach (Node node in AdjacentNodes)
                {
                    if (NodesChecked.Contains(node)){ continue; }
                    if (!CheckifBlocked(hexFrom.HexNode, node))
                    {
                        NodesinLOS.Add(node);
                        NodesToCheckLOS.Add(node);
                    }
                }
                NodesChecked.Add(NodesToCheckLOS[0]);
                NodesToCheckLOS.Remove(NodesToCheckLOS[0]);
            }
        }
        return NodesinLOS;
    }

    bool CheckifBlocked(Node StartNode, Node EndNode)
    {
        Vector3 DirectionVector = (EndNode.transform.position - StartNode.transform.position).normalized;
        float Distance = (EndNode.transform.position - StartNode.transform.position).magnitude;
        Ray NodeRay = new Ray(StartNode.transform.position + Vector3.up * .1f, DirectionVector);
        RaycastHit[] hits = Physics.RaycastAll(NodeRay, Distance, HexLayer);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.GetComponent<Hex>().BlockingLineOfSight)
            {
                return true;
            }
        }
        return false;
    }

     List<Node> FindNodeNotBlockingLOSOrAlreadyChecked(Node StartNode, List<Node> NodesChecked)
    {
        List<Node> NodesInLOS = new List<Node>();
        for (int j = 1; j < 7; j++)
        {
            List<Node> NodesInDirection = FindNodesInDirection((Direction)j, StartNode);
            foreach (Node node in NodesInDirection)
            {
                if (!node.NodeHex.BlockingLineOfSight && !NodesChecked.Contains(node)) { NodesInLOS.Add(node); }
            }
        }
        return NodesInLOS;
    }

    public Direction FindDirection(Node StartNode, Node EndNode)
    {
        int Xdifference = EndNode.X - StartNode.X;
        int Ydifference = EndNode.Y - StartNode.Y;
        if (StartNode.Y % 2 != 0)
        {
            if(Xdifference == 0 && Ydifference == 1) { return Direction.NorthWest; }
            else if (Xdifference == 1 && Ydifference == 1) { return Direction.SouthWest; }
            else if (Xdifference == 0 && Ydifference == -1) { return Direction.NorthEast; }
            else if (Xdifference == 1 && Ydifference == -1) { return Direction.SouthEast; }
            else if (Xdifference == -1 && Ydifference == 0) { return Direction.North; }
            else if (Xdifference == 1 && Ydifference == 0) { return Direction.South; }
        }
        else
        {
            if (Xdifference == -1 && Ydifference == 1) { return Direction.NorthWest; }
            else if (Xdifference == 0 && Ydifference == 1) { return Direction.SouthWest; }
            else if (Xdifference == -1 && Ydifference == -1) { return Direction.NorthEast; }
            else if (Xdifference == 0 && Ydifference == -1) { return Direction.SouthEast; }
            else if (Xdifference == -1 && Ydifference == 0) { return Direction.North; }
            else if (Xdifference == 1 && Ydifference == 0) { return Direction.South; }
        }
        return Direction.North;
    }

    public List<Node> GetNodesSurrounding(Node StartNode)
    {
        List<Node> nodes = new List<Node>();
        nodes.Add(GetNodeInDirection(Direction.North, StartNode));
        nodes.Add(GetNodeInDirection(Direction.NorthEast, StartNode));
        nodes.Add(GetNodeInDirection(Direction.SouthEast, StartNode));
        nodes.Add(GetNodeInDirection(Direction.South, StartNode));
        nodes.Add(GetNodeInDirection(Direction.SouthWest, StartNode));
        nodes.Add(GetNodeInDirection(Direction.NorthWest, StartNode));
        return nodes;
    }

    public Node GetNextNodeInDirection(Node StartNode, Direction direction)
    {
        Node nextNode = GetNodeInDirection(direction, StartNode);
        return nextNode;
    }

    public Node GetNextCounterClockwizeNode(Node StartNode, Node EndNode, Direction DirectionOfEndNode)
    {
        Direction nextDirection = (int)DirectionOfEndNode == 1 ? Direction.NorthWest : (Direction)((int)(DirectionOfEndNode) - 1);
        Node nextNode = GetNodeInDirection(nextDirection, StartNode);
        return nextNode;
    }

    Node GetNodeInDirection(Direction direction, Node startNode)
    {
        Node nodeInDirection = null;

        if (startNode.Y % 2 != 0)
        {
            switch (direction)
            {
                case Direction.North:
                    if (startNode.X <= 0) { return null; }
                    nodeInDirection = Map[startNode.X - 1, startNode.Y];
                    break;
                case Direction.NorthEast:
                    if (startNode.Y <= 0) { return null; }
                    nodeInDirection = (Map[startNode.X, startNode.Y - 1]);
                    break;
                case Direction.SouthEast:
                    if (startNode.X >= gridWidth - 1) { return null; }
                    if (startNode.Y <= 0) { return null; }
                    nodeInDirection = (Map[startNode.X + 1, startNode.Y - 1]);
                    break;
                case Direction.South:
                    if (startNode.X >= gridWidth - 1) { return null; }
                    nodeInDirection = (Map[startNode.X + 1, startNode.Y]);
                    break;
                case Direction.SouthWest:
                    if (startNode.X >= gridWidth - 1) { return null; }
                    if (startNode.Y >= gridHeight - 1) { return null; }
                    nodeInDirection = (Map[startNode.X + 1, startNode.Y + 1]);
                    break;
                case Direction.NorthWest:
                    if (startNode.Y >= gridHeight - 1) { return null; }
                    nodeInDirection = (Map[startNode.X, startNode.Y + 1]);
                    break;
            }
        } 
        else
        {
            switch (direction)
            {
                case Direction.North:
                    if (startNode.X <= 0) { return null; }
                    nodeInDirection = (Map[startNode.X - 1, startNode.Y]);
                    break;
                case Direction.NorthEast:
                    if (startNode.X <= 0) { return null; }
                    if (startNode.Y <= 0) { return null; }
                    nodeInDirection = (Map[startNode.X - 1, startNode.Y - 1]);
                    break;
                case Direction.SouthEast:
                    if (startNode.Y <= 0) { return null; }
                    nodeInDirection = (Map[startNode.X, startNode.Y - 1]);
                    break;
                case Direction.South:
                    if (startNode.X >= gridWidth - 1) { return null; }
                    nodeInDirection = (Map[startNode.X + 1, startNode.Y]);
                    break;
                case Direction.SouthWest:
                    if (startNode.Y >= gridHeight - 1) { return null; }
                    nodeInDirection = (Map[startNode.X, startNode.Y + 1]);
                    break;
                case Direction.NorthWest:
                    if (startNode.Y >= gridHeight - 1) { return null; }
                    if (startNode.X <= 0) { return null; }
                    nodeInDirection = (Map[startNode.X - 1, startNode.Y + 1]);
                    break;
            }
        }
        return nodeInDirection;
    }



    //THIS NEEDS TO BE CHANGED!!
     List<Node> FindNodesInDirection(Direction direction, Node node)
    {
        // Directions should be as follows: (1,0), (-1,0), (0,1) (0,-1)
        //depending on what y value: (1,1), (1,-1) or (-1,1), (-1,-1)
        List<Node> NextNodes = new List<Node>();
        switch (direction)
        {
            case Direction.NorthEast:
                if (node.Y % 2 == 0)
                {
                    NextNodes.Add(Map[node.X, node.Y + 1]); //UpLeft
                    NextNodes.Add(Map[node.X - 1, node.Y + 1]); //UpRight
                    NextNodes.Add(Map[node.X - 1, node.Y]); //Right
                }
                else
                {
                    NextNodes.Add(Map[node.X + 1, node.Y + 1]); //UpLeft
                    NextNodes.Add(Map[node.X, node.Y + 1]); //UpRight
                    NextNodes.Add(Map[node.X - 1, node.Y]); //Right
                }
                break;
            case Direction.North:
                if (node.Y % 2 == 0)
                {
                    NextNodes.Add(Map[node.X - 1, node.Y + 1]); //UpRight
                    NextNodes.Add(Map[node.X - 1, node.Y]); //Right
                    NextNodes.Add(Map[node.X - 1, node.Y - 1]); //DownRight
                }
                else
                {
                    NextNodes.Add(Map[node.X, node.Y + 1]); //UpRight
                    NextNodes.Add(Map[node.X - 1, node.Y]); //Right
                    NextNodes.Add(Map[node.X, node.Y - 1]); //DownRight
                }
                break;
            case Direction.SouthEast:
                if (node.Y % 2 == 0)
                {
                    NextNodes.Add(Map[node.X - 1, node.Y]); //Right
                    NextNodes.Add(Map[node.X - 1, node.Y - 1]); //DownRight
                    NextNodes.Add(Map[node.X, node.Y - 1]); //DownLeft
                }
                else
                {
                    NextNodes.Add(Map[node.X - 1, node.Y]); //Right
                    NextNodes.Add(Map[node.X, node.Y - 1]); //DownRight
                    NextNodes.Add(Map[node.X + 1, node.Y - 1]); //DownLeft
                }
                break;
            case Direction.SouthWest:
                if (node.Y % 2 == 0)
                {
                    NextNodes.Add(Map[node.X + 1, node.Y]); //Left
                    NextNodes.Add(Map[node.X - 1, node.Y - 1]); //DownRight
                    NextNodes.Add(Map[node.X, node.Y - 1]); //DownLeft
                }
                else
                {
                    NextNodes.Add(Map[node.X + 1, node.Y]); //Left
                    NextNodes.Add(Map[node.X, node.Y - 1]); //DownRight
                    NextNodes.Add(Map[node.X + 1, node.Y - 1]); //DownLeft
                }
                break;
            case Direction.South:
                if (node.Y % 2 == 0)
                {
                    NextNodes.Add(Map[node.X + 1, node.Y]); //Left
                    NextNodes.Add(Map[node.X, node.Y + 1]); //UpLeft
                    NextNodes.Add(Map[node.X, node.Y - 1]); //DownLeft
                }
                else
                {
                    NextNodes.Add(Map[node.X + 1, node.Y]); //Left
                    NextNodes.Add(Map[node.X + 1, node.Y + 1]); //UpLeft
                    NextNodes.Add(Map[node.X + 1, node.Y - 1]); //DownLeft
                }
                break;
            case Direction.NorthWest:
                if (node.Y % 2 == 0)
                {
                    NextNodes.Add(Map[node.X + 1, node.Y]); //Left
                    NextNodes.Add(Map[node.X, node.Y + 1]); //UpLeft
                    NextNodes.Add(Map[node.X - 1, node.Y + 1]); //UpRight
                }
                else
                {
                    NextNodes.Add(Map[node.X + 1, node.Y]); //Left
                    NextNodes.Add(Map[node.X + 1, node.Y + 1]); //UpLeft
                    NextNodes.Add(Map[node.X, node.Y + 1]); //UpRight
                }
                break;
        }
        return NextNodes;


    }


}

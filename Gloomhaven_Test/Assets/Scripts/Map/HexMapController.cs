﻿using System.Collections;
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

public class HexPoint
{
    public Vector3 Location;
}

public class HexMapController : MonoBehaviour {

    // Use this for initialization
    HexMapBuilder hexBuilder;
    public LayerMask HexLayer;
    public LayerMask WallLayer;

    public Hashtable Map = new Hashtable();
    public Hex[] AllHexes;

    public void CreateTable()
    {
        Map.Clear();
        Node[] nodes = GetComponentsInChildren<Node>();
        foreach (Node node in nodes)
        {
            AddHex(node);
        }
    }

    void Awake () {
        hexBuilder = FindObjectOfType<HexMapBuilder>();
        AllHexes = GetComponentsInChildren<Hex>();
        CreateTable();
    }

    public HexPoint GetPoint(Node node, int directionIndex)
    {
        HexPoint HPoint = new HexPoint();
        Vector3 point = new Vector3();
        switch (directionIndex)
        {
            case 0:
                point = (node.transform.position + new Vector3(0.87606f, 0, -0.4955f));
                break;
            case 1:
                point = (node.transform.position + new Vector3(0, 0, -0.991f));
                break;
            case 2:
                point = (node.transform.position + new Vector3(-0.87606f, 0, -0.4955f));
                break;
            case 3:
                point = (node.transform.position + new Vector3(-0.87606f, 0, 0.4955f));
                break;
            case 4:
                point = (node.transform.position + new Vector3(0, 0, 0.991f));
                break;
            case 5:
                point = (node.transform.position + new Vector3(0.87606f, 0, 0.4955f));
                break;
        }
        HPoint.Location = point;
        return HPoint;
    }

    public List<Vector3> GetHexesSurrounding(Node StartingNode, List<Node> nodesInArea)
    {
        if (nodesInArea.Count == 1 && nodesInArea[0] == StartingNode)
        {
            List<Vector3> Points = new List<Vector3>();
            for (int i = 0; i < 6; i++)
            {
                Points.Add(GetPoint(StartingNode, i).Location);
            }
            return Points;
        }

        Node FirstOuterNode = null;
        int currentDirectionIndex = 0;
        nodesInArea.Add(StartingNode);
        for (int i = 0; i < 6; i++)
        {
            Node node = FindFirstOuterNode(StartingNode, nodesInArea, i);
            if (node != StartingNode)
            {
                FirstOuterNode = node;
                currentDirectionIndex = (1 + i) % 6;
                break;
            }
        }
        List<Node> OuterNodes = new List<Node>();
        List<Vector3> OuterPoints = new List<Vector3>();
        Node CurrentNode = FirstOuterNode;

        //first node
        for (int i = 0; i < 6; i++)
        {
            int index = 0;
            index = (currentDirectionIndex - 1 + i) % 6;
            int direction = index < 0 ? index + 6 : index;
            Node node = GetNodeInDirection(GetDirections()[direction], CurrentNode);
            if (nodesInArea.Contains(node))
            {
                currentDirectionIndex = direction;
                OuterNodes.Add(CurrentNode);
                CurrentNode = node;
                break;
            }
        }

        HexPoint currentPoint = null;
        HexPoint firstPoint = null;

        int timeoutTimer = 0;
        while (firstPoint == null || currentPoint == null || (firstPoint.Location - currentPoint.Location).magnitude > .1f)
        {
            if (timeoutTimer > 1000) {
                Debug.Log("TimedOut");
                break;
            }
            timeoutTimer++;
            for (int i = 0; i < 6; i++)
            {
                int index = 0;
                index = (currentDirectionIndex + 4 + i) % 6;
                int direction = index < 0 ? index + 6 : index;
                Node node = GetNodeInDirection(GetDirections()[direction], CurrentNode);
                if (nodesInArea.Contains(node))
                {
                    currentDirectionIndex = direction;
                    OuterNodes.Add(CurrentNode);
                    CurrentNode = node;
                    break;
                }
                else
                {
                    if (firstPoint == null)
                    {
                        firstPoint = GetPoint(CurrentNode, direction);
                        OuterPoints.Add(firstPoint.Location);
                    }
                    else
                    {
                        currentPoint = GetPoint(CurrentNode, direction);
                        if (currentPoint.Location == firstPoint.Location) { return OuterPoints; }
                        OuterPoints.Add(currentPoint.Location);
                    }
                }
            }
        }
        return OuterPoints;
    }

    Node FindFirstOuterNode(Node StartingNode, List<Node> nodesInArea, int direction)
    {
        Node OuterNode = null;
        Node CurrentNode = StartingNode;
        while (OuterNode == null)
        {
            Node NextNode = GetNodeInDirection(GetDirections()[direction], CurrentNode);
            if (NextNode == null || !nodesInArea.Contains(NextNode)) {
                if (!nodesInArea.Contains(GetNodeInDirection(GetDirections()[direction], NextNode)))
                {
                    OuterNode = CurrentNode;
                }        
            }
            CurrentNode = NextNode;
        }
        return OuterNode;
    }

    public List<Hex> GetAllHexesInThisRoom(string Room, Node StartNode)
    {
        CreateTable();
        List<Hex> hexesInRoom = new List<Hex>();
        List<Node> NodesChecked = new List<Node>();
        List<Node> NodesToCheck = new List<Node>();
        NodesToCheck.Add(StartNode);
        while (NodesToCheck.Count > 0)
        {
            List<Node> Neighbors = GetRealNeighborsNoDoor(NodesToCheck[0]);
            foreach(Node node in Neighbors)
            {
                if (NodesChecked.Contains(node)) { continue; }
                if (node == null) { continue; }
                if ((node.isAvailable || node.Used) && node.RoomName.Contains(Room))
                {
                    NodesToCheck.Add(node);
                    hexesInRoom.Add(node.GetComponent<Hex>());
                }
                NodesChecked.Add(node);
            }
            NodesToCheck.Remove(NodesToCheck[0]);
        }

        hexesInRoom.Add(StartNode.GetComponent<Hex>());
        return hexesInRoom;
    }

    public void AddHex(Node node) { Map.Add(GetHexHash(node.q, node.r), node); }
    public string GetHexHash(int x, int y) { return x.ToString() + "," + y.ToString(); }
    public Node GetNode(int x, int y) { return (Node)Map[GetHexHash(x, y)]; }

    public Node[] GetNeighbors(Node node)
    {
        return new Node[] {
            GetNode(node.q + 1, node.r),
            GetNode(node.q + 1, node.r - 1),
            GetNode(node.q, node.r - 1),
            GetNode(node.q - 1, node.r),
            GetNode(node.q - 1, node.r + 1),
            GetNode(node.q, node.r + 1),
        };
    }

    public List<Node> GetNeighborsNoRoom(Node node)
    {
        List<Node> RealNodes = new List<Node>();
        Node[] nodes = GetNeighbors(node);
        foreach (Node n in nodes)
        {
            if (n != null)
            {
                RealNodes.Add(n);
            }
        }
        return RealNodes;
    }
    

    public List<Node> GetRealNeighborsNoDoor(Node node)
    {
        List<Node> RealNodes = new List<Node>();
        Node[] nodes = GetNeighbors(node);
        foreach (Node n in nodes)
        {
            if (n != null && n.isConnectedToRoom(node))
            {
                RealNodes.Add(n);
            }
        }
        return RealNodes;
    }

    public List<Node> GetRealNeighbors(Node node)
    {
        List<Node> RealNodes = new List<Node>();
        Node[] nodes = GetNeighbors(node);
        foreach (Node n in nodes)
        {
            if (n.edge || !n.Shown) { continue; }
            if (n.GetComponent<Door>() != null && !n.GetComponent<Door>().isOpen) { continue; }
            if (n != null && n.isConnectedToRoom(node)) {
                RealNodes.Add(n);
            }
        }
        return RealNodes;
    }

    public Node GetClosestNodeFromNeighbors(Hex hexMovingNear, Character character)
    {
        List<Node> nodes =  GetRealNeighbors(hexMovingNear.GetComponent<Node>());
        if (nodes.Contains(character.HexOn.HexNode)) { return character.HexOn.HexNode; }
        return FindObjectOfType<AStar>().DiskatasWithArea(character.HexOn.HexNode, nodes, character.myCT);
    }

    public Vector2[] GetDirections()
    {
        return new Vector2[] {
            //East
            new Vector2(1, 0),
            //SouthEast
            new Vector2(0, +1),
            //SouthWest
            new Vector2(-1, +1),
            //West
            new Vector2(-1, 0),
            //NorthWest
            new Vector2(0, -1),
             //NorthEast
            new Vector2(1, -1),
        };
    }

    public int GetDirectionIndex(Vector2 direction)
    {
        for (int i = 0; i < 6; i++)
        {
            if (direction == GetDirections()[i]) { return i; }
        }
        return -1;
    }

    public int GetDistance(Node start, Node end)
    {
        return (Mathf.Abs(end.r - start.r) + Mathf.Abs(end.s - start.s) + Mathf.Abs(end.q - start.q)) / 2;
    }

    public List<Node> GetRange(Node StartNode, int MoveDistance)
    {
        List<Node> nodes = new List<Node>();
        for (int x = -MoveDistance; x <= MoveDistance; x++)
        {
            for (int y = -MoveDistance; y <= MoveDistance; y++)
            {
                for (int z = -MoveDistance; z <= MoveDistance; z++)
                {
                    if (x + y + z == 0) {nodes.Add(GetNode(StartNode.q + x, StartNode.r + y));}
                }
            }
        }
        return nodes;
    }


    public List<Node> GetDistanceRange(Node StartNode, int MoveDistance, Character.CharacterType CT)
    {
        List<Node> NodesInRange = GetRange(StartNode, MoveDistance);
        List<Node> NodesInDistance = new List<Node>();
        foreach(Node node in NodesInRange)
        {
            if (node == null) { continue; }
            if (node.isConnectedToRoom(node))
            {
                if (node.NodeHex.EntityHolding == null || (node.NodeHex.EntityHolding.GetComponent<Character>() != null && node.NodeHex.EntityHolding.GetComponent<Character>().myCT == CT))
                {
                    NodesInDistance.Add(node);
                }
            }
        }
        return NodesInDistance;
    }


    public List<Node> GetNodesAtDistanceFromNode(Node StartNode, int distance)
    {
        List<Node> NodesInRange = GetRange(StartNode, distance);
        List<Node> NodesinInDistance = new List<Node>();
        foreach (Node node in NodesInRange)
        {
            if (node == null) { continue; }
            if (node.isAvailable == false) { continue; }
            NodesinInDistance.Add(node);
        }
        return NodesinInDistance;
    }


    public List<Node> GetNodesAdjacent(Node node)
    {
        List<Node> NeighborsNodes = GetRealNeighbors(node);
        List<Node> AdjacentNodesAvailable = new List<Node>();
        foreach(Node aNode in NeighborsNodes)
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
                Vector2 direction = FindDirection(OriginNode, StartNode);
                Node nodeInCleave = GetNextCounterClockwizeNode(OriginNode, StartNode, direction);
                NodesinAOE.Add(StartNode);
                if (!IsAPossibleConnectedNode(nodeInCleave, StartNode)) { break; }
                NodesinAOE.Add(nodeInCleave);
                break;
            case AOEType.GreatCleave:
                Vector2 CleaveDirection = FindDirection(OriginNode, StartNode);
                Node node1InCleave = GetNextCounterClockwizeNode(OriginNode, StartNode, CleaveDirection);
                NodesinAOE.Add(StartNode);
                if (!IsAPossibleConnectedNode(node1InCleave, StartNode)) { break; }
                NodesinAOE.Add(node1InCleave);
                Vector2 Cleave2Direction = FindDirection(OriginNode, node1InCleave);
                Node node2InCleave = GetNextCounterClockwizeNode(OriginNode, node1InCleave, Cleave2Direction);
                if (!IsAPossibleConnectedNode(node2InCleave, StartNode)) { break; }
                NodesinAOE.Add(node2InCleave);
                break;
            case AOEType.Line:
                Vector2 lineDirection = FindDirection(OriginNode, StartNode);
                Node node = GetNextNodeInDirection(StartNode, lineDirection);
                NodesinAOE.Add(StartNode);
                if (!IsAPossibleConnectedNode(node, StartNode)) { break; }
                NodesinAOE.Add(node);
                break;
            case AOEType.SingleTarget:
                NodesinAOE.Add(StartNode);
                break;
            case AOEType.Surounding:
                List<Node> nodes = GetNodesSurrounding(OriginNode);
                foreach(Node myNode in nodes) {
                    if (!IsAPossibleConnectedNode(myNode, StartNode)) { continue; }
                    NodesinAOE.Add(myNode);
                }
                break;
            case AOEType.Circle:
                List<Node> CircleNodes = GetNodesSurrounding(OriginNode);
                foreach (Node myNode in CircleNodes) {
                    if (!IsAPossibleConnectedNode(myNode, StartNode)) { continue; }
                    NodesinAOE.Add(myNode);
                }
                NodesinAOE.Add(OriginNode);
                break;
            case AOEType.Triangle:

                break;
        }
        return NodesinAOE;
    }

    bool IsAPossibleConnectedNode(Node node1, Node node2)
    {
        if (!node1.isConnectedToRoom(node2)) { return false; }
        if (node1.edge) { return false; }
        if (!node1.Shown) { return false; }
        return true;
    }

    public List<Node> GetNodesInLOS(Node StartNode, int distance)
    {
        List<Node> nodes = GetNodesAtDistanceFromNode(StartNode, distance);
        List<Node> NodesInLOS = new List<Node>();
        foreach (Node node in nodes)
        {
            float rayDistance = (StartNode.transform.position - node.transform.position).magnitude;
            Vector3 direction = (node.transform.position - StartNode.transform.position).normalized;
            if (!Physics.Raycast(StartNode.transform.position, direction, rayDistance, WallLayer))
            {
                NodesInLOS.Add(node);
            }
            else
            {
               //Debug.Log("Hit Wall");
            }
        }
        return NodesInLOS;
    }

    public Vector2 FindDirection(Node StartNode, Node EndNode)
    {
        int Xdifference = EndNode.q - StartNode.q;
        int Ydifference = EndNode.r - StartNode.r;
        return (new Vector2(Xdifference, Ydifference));
    }

    List<Node> GetNodesInCircle(Node StartNode)
    {
        List<Node> nodes = new List<Node>();
        nodes.Add(StartNode);
        nodes.Add(GetNodeInDirection(GetDirections()[0], StartNode));
        nodes.Add(GetNodeInDirection(GetDirections()[1], StartNode));
        nodes.Add(GetNodeInDirection(GetDirections()[2], StartNode));
        nodes.Add(GetNodeInDirection(GetDirections()[3], StartNode));
        nodes.Add(GetNodeInDirection(GetDirections()[4], StartNode));
        nodes.Add(GetNodeInDirection(GetDirections()[5], StartNode));
        return nodes;
    }

    public List<Node> GetNodesSurrounding(Node StartNode)
    {
        List<Node> nodes = new List<Node>();
        nodes.Add(GetNodeInDirection(GetDirections()[0], StartNode));
        nodes.Add(GetNodeInDirection(GetDirections()[1], StartNode));
        nodes.Add(GetNodeInDirection(GetDirections()[2], StartNode));
        nodes.Add(GetNodeInDirection(GetDirections()[3], StartNode));
        nodes.Add(GetNodeInDirection(GetDirections()[4], StartNode));
        nodes.Add(GetNodeInDirection(GetDirections()[5], StartNode));
        return nodes;
    }

    public Node GetNextNodeInDirection(Node StartNode, Vector2 direction)
    {
        Node nextNode = GetNodeInDirection(direction, StartNode);
        return nextNode;
    }

    public Node GetNextCounterClockwizeNode(Node StartNode, Node EndNode, Vector2 DirectionOfEndNode)
    {
        int index = GetDirectionIndex(DirectionOfEndNode);
        if (index == -1) {
            Debug.LogError("Direction found is incompatable");
            return null;
        }
        index = index == 5 ? 0 : index + 1;
        Vector2 nextDirection = GetDirections()[index];
        Node nextNode = GetNodeInDirection(nextDirection, StartNode);
        return nextNode;
    }

    Node GetNodeInDirection(Vector2 direction, Node startNode)
    {
        return GetNode(startNode.q + (int)direction.x, startNode.r + (int)direction.y);
    }

}

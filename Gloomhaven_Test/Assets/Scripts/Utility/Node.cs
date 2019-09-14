using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<string> RoomName;
    public bool edge = false;
    public bool isAvailable = true;
    public bool Shown = false;

    public int q;
    public int r;
    public int s;

    public bool IsWalkable;
    public float G;
    public float H;
    public float F { get { return this.G + this.H; } }
    public NodeState State { get; set; }
    public Node ParentNode { get; set; }

    public Hex NodeHex;

    public bool isConnectedToRoom(Node node)
    {
        if (node.isAvailable)
        {
            if (node.RoomName.Count == this.RoomName.Count && node.RoomName[0] == this.RoomName[0])
            {
                return true;
            }
            //Moving to door
            else if (node.RoomName.Count > this.RoomName.Count && node.RoomName.Contains(this.RoomName[0]))
            {
                return true;
            }
            //Moving off door
            else if (this.RoomName.Count > node.RoomName.Count && this.RoomName.Contains(node.RoomName[0]))
            {
                return true;
            }
        }
        return false;
    }

    //EnviromentTile Tile;
    private void Start()
    {
        NodeHex = GetComponent<Hex>();
    }

    public static float GetTraversalCost(Point startLocation, Point endLocation)
    {
        return 1;
    }

    public void CalculateG(Node StartPoint)
    {
        G = (StartPoint.transform.position - transform.position).magnitude;
    }
    

    public void CalculateH(Node EndPoint)
    {
        H  = (EndPoint.transform.position - transform.position).magnitude;
    }


    public void SetNode(int r_, int q_)
    {
        r = r_;
        q = q_;
        s = -q_ - r_;
    }

}

public enum NodeState { Untested, Open, Closed }

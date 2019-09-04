using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<string> RoomName;
    public bool edge = false;
    public bool isAvailable = true;
    public bool Shown = false;

    public int X;
    public int Y;

    public Point Location { get; set; }
    public bool IsWalkable;
    public float G;
    public float H;
    public float F { get { return this.G + this.H; } }
    public NodeState State { get; set; }
    public Node ParentNode { get; set; }

    public Hex NodeHex;

    public bool isConnectedToRoom(Node node, bool edgeAvailable)
    {
        if (node.isAvailable || (node.edge  && edgeAvailable))
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
        Location = new Point(X, Y);
        FindObjectOfType<HexMapController>().SetNodeMap(this, X, Y);
    }

    // TODO change this to actual distance 
    public static float GetTraversalCost(Point startLocation, Point endLocation)
    {
        return 1;
    }

    public void CalculateG(Node StartPoint)
    {
        G = (StartPoint.transform.position - transform.position).magnitude;



        //int dx = (StartPoint.X - this.Location.X);
        //int dy = (StartPoint.Y - this.Location.Y);
        //if (Mathf.Sign(dx) == Mathf.Sign(dy))
        //{
        //    G = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        //}
        //else
        //{
        //    G = Mathf.Abs(dx) + Mathf.Abs(dy);
        //}
        //G = Mathf.Abs(StartPoint.X - this.Location.X) + Mathf.Abs(StartPoint.Y - this.Location.Y);
    }
    

    public void CalculateH(Node EndPoint)
    { 

        H  = (EndPoint.transform.position - transform.position).magnitude;
        //{
        //    int dx = (EndPoint.X - this.Location.X);
        //    int dy = (EndPoint.Y - this.Location.Y);
        //    if (Mathf.Sign(dx) == Mathf.Sign(dy))
        //    {
        //        H = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        //    }
        //    else
        //    {
        //        H = Mathf.Abs(dx) + Mathf.Abs(dy);
        //    }

        //H = Mathf.Abs(EndPoint.X - this.Location.X) + Mathf.Abs(EndPoint.Y - this.Location.Y);
    }


    public void SetNode(int x, int y)
    {
        Location = new Point(x, y);
        X = x;
        Y = y;
    }

    

}

public enum NodeState { Untested, Open, Closed }

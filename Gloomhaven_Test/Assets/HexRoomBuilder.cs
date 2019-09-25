using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Odds are on side doors (Doors with door hex and connection hex as same)
//Even are on vertical doors (Door hex on the door)
public enum RoomType
{
    A1,
    A3,
    B2,
    B4,
};

public class HexRoomBuilder : MonoBehaviour {

    HexMapController HexController;
    string RoomName;
    public LayerMask WallLayer;

    public List<Hex> BuildRoom(RoomType room, Node StartNode, string roomName)
    {
        HexController = GetComponent<HexMapController>();
        HexController.CreateTable();
        RoomName = roomName;
        if (!RoomAvailableToBuild(room, StartNode)) {
            Debug.LogWarning("Room unable to be made since a room node is either null or already taken");
            return null;
        }
        List<Hex> NewRoom = ConstructRoom(room, StartNode);
        return NewRoom;
    }

    List<Hex> ConstructRoom(RoomType room, Node StartNode)
    {
        int Q = StartNode.q;
        int R = StartNode.r;
        List<Hex> Room = new List<Hex>();
        foreach (Vector4 delta in RoomDelta(room))
        {
            if (delta.z == 0)
            {
                Room.Add(SetNodeAsNormalHex(Q + (int)delta.x, R + (int)delta.y, RoomName));
            }
            else if (delta.z == 1)
            {
                Room.Add(SetNodeAsFragmentHex(Q + (int)delta.x, R + (int)delta.y, (int)delta.w, RoomName));
            }
            else if (delta.z == 2)
            {
                Room.Add(SetNodeAsHalfHex(Q + (int)delta.x, R + (int)delta.y, (int)delta.w, RoomName));
            }
        }
        return Room;
    }

    bool RoomAvailableToBuild(RoomType room, Node StartNode)
    {
        List<Node> RoomNodes = new List<Node>();
        int Q = StartNode.q;
        int R = StartNode.r;
        foreach(Vector4 delta in RoomDelta(room))
        {
            if (CheckNodeIfTakenOrNull(Q + (int)delta.x, R + (int)delta.y)) { return false; }
        }
        return true;
    }

    bool CheckNodeIfTakenOrNull(int q, int r)
    {
        Node node = HexController.GetNode(q, r);
        if (node == null || node.isAvailable) {
            Debug.Log(node == null);
            Debug.Log(q + "," + r);
            return true;
        }
        return false;
    }

    Hex SetNodeAsNormalHex(int q, int r, string RoomName)
    {
        Node node = HexController.GetNode(q, r);
        node.SetRoomName(RoomName);
        return node.GetComponent<Hex>();
    }

    Hex SetNodeAsFragmentHex(int q, int r, int a, string RoomName)
    {
        Node node = HexController.GetNode(q, r);
        node.SetRoomName(RoomName);
        node.GetComponent<HexAdjuster>().SetHexToFragment();
        EndHex endHex = node.gameObject.AddComponent<EndHex>();
        endHex.WallLayer = WallLayer;
        switch (a)
        {
            case 0:
                node.GetComponent<HexAdjuster>().RotateHexToBottomLeft();
                break;
            case 1:
                node.GetComponent<HexAdjuster>().RotateHexToBottomMiddle();
                break;
            case 2:
                node.GetComponent<HexAdjuster>().RotateHexToBottomRight();
                break;
            case 3:
                node.GetComponent<HexAdjuster>().RotateHexToTopRight();
                break;
            case 4:
                node.GetComponent<HexAdjuster>().RotateHexToTopMiddle();
                break;
            case 5:
                node.GetComponent<HexAdjuster>().RotateHexToTopLeft();
                break;
        }
        return node.GetComponent<Hex>();
    }

    Hex SetNodeAsHalfHex(int q, int r, int a, string RoomName)
    {
        Node node = HexController.GetNode(q, r);
        node.SetRoomName(RoomName);
        node.GetComponent<HexAdjuster>().SetHexToHalf();
        EndHex endHex = node.gameObject.AddComponent<EndHex>();
        endHex.WallLayer = WallLayer;
        switch (a)
        {
            case 0:
                node.GetComponent<HexAdjuster>().RotateHexToBottomLeft();
                break;
            case 1:
                node.GetComponent<HexAdjuster>().RotateHexToBottomMiddle();
                break;
            case 2:
                node.GetComponent<HexAdjuster>().RotateHexToBottomRight();
                break;
            case 3:
                node.GetComponent<HexAdjuster>().RotateHexToTopRight();
                break;
            case 4:
                node.GetComponent<HexAdjuster>().RotateHexToTopMiddle();
                break;
            case 5:
                node.GetComponent<HexAdjuster>().RotateHexToTopLeft();
                break;
        }
        return node.GetComponent<Hex>();
    }


    public List<Vector4> RoomDelta(RoomType room)
    {
        List<Vector4> Deltas = new List<Vector4>();
        switch (room)
        {
            case RoomType.A1:
                Deltas.Add(new Vector4(-3, 2, 0));
                Deltas.Add(new Vector4(-4, 2, 0));
                Deltas.Add(new Vector4(-5, 2, 0));
                Deltas.Add(new Vector4(-6, 2, 0));
                Deltas.Add(new Vector4(-7, 2, 0));

                Deltas.Add(new Vector4(-2, 1, 0));
                Deltas.Add(new Vector4(-3, 1, 0));
                Deltas.Add(new Vector4(-4, 1, 0));
                Deltas.Add(new Vector4(-5, 1, 0));
                Deltas.Add(new Vector4(-6, 1, 0));
                Deltas.Add(new Vector4(-7, 1, 0));

                Deltas.Add(new Vector4(-1, 0, 0));
                Deltas.Add(new Vector4(-2, 0, 0));
                Deltas.Add(new Vector4(-3, 0, 0));
                Deltas.Add(new Vector4(-4, 0, 0));
                Deltas.Add(new Vector4(-5, 0, 0));
                Deltas.Add(new Vector4(-6, 0, 0));
                Deltas.Add(new Vector4(-7, 0, 0));

                Deltas.Add(new Vector4(-1, -1, 0));
                Deltas.Add(new Vector4(-2, -1, 0));
                Deltas.Add(new Vector4(-3, -1, 0));
                Deltas.Add(new Vector4(-4, -1, 0));
                Deltas.Add(new Vector4(-5, -1, 0));
                Deltas.Add(new Vector4(-6, -1, 0));

                Deltas.Add(new Vector4(-1, -2, 0));
                Deltas.Add(new Vector4(-2, -2, 0));
                Deltas.Add(new Vector4(-3, -2, 0));
                Deltas.Add(new Vector4(-4, -2, 0));
                Deltas.Add(new Vector4(-5, -2, 0));

                Deltas.Add(new Vector4(-4, 3, 1, 1));
                Deltas.Add(new Vector4(-5, 3, 1, 1));
                Deltas.Add(new Vector4(-6, 3, 1, 1));
                Deltas.Add(new Vector4(-7, 3, 1, 1));

                Deltas.Add(new Vector4(-2, 2, 1, 2));
                Deltas.Add(new Vector4(-8, 2, 1, 0));

                Deltas.Add(new Vector4(0, -2, 1, 3));
                Deltas.Add(new Vector4(-6, -2, 1, 5));

                Deltas.Add(new Vector4(-1, -3, 1, 4));
                Deltas.Add(new Vector4(-2, -3, 1, 4));
                Deltas.Add(new Vector4(-3, -3, 1, 4));
                Deltas.Add(new Vector4(-4, -3, 1, 4));
                break;
            case RoomType.A3:
                Deltas.Add(new Vector4(1, 2, 0));
                Deltas.Add(new Vector4(2, 2, 0));
                Deltas.Add(new Vector4(3, 2, 0));
                Deltas.Add(new Vector4(4, 2, 0));
                Deltas.Add(new Vector4(5, 2, 0));

                Deltas.Add(new Vector4(1, 1, 0));
                Deltas.Add(new Vector4(2, 1, 0));
                Deltas.Add(new Vector4(3, 1, 0));
                Deltas.Add(new Vector4(4, 1, 0));
                Deltas.Add(new Vector4(5, 1, 0));
                Deltas.Add(new Vector4(6, 1, 0));

                Deltas.Add(new Vector4(1, 0, 0));
                Deltas.Add(new Vector4(2, 0, 0));
                Deltas.Add(new Vector4(3, 0, 0));
                Deltas.Add(new Vector4(4, 0, 0));
                Deltas.Add(new Vector4(5, 0, 0));
                Deltas.Add(new Vector4(6, 0, 0));
                Deltas.Add(new Vector4(7, 0, 0));

                Deltas.Add(new Vector4(2, -1, 0));
                Deltas.Add(new Vector4(3, -1, 0));
                Deltas.Add(new Vector4(4, -1, 0));
                Deltas.Add(new Vector4(5, -1, 0));
                Deltas.Add(new Vector4(6, -1, 0));
                Deltas.Add(new Vector4(7, -1, 0));

                Deltas.Add(new Vector4(3, -2, 0));
                Deltas.Add(new Vector4(4, -2, 0));
                Deltas.Add(new Vector4(5, -2, 0));
                Deltas.Add(new Vector4(6, -2, 0));
                Deltas.Add(new Vector4(7, -2, 0));

                Deltas.Add(new Vector4(1, 3, 1, 1));
                Deltas.Add(new Vector4(2, 3, 1, 1));
                Deltas.Add(new Vector4(3, 3, 1, 1));
                Deltas.Add(new Vector4(4, 3, 1, 1));

                Deltas.Add(new Vector4(0, 2, 1, 0));
                Deltas.Add(new Vector4(6, 2, 1, 2));

                Deltas.Add(new Vector4(2, -2, 1, 5));
                Deltas.Add(new Vector4(8, -2, 1, 3));

                Deltas.Add(new Vector4(4, -3, 1, 4));
                Deltas.Add(new Vector4(5, -3, 1, 4));
                Deltas.Add(new Vector4(6, -3, 1, 4));
                Deltas.Add(new Vector4(7, -3, 1, 4));
                break;
            case RoomType.B2:
                Deltas.Add(new Vector4(0, 1, 0));
                Deltas.Add(new Vector4(0, 2, 0));
                Deltas.Add(new Vector4(0, 3, 0));
                Deltas.Add(new Vector4(-1, 1, 0));
                Deltas.Add(new Vector4(-1, 2, 0));
                Deltas.Add(new Vector4(-1, 3, 0));
                Deltas.Add(new Vector4(-1, 4, 0));
                Deltas.Add(new Vector4(-1, 5, 0));
                Deltas.Add(new Vector4(-2, 2, 0));
                Deltas.Add(new Vector4(-2, 3, 0));
                Deltas.Add(new Vector4(-2, 4, 0));
                Deltas.Add(new Vector4(-2, 5, 0));
                Deltas.Add(new Vector4(-2, 6, 0));
                Deltas.Add(new Vector4(-3, 3, 0));
                Deltas.Add(new Vector4(-3, 4, 0));
                Deltas.Add(new Vector4(-3, 5, 0));
                Deltas.Add(new Vector4(-3, 6, 0));
                Deltas.Add(new Vector4(-3, 7, 0));
                Deltas.Add(new Vector4(-4, 5, 0));
                Deltas.Add(new Vector4(-4, 6, 0));
                Deltas.Add(new Vector4(-4, 7, 0));

                Deltas.Add(new Vector4(1, 1, 2, 4));
                Deltas.Add(new Vector4(1, 2, 2, 3));
                Deltas.Add(new Vector4(0, 4, 2, 3));
                Deltas.Add(new Vector4(-1, 6, 2, 3));
                Deltas.Add(new Vector4(-2, 7, 2, 2));
                Deltas.Add(new Vector4(-5, 7, 2, 1));
                Deltas.Add(new Vector4(-5, 6, 2, 0));
                Deltas.Add(new Vector4(-4, 4, 2, 0));
                Deltas.Add(new Vector4(-3, 2, 2, 0));
                Deltas.Add(new Vector4(-2, 1, 2, 5));

                Deltas.Add(new Vector4(-4, 8, 1, 1));
                break;
            case RoomType.B4:
                Deltas.Add(new Vector4(0, -1, 0));
                Deltas.Add(new Vector4(0, -2, 0));
                Deltas.Add(new Vector4(0, -3, 0));
                Deltas.Add(new Vector4(1, -1, 0));
                Deltas.Add(new Vector4(1, -2, 0));
                Deltas.Add(new Vector4(1, -3, 0));
                Deltas.Add(new Vector4(1, -4, 0));
                Deltas.Add(new Vector4(1, -5, 0));
                Deltas.Add(new Vector4(2, -2, 0));
                Deltas.Add(new Vector4(2, -3, 0));
                Deltas.Add(new Vector4(2, -4, 0));
                Deltas.Add(new Vector4(2, -5, 0));
                Deltas.Add(new Vector4(2, -6, 0));
                Deltas.Add(new Vector4(3, -3, 0));
                Deltas.Add(new Vector4(3, -4, 0));
                Deltas.Add(new Vector4(3, -5, 0));
                Deltas.Add(new Vector4(3, -6, 0));
                Deltas.Add(new Vector4(3, -7, 0));
                Deltas.Add(new Vector4(4, -5, 0));
                Deltas.Add(new Vector4(4, -6, 0));
                Deltas.Add(new Vector4(4, -7, 0));

                Deltas.Add(new Vector4(-1, -1, 2, 1));
                Deltas.Add(new Vector4(-1, -2, 2, 0));
                Deltas.Add(new Vector4(0, -4, 2, 0));
                Deltas.Add(new Vector4(1, -6, 2, 0));
                Deltas.Add(new Vector4(2, -7, 2, 5));
                Deltas.Add(new Vector4(5, -7, 2, 4));
                Deltas.Add(new Vector4(5, -6, 2, 3));
                Deltas.Add(new Vector4(4, -4, 2, 3));
                Deltas.Add(new Vector4(3, -2, 2, 3));
                Deltas.Add(new Vector4(2, -1, 2, 2));

                Deltas.Add(new Vector4(4, -8, 1, 4));
                break;
        }
        return Deltas;
    }

}

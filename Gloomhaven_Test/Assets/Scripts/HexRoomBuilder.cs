﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Odds are on side doors (Doors with door hex and connection hex as same)
//Even are on vertical doors (Door hex on the door)

public enum RoomSide
{
    Right,
    Left,
    Top,
    Down,
};

public class HexRoomBuilder : MonoBehaviour {

    HexMapController HexController;
    string RoomName;
    public LayerMask WallLayer;

    public List<Hex> BuildRoomBySize(Node startNode, int height, int width, string roomName, RoomSide RS)
    {
        if (RS == RoomSide.Left || RS == RoomSide.Right)
        {
            if (height % 4 != 1)
            {
                Debug.LogWarning("Height must be every other odd number ie: 1, 5, 9, 13...");
                return null;
            }
            if (width % 2 != 1)
            {
                Debug.LogWarning("Width must be odd number");
                return null;
            }
        }
        else if (RS == RoomSide.Down || RS == RoomSide.Top)
        {
            if (height % 2 != 0)
            {
                Debug.LogWarning("Height must be even number");
                return null;
            }
        }

            HexController = GetComponent<HexMapController>();
        HexController.CreateTable();
        RoomName = roomName;
        if (!RoomAvailableToBuild(startNode, RoomByDimensions(height, width, RS)))
        {
            Debug.LogWarning("Room unable to be made since a room node is either null or already taken");
            return null;
        }
        List<Hex> NewRoom = ConstructRoom(startNode, RoomByDimensions(height, width, RS));
        return NewRoom;
    }

    List<Hex> ConstructRoom(Node StartNode, List<Vector4> roomDeltas)
    {
        int Q = StartNode.q;
        int R = StartNode.r;
        List<Hex> Room = new List<Hex>();
        foreach (Vector4 delta in roomDeltas)
        {
            if (delta.z == 0)
            {
                Room.Add(SetNodeAsNormalHex(Q + (int)delta.x, R + (int)delta.y, (int)delta.w, RoomName));
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

    bool RoomAvailableToBuild(Node StartNode, List<Vector4> roomDeltas)
    {
        int Q = StartNode.q;
        int R = StartNode.r;
        foreach(Vector4 delta in roomDeltas)
        {
            if (CheckNodeIfTakenOrNull(Q + (int)delta.x, R + (int)delta.y)) {
                return false;
            }
        }
        return true;
    }

    bool CheckNodeIfTakenOrNull(int q, int r)
    {
        Node node = HexController.GetNode(q, r);
        if (node == null || (node.isAvailable && !node.edge)) {
            //Debug.Log(node == null);
            //Debug.Log(q + "," + r);
            return true;
        }
        return false;
    }

    Hex SetNodeAsNormalHex(int q, int r, int a, string RoomName)
    {
        Node node = HexController.GetNode(q, r);
        node.GetComponent<HexAdjuster>().ClearSides();
        node.GetComponent<HexAdjuster>().SetHexToFull();
        node.GetComponent<HexWallAdjuster>().DestroyAllWalls();
        //Right Wall
        if (a == 1)
        {
            node.GetComponent<HexWallAdjuster>().CreateHexWall(0, 1, RoomName);
        }
        //Left Wall
        else if(a == 2)
        {
            node.GetComponent<HexWallAdjuster>().CreateHexWall(0, 0, RoomName);
        }
        node.SetRoomName(RoomName);
        return node.GetComponent<Hex>();
    }

    Hex SetNodeAsFragmentHex(int q, int r, int a, string RoomName)
    {
        Node node = HexController.GetNode(q, r);
        switch (a)
        {
            case 4:
                node.GetComponent<HexAdjuster>().UpRoomSide = RoomName;
                break;
            case 1:
                node.GetComponent<HexAdjuster>().DownRoomSide = RoomName;
                break;
        }
        if (node.isAvailable)
        {
            node.AddRoomName(RoomName);
            node.GetComponent<HexWallAdjuster>().CreateHexWall(2, 1, RoomName);
        }
        else
        {
            node.SetRoomName(RoomName);
            node.GetComponent<HexAdjuster>().SetHexToFragment();
            switch (a)
            {
                case 0:
                    node.GetComponent<HexAdjuster>().RotateHexToBottomLeft();
                    break;
                case 1:
                    node.GetComponent<HexAdjuster>().RotateHexToBottomMiddle();
                    node.GetComponent<HexWallAdjuster>().CreateHexWall(2, 0, RoomName);
                    break;
                case 2:
                    node.GetComponent<HexAdjuster>().RotateHexToBottomRight();
                    break;
                case 3:
                    node.GetComponent<HexAdjuster>().RotateHexToTopRight();
                    break;
                case 4:
                    node.GetComponent<HexAdjuster>().RotateHexToTopMiddle();
                    node.GetComponent<HexWallAdjuster>().CreateHexWall(2, 0, RoomName);
                    break;
                case 5:
                    node.GetComponent<HexAdjuster>().RotateHexToTopLeft();
                    break;
            }
        }
        return node.GetComponent<Hex>();
    }

    Hex SetNodeAsHalfHex(int q, int r, int a, string RoomName)
    {
        Node node = HexController.GetNode(q, r);
        switch (a)
        {
            case 0:
                node.GetComponent<HexAdjuster>().LeftRoomSide = RoomName;
                break;
            case 3:
                node.GetComponent<HexAdjuster>().RightRoomSide = RoomName;
                break;
        }
        if (node.isAvailable)
        {
            node.AddRoomName(RoomName);
            EndHex endHex = node.gameObject.AddComponent<EndHex>();
            endHex.WallLayer = WallLayer;
            node.GetComponentInChildren<FlatWall>().LinkWallToRoom(RoomName);
        }
        else
        {
            node.SetRoomName(RoomName);
            node.GetComponent<HexAdjuster>().SetHexToHalf();
            EndHex endHex = node.gameObject.AddComponent<EndHex>();
            endHex.WallLayer = WallLayer;
            node.GetComponent<HexWallAdjuster>().CreateHexWall(1, 0, RoomName);
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
        }
        return node.GetComponent<Hex>();
    }

    public List<Vector4> RoomByDimensions(int height, int width, RoomSide rs)
    {
        switch (rs)
        {
            case RoomSide.Right:
                return (RoomRightByDimensions(height, width));
            case RoomSide.Left:
                return (RoomLeftByDimensions(height, width));
            case RoomSide.Top:
                return (RoomUpByDimensions(height, width));
            case RoomSide.Down:
                return (RoomDownByDimensions(height, width));
        }
        return null;
    }

    public List<Vector4> RoomDownByDimensions(int height, int width)
    {
        List<Vector4> deltas = new List<Vector4>();
        int t = (int)Mathf.Floor(width / 2);
        for (int r = -1; r > -height; r--)
        {
            int r_offset = (int)Mathf.Round(r / 2);
            if (r < 0 && r % 2 != 0) { r_offset--; }
            for (int q = -r_offset - t; q <= (t - r_offset); q++)
            {
                if (r == -1)
                {
                    if (!(q - 1 == 0 && r + 1 == 0))
                    {
                        deltas.Add(new Vector4(q - 1, r + 1, 1, 1));
                    }
                }
                if (r == -height + 1) { deltas.Add(new Vector4(q, r - 1, 1, 4)); }

                if ((q == -r_offset - t) && (r % 2 != 0))
                {
                    deltas.Add(new Vector4(q - 1, r, 2, 0));
                }
                if ((q == -r_offset - t) && (r % 2 == 0))
                {
                    deltas.Add(new Vector4(q, r, 0, 1));
                    continue;
                }

                if ((q == t - r_offset) && (r % 2 != 0))
                {
                    deltas.Add(new Vector4(q, r, 2, 3));
                    continue;
                }

                if ((q == (t - r_offset)) && (r % 2 == 0))
                {
                    deltas.Add(new Vector4(q, r, 0, 2));
                    continue;
                }

                deltas.Add(new Vector4(q, r, 0, 0));
            }
        }
        return deltas;
    }

    public List<Vector4> RoomUpByDimensions(int height, int width)
    {
        List<Vector4> deltas = new List<Vector4>();
        int t = (int)Mathf.Floor(width / 2);
        for (int r = 1; r < height; r++)
        {
            int r_offset = (int)Mathf.Round(r / 2);
            if (r > 0 && r % 2 != 0) { r_offset++; }
            for (int q = -r_offset - t; q <= (t - r_offset); q++)
            {
                if (r == 1)
                {
                    if (!(q + 1 == 0 && r - 1 == 0))
                    {
                        deltas.Add(new Vector4(q + 1, r - 1, 1, 4));
                    }
                }
                if (r == height - 1) { deltas.Add(new Vector4(q, r + 1, 1, 1)); }

                if ((q == -r_offset - t) && (r % 2 != 0))
                {
                    deltas.Add(new Vector4(q, r, 2, 0));
                    continue;
                }
                if ((q == -r_offset - t) && (r % 2 == 0))
                {
                    deltas.Add(new Vector4(q, r, 0, 1));
                    continue;
                }

                if ((q == t - r_offset) && (r % 2 != 0))
                {
                    deltas.Add(new Vector4(q + 1, r, 2, 3));
                }

                if ((q == (t - r_offset)) && (r % 2 == 0))
                {
                    deltas.Add(new Vector4(q, r, 0, 2));
                    continue;
                }

                deltas.Add(new Vector4(q, r, 0, 0));
            }
        }
        return deltas;
    }

    public List<Vector4> RoomLeftByDimensions(int height, int width)
    {
        List<Vector4> deltas = new List<Vector4>();
        int t = (int)Mathf.Floor(height / 2);
        for (int r = -t; r <= t; r++)
        {
            int r_offset = (int)Mathf.Round(r / 2);
            if (r > 0 && r % 2 != 0) { r_offset++; }
            for (int q = -r_offset + 1; q < (width - r_offset); q++)
            {
                if (r == -t) { deltas.Add(new Vector4(q, r - 1, 1, 4)); }
                if (r == t) { deltas.Add(new Vector4(q - 1, r + 1, 1, 1)); }

                if ((q == -r_offset + 1) && (r % 2 == 0)) { if (!(q - 1 == 0 && r == 0)) { deltas.Add(new Vector4(q - 1, r, 2, 0)); } }
                if ((q == -r_offset + 1) && (r % 2 != 0))
                {
                    if (!(q == 0 && r == 0))
                    {
                        deltas.Add(new Vector4(q, r, 0, 1));
                        continue;
                    }
                }
                if ((q == (width - r_offset) - 1) && (r % 2 == 0))
                {
                    deltas.Add(new Vector4(q, r, 2, 3));
                    continue;
                }
                if ((q == (width - r_offset) - 1) && (r % 2 != 0))
                {
                    deltas.Add(new Vector4(q, r, 0, 2));
                    continue;
                }

                deltas.Add(new Vector4(q, r, 0, 0));
            }
        }
        return deltas;
    }

    public List<Vector4> RoomRightByDimensions(int height, int width)
    {
        List<Vector4> deltas = new List<Vector4>();
        int t = (int)Mathf.Floor(height / 2);
        for (int r = -t; r <= t; r++)
        {
            int r_offset = (int)Mathf.Round(r / 2);
            if (r < 0 && r % 2 != 0) { r_offset--; }
            for (int q = -r_offset - 1; q > (-width - r_offset); q--)
            {
                if (r == -t) { deltas.Add(new Vector4(q + 1, r - 1, 1, 4)); }
                if (r == t) { deltas.Add(new Vector4(q, r + 1, 1, 1)); }

                if ((q == -r_offset - 1) && (r % 2 == 0)) { if (!(q + 1 == 0 && r == 0)) { deltas.Add(new Vector4(q + 1, r, 2, 3)); } }
                if ((q == -r_offset - 1) && (r % 2 != 0))
                {
                    if (!(q == 0 && r == 0))
                    {
                        deltas.Add(new Vector4(q, r, 0, 2));
                        continue;
                    }
                }
                if ((q == (-width - r_offset) + 1) && (r % 2 == 0))
                {
                    deltas.Add(new Vector4(q, r, 2, 0));
                    continue;
                }
                if ((q == (-width - r_offset) + 1) && (r % 2 != 0))
                {
                    deltas.Add(new Vector4(q, r, 0, 1));
                    continue;
                }

                deltas.Add(new Vector4(q, r, 0, 0));
            }
        }
        return deltas;
    }

}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class EdgeHexes
{
    public List<Hex> Edges = new List<Hex>();
    public void AddHex(Hex hex) { Edges.Add(hex); }
    public void RemoveHex(Hex hex) { Edges.Remove(hex); }
}

class Rooms
{
    public string RoomName = "";
    public List<Hex> RoomHexes = new List<Hex>();
    public void AddHex(Hex hex) { RoomHexes.Add(hex); }
    public void RemoveHex(Hex hex) { RoomHexes.Remove(hex); }
}

public class ProceduralMapCreator : MonoBehaviour {

    public int ChallengeRating = 15;
    public int MaxLength = 15;
    public List<GameObject> ObstaclePool = new List<GameObject>();
    public List<GameObject> EnemyPool = new List<GameObject>();

    HexMapController hexMap;
    HexRoomBuilder hexRoomBuilder;
    public int RoomIndex = 0;

    List<EdgeHexes> EdgesAvailable;
    List<Rooms> RoomsMade; 

    //Create a random room size

    //Add Enemies dependent on size, enemies that have already been added, and randomness
    //With a max enemy for the room
    //Randomely select a hex to place the enemy on

    //Add Obstacles dependent on room size, max obstacles for the room and randomness
    //Select random hexes for the room.

    // Add a door either 1 or 3 dependent on randomness

    //repeat

    public void BuildNewMap()
    {
        FindObjectOfType<HexMapBuilder>().DestroyMap();
        FindObjectOfType<HexMapBuilder>().BuildMap();
        BuildMap();
    }

    public void BuildMap()
    {
        RoomIndex = 0;
        EdgesAvailable = new List<EdgeHexes>();
        RoomsMade = new List<Rooms>();

        hexMap = FindObjectOfType<HexMapController>();
        hexMap.CreateTable();
        hexRoomBuilder = FindObjectOfType<HexRoomBuilder>();
        List<Hex> StartHexes = CreateStartRoom();
        CollectAndSortEdges(StartHexes, new List<Hex>());
        if (StartHexes.Count == 0) { return; }
        CreateNextRoom();
        PopulateRooms();
    }

    void PopulateRooms()
    {
        int RoomsWithEnemiesInside = ChallengeRating / 3;
        List<Rooms> RoomToBuildEnemies = new List<Rooms>();
        foreach(Rooms room in RoomsMade) { RoomToBuildEnemies.Add(room); }
        for (int i = 0; i < RoomsWithEnemiesInside; i++)
        {
            int RandomIndex = Random.Range(0, RoomToBuildEnemies.Count);
            AddEnemies(RoomToBuildEnemies[RandomIndex].RoomHexes);
            RoomToBuildEnemies.Remove(RoomToBuildEnemies[RandomIndex]);
        }    

        foreach(Rooms room in RoomsMade)
        {
            AddObstaclesToRoom(room.RoomHexes);
            ShowHexSet(room.RoomHexes, room.RoomName);
        }
    }

    void CollectAndSortEdges(List<Hex> hexes, List<Hex> EdgeFrom)
    {
        List<Hex> edgeHexes = GetEdgeHexes(hexes);
        EdgeHexes UpHex = new EdgeHexes();
        EdgeHexes RightHex = new EdgeHexes();
        EdgeHexes DownHex = new EdgeHexes();
        EdgeHexes LeftHex = new EdgeHexes();
        int i = 0;
        while (i < edgeHexes.Count)
        {
            if (EdgeFrom.Contains(edgeHexes[i]))
            {
                edgeHexes.Remove(edgeHexes[i]);
            }
            else
            {
                i++;
            }
        }
        foreach (Hex hex in edgeHexes)
        {
            if (hex.GetComponent<HexAdjuster>().DownRoomSide != "") { UpHex.AddHex(hex); }
            else if (hex.GetComponent<HexAdjuster>().LeftRoomSide != "") { RightHex.AddHex(hex); }
            else if (hex.GetComponent<HexAdjuster>().UpRoomSide != "") { DownHex.AddHex(hex); }
            else if (hex.GetComponent<HexAdjuster>().RightRoomSide != "") { LeftHex.AddHex(hex); }
        }
        if (UpHex.Edges.Count > 0) { EdgesAvailable.Add(UpHex); }
        if (RightHex.Edges.Count > 0) { EdgesAvailable.Add(RightHex); }
        if (DownHex.Edges.Count > 0) { EdgesAvailable.Add(DownHex); }
        if (LeftHex.Edges.Count > 0) { EdgesAvailable.Add(LeftHex); }
    }

    List<Hex> CreateStartRoom()
    {
        List<Hex> hexes = BuildRoom(0, 0, RoomSide.Top);
        if (hexes.Count == 0) { Debug.LogWarning("Room not made!!"); }
        string RoomName = ((char)((int)('A') + RoomIndex)).ToString();
        Node StartNode = hexMap.GetNode(0, 0);
        foreach(Hex hex in hexes)
        {
            hex.GetComponent<Node>().Shown = true;
        }
        ShowHexSet(hexes, RoomName);
        SetStartNode(StartNode, RoomName);
        RoomIndex++;
        return hexes;
    }

    void CreateNextRoom()
    {
        if (EdgesAvailable.Count <= 0) { return; }
        int RandomEdgeIndex = Random.Range(0, EdgesAvailable.Count);
        EdgeHexes edge = EdgesAvailable[RandomEdgeIndex];
        int RandomHexIndex = Random.Range(0, edge.Edges.Count);
        Hex hex = edge.Edges[RandomHexIndex];
        EdgesAvailable.Remove(edge);
        if (!IsAlreadySurrounded(hex.GetComponent<HexAdjuster>()))
        {
            RoomSide roomToBuildTowards = GetRoomSideToBuildTowards(hex.GetComponent<HexAdjuster>());
            Node node = hex.GetComponent<Node>();
            List<Hex> NewHexes = BuildRoom(node.q, node.r, roomToBuildTowards);
            if (NewHexes != null && NewHexes.Count > 0)
            {
                string NewRoomName = ((char)((int)('A') + RoomIndex)).ToString();

                BuildDoor(hex, NewHexes, roomToBuildTowards, NewRoomName);

                //AddObstaclesToRoom(NewHexes);

                // To show all rooms
                //ShowHexSet(NewHexes, NewRoomName, node);
                //

                //To Hide all rooms
                SetNexHexes(NewHexes);
                //
                RoomIndex++;
                CollectAndSortEdges(NewHexes, edge.Edges);
            }
        }
        else
        {
            Debug.Log("Building Door In Between");
            CreateDoorInBetweenRooms(hex);
            RemoveSharedEdges(edge);
        }
        CreateNextRoom();
    }

    void AddEnemies(List<Hex> hexes)
    {
        if (ChallengeRating <= 0) { return; }
        List<Hex> NonEdgeHexes = GetNonEdgeHexes(hexes);
        int EnemiesToSpawn = Random.Range(2, 5);
        int RoomChallengeRating = 0;
        for (int i= 0; i< EnemiesToSpawn; i++)
        {
            GameObject RandomEnemy = EnemyPool[Random.Range(0, EnemyPool.Count)];
            if (NonEdgeHexes.Count <= 0) { continue; }
            Hex RandomHex = NonEdgeHexes[Random.Range(0, NonEdgeHexes.Count)];
            RandomHex.EntityToSpawn = RandomEnemy.GetComponent<Entity>();
            NonEdgeHexes.Remove(RandomHex);
            RoomChallengeRating++;
        }
        ChallengeRating -= RoomChallengeRating;
    }

    void AddObstaclesToRoom(List<Hex> hexes)
    {
        List<Hex> NonEdgeHexes = GetNonEdgeHexes(hexes);
        int MaximumObstaclesShouldPlace = NonEdgeHexes.Count / 10;
        for (int i =0; i < MaximumObstaclesShouldPlace; i++)
        {
            int RandomLocation = Random.Range(0, NonEdgeHexes.Count);
            Hex AttemptHex = NonEdgeHexes[RandomLocation];
           if (HexNotNextToOtherObstacleOrDoor(AttemptHex))
            {
                int RandomObstacleIndex = Random.Range(0, ObstaclePool.Count);
                AttemptHex.EntityToSpawn = ObstaclePool[RandomObstacleIndex].GetComponent<Entity>(); ;
            }
        }
    }

    bool HexNotNextToOtherObstacleOrDoor(Hex hex)
    {
        if (hex.EntityToSpawn != null) { return false; }
        List<Node> AdjacentNodes = hexMap.GetNeighborsNoRoom(hex.GetComponent<Node>());
        foreach(Node node in AdjacentNodes)
        {
            if (node.GetComponent<Hex>().EntityToSpawn != null) { return false; }
            if (node.GetComponent<Door>() != null) { return false; }

            //TODO change this to make it so that it doesnt block a small hallway
            if (node.edge) { return false; }
        }
        return true;
    }

    void SetNexHexes(List<Hex> hexes)
    {
        foreach(Hex hex in hexes)
        {
            hex.GetComponent<Node>().Used = true;
            hex.GetComponent<HexWallAdjuster>().HideWall();
        }
    }

    void RemoveSharedEdges(EdgeHexes edge)
    {
        foreach(EdgeHexes edgeHex in EdgesAvailable)
        {
            foreach(Hex hex in edge.Edges)
            {
                if (edgeHex.Edges.Contains(hex)) { edgeHex.RemoveHex(hex); }
            }
        }

        int i = 0;
        while (i < EdgesAvailable.Count)
        {
            if (EdgesAvailable[i].Edges.Count <= 0) {EdgesAvailable.Remove(EdgesAvailable[i]);}
            else{i++;}
        }
    }

    void CreateDoorInBetweenRooms(Hex hex)
    {
        List<Node> nodesAdjacent = hexMap.GetNeighborsNoRoom(hex.GetComponent<Node>());
        foreach (Node node in nodesAdjacent)
        {
            if (node.GetComponent<Door>() != null) { return; }
        }

        if (hex.GetComponent<Door>() == null)
        {
            Debug.Log("Building Door");
            Door door = hex.gameObject.AddComponent<Door>();
            if (hex.GetComponent<Node>().RoomName.Count <= 1)
            {
                Debug.Log("Not In between rooms");
                return;
            }
            RoomSide roomSideTowards = GetRoomSideToBuildTowards(hex.GetComponent<HexAdjuster>());
            door.RoomNameToBuild = hex.GetComponent<Node>().RoomName[1];
            door.GetComponent<HexAdjuster>().ClearSides();
            hex.GetComponent<HexWallAdjuster>().DestroyAllWalls();
            if (roomSideTowards == RoomSide.Right) { door.myDoorLocation = Door.DoorLocation.Right; }
            else if (roomSideTowards == RoomSide.Left) { door.myDoorLocation = Door.DoorLocation.Left; }
            else if (roomSideTowards == RoomSide.Down) { door.myDoorLocation = Door.DoorLocation.Middle; }
            else { door.myDoorLocation = Door.DoorLocation.Middle; }
            door.BuildDoor();
            hex.GetComponent<HexAdjuster>().SetHexToFull();
        }
    }

    void BuildDoor(Hex hex, List<Hex> hexesInRoom, RoomSide roomSideTowards, string RoomName)
    {
        if (hex.GetComponent<Door>() == null)
        {
            Door door = hex.gameObject.AddComponent<Door>();
            door.RoomNameToBuild = RoomName;
            door.GetComponent<HexAdjuster>().ClearSides();
            hex.GetComponent<HexWallAdjuster>().DestroyAllWalls();
            if (roomSideTowards == RoomSide.Right) { door.myDoorLocation = Door.DoorLocation.Right; }
            else if (roomSideTowards == RoomSide.Left) { door.myDoorLocation = Door.DoorLocation.Left; }
            else if (roomSideTowards == RoomSide.Down) { door.myDoorLocation = Door.DoorLocation.Middle; }
            else { door.myDoorLocation = Door.DoorLocation.Middle; }
            hex.GetComponent<Node>().AddRoomName(RoomName);
            door.BuildDoor();
            if (!hex.GetComponent<Node>().isAvailable) { door.door.transform.parent.gameObject.SetActive(false); }
            hex.GetComponent<HexAdjuster>().SetHexToFull();
        }
    }

    bool IsAlreadySurrounded(HexAdjuster hex)
    {
        if (hex.LeftRoomSide != "" && hex.RightRoomSide != "") { return true; }
        else if (hex.UpRoomSide != "" && hex.DownRoomSide != "") { return true; }
        else if (hex.LeftRoomSide == "" && hex.RightRoomSide == "" && hex.UpRoomSide == "" && hex.DownRoomSide == "") { return true; }
        return false;
    }

    RoomSide GetRoomSideToBuildTowards(HexAdjuster hex)
    {
        if (hex.LeftRoomSide != "") { return RoomSide.Right; }
        else if (hex.UpRoomSide != "") { return RoomSide.Down; }
        else if (hex.RightRoomSide != "") { return RoomSide.Left; }
        else { return RoomSide.Top; }
    }

    List<Hex> GetNonEdgeHexes(List<Hex> hexes)
    {
        List<Hex> nonEdgeHexes = new List<Hex>();
        foreach (Hex hex in hexes)
        {
            if (!hex.GetComponent<Node>().edge && hex.GetComponent<Door>() == null) { nonEdgeHexes.Add(hex); }
        }
        return nonEdgeHexes;
    }

    List<Hex> GetEdgeHexes(List<Hex> hexes)
    {
        List<Hex> edgeHexes = new List<Hex>();
        foreach(Hex hex in hexes)
        {
            if (hex.GetComponent<Node>().edge) { edgeHexes.Add(hex); }
        }
        return edgeHexes;
    }

    List<Hex> BuildRoom(int q, int r, RoomSide directionBuilding)
    {
        int width = Random.Range(3, MaxLength);
        int height = Random.Range(3, MaxLength);
        if (directionBuilding == RoomSide.Left || directionBuilding == RoomSide.Right)
        {
            width = width % 2 != 1 ? width + 1 : width;
            height = height % 4 != 1 ? height + (4 - ((height - 1) % 4)) : height;
        }
        if (directionBuilding == RoomSide.Top || directionBuilding == RoomSide.Down)
        {
            height = height % 2 != 0 ? height + (height % 2) : height;
        }
        Node StartNode = hexMap.GetNode(q, r);
        string RoomName = ((char)((int)('A') + RoomIndex)).ToString();
        List<Hex> hexes = hexRoomBuilder.BuildRoomBySize(StartNode, height, width, RoomName, directionBuilding);
        if (hexes != null)
        {
            Rooms NewRoom = new Rooms();
            foreach (Hex hex in hexes) { NewRoom.AddHex(hex); }
            NewRoom.RoomName = RoomName;
            RoomsMade.Add(NewRoom);
        }
        return hexes;
    }

    void ShowHexSet(List<Hex> hexes, string Room)
    {
        foreach (Hex hex in hexes)
        {
            hex.GetComponent<Node>().Used = true;
            hex.GetComponent<HexAdjuster>().AddRoomShown(Room);
            hex.GetComponent<HexAdjuster>().RevealRoomEdge();
            hex.GetComponent<HexWallAdjuster>().ShowWall();
            if (hex.GetComponent<Door>() != null && hex.GetComponent<Door>().door != null)
            {
                hex.GetComponent<Door>().door.transform.parent.gameObject.SetActive(true);
            }
            //if (hex.GetComponent<HexAdjuster>().IsApartOfBothRooms(StartNode.RoomName)) { continue; }
           // else
         //   {
            hex.ShowHexEditor();
            hex.ShowHexEnd();
            hex.GetComponent<Node>().isAvailable = true;
            if (hex.EntityToSpawn != null)
            {
                hex.GenerateCharacter();
            }
           // }
        }
    }

    void SetStartNode(Node node, string RoomName)
    {
        node.GetComponent<HexAdjuster>().UpRoomSide = RoomName;
        node.SetRoomName(RoomName);
        node.GetComponent<HexAdjuster>().SetHexToFragment();
        node.GetComponent<HexAdjuster>().RotateHexToTopMiddle();
        node.GetComponent<HexWallAdjuster>().CreateHexWall(2, 0, RoomName);
        node.GetComponent<HexAdjuster>().RevealRoomEdge();
        node.GetComponent<HexWallAdjuster>().ShowWall();
        node.GetComponent<Hex>().ShowHexEditor();
        node.GetComponent<Hex>().ShowHexEnd();
        node.isAvailable = true;
        node.Shown = true;
    }
}

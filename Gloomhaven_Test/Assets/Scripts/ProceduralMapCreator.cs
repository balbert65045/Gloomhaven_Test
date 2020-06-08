using System.Collections;
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

    public int GoldAmount = 50;
    public int CurrentGoldAmount = 0;

    public int TreasureAmount = 7;
    public int CurrentTreasureAmount = 0;

    public int ChallengeRating = 15;
    public int CurrentChallengeRating = 0;
    public int MaxLength = 15;
    public GameObject ExplorationChestPrefab;
    public GameObject CombatChestPrefab;
    public List<GameObject> ObstaclePool = new List<GameObject>();
    public List<GameObject> EnemyPool = new List<GameObject>();
    public List<GameObject> PlayerCharacters = new List<GameObject>();

    HexMapController hexMap;
    HexRoomBuilder hexRoomBuilder;
    private int RoomIndex = 0;

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
    bool ShowMap = false;

    public void BuildNewMapAndReveal()
    {
        ShowMap = true;
        FindObjectOfType<HexMapBuilder>().DestroyMap();
        FindObjectOfType<HexMapBuilder>().BuildMap();
        BuildMap();
    }

    public void BuildNewMap()
    {
        ShowMap = false;
        FindObjectOfType<HexMapBuilder>().DestroyMap();
        FindObjectOfType<HexMapBuilder>().BuildMap();
        BuildMap();
    }

    public void BuildMapToStart(List<GameObject> Characters)
    {
        PlayerCharacters = Characters;
        ShowMap = false;
        FindObjectOfType<HexMapBuilder>().BuildMap();
        BuildMap();
    }

    public void BuildMap()
    {
        CurrentGoldAmount = 0;
        RoomIndex = 0;
        CurrentTreasureAmount = TreasureAmount;
        EdgesAvailable = new List<EdgeHexes>();
        RoomsMade = new List<Rooms>();

        CurrentChallengeRating = ChallengeRating;
        hexMap = FindObjectOfType<HexMapController>();
        hexMap.CreateTable();
        hexRoomBuilder = FindObjectOfType<HexRoomBuilder>();
        List<Hex> StartHexes = CreateStartRoom();
        CollectAndSortEdges(StartHexes, new List<Hex>());
        if (StartHexes.Count == 0) { return; }
        CreateNextRoom();
        PopulateRooms();
        //Create exit
        CreateExit();
    }

    void PopulateRooms()
    {
        int RoomsWithEnemiesInside = CurrentChallengeRating / 3;
        List<Rooms> RoomToBuildEnemies = new List<Rooms>();
        foreach(Rooms room in RoomsMade) { RoomToBuildEnemies.Add(room); }
        for (int i = 0; i < RoomsWithEnemiesInside; i++)
        {
            int RandomIndex = Random.Range(0, RoomToBuildEnemies.Count);
            AddEnemies(RoomToBuildEnemies[RandomIndex].RoomHexes);
            RoomToBuildEnemies.Remove(RoomToBuildEnemies[RandomIndex]);
        }

        foreach (Rooms room in RoomsMade) { AddObstaclesToRoom(room.RoomHexes); }

        int totalRooms = RoomsMade.Count;
        List<Rooms> RoomToBuildChests= new List<Rooms>();
        foreach (Rooms room in RoomsMade) { RoomToBuildChests.Add(room); }
        for (int i = 0; i < totalRooms; i++)
        {
            int RandomIndex = Random.Range(0, RoomToBuildChests.Count);
            AddChestsToRoom(RoomToBuildChests[RandomIndex].RoomHexes);
            RoomToBuildChests.Remove(RoomToBuildChests[RandomIndex]);
        }

        foreach (Rooms room in RoomsMade) { AddGoldToRooms(room.RoomHexes); }

        if (ShowMap)
        {
            // TO Show All Rooms
            foreach (Rooms room in RoomsMade)
            {
                ShowHexSet(room.RoomHexes, room.RoomName);
            }
        }
    }

    void AddGoldToRooms(List<Hex> hexes)
    {
        if (CurrentGoldAmount > GoldAmount) { return; }
        List<Hex> NonEdgeHexes = GetNonEdgeHexes(hexes);
        int MaximumGoldShouldPlace = 4;
        for (int i = 0; i < MaximumGoldShouldPlace; i++)
        {
            int RandomLocation = Random.Range(0, NonEdgeHexes.Count);
            Hex AttemptHex = NonEdgeHexes[RandomLocation];
            int flip = Random.Range(0, 2);
            if (flip == 1) {
                int goldPlacing = Random.Range(1, 7);
                CurrentGoldAmount += goldPlacing;
                AttemptHex.goldHolding = goldPlacing;
            }
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
        int maxLength = MaxLength;
        MaxLength = 6;
        List<Hex> hexes = BuildRoom(0, 0, RoomSide.Top);
        MaxLength = maxLength;
        if (hexes.Count == 0) { Debug.LogWarning("Room not made!!"); }
        string RoomName = ((char)((int)('A') + RoomIndex)).ToString();
        Node StartNode = hexMap.GetNode(0, 0);
        foreach(Hex hex in hexes) { hex.GetComponent<Node>().Shown = true; }
        AddPlayerCharactersToRoom(hexes);
        AddObstaclesToRoom(hexes);
        SetStartNode(StartNode, RoomName);
        hexes.Add(StartNode.GetComponent<Hex>());
        ShowHexSet(hexes, RoomName);
        RoomIndex++;
        return hexes;
    }

    void CreateExit()
    {
        if (EdgesAvailable.Count <= 0) {
            Debug.Log("No Room to build an exit");
            return;
        }
        int RandomEdgeIndex = Random.Range(0, EdgesAvailable.Count);
        EdgeHexes edge = EdgesAvailable[RandomEdgeIndex];
        int RandomHexIndex = Random.Range(0, edge.Edges.Count);
        Hex hex = edge.Edges[RandomHexIndex];
        EdgesAvailable.Remove(edge);
        if (IsAlreadySurrounded(hex.GetComponent<HexAdjuster>())) { CreateExit(); }
        else
        {
            RoomSide roomToBuildTowards = GetRoomSideToBuildTowards(hex.GetComponent<HexAdjuster>());
            Node node = hex.GetComponent<Node>();
            ExitHex exit = hex.gameObject.AddComponent<ExitHex>();
            node.GetComponent<HexAdjuster>().ClearSides();
            node.GetComponent<HexWallAdjuster>().DestroyAllWalls();
            if (roomToBuildTowards == RoomSide.Right) { exit.myExitLocation = ExitHex.ExitLocation.Right; }
            else if (roomToBuildTowards == RoomSide.Left) { exit.myExitLocation = ExitHex.ExitLocation.Left; }
            else if (roomToBuildTowards == RoomSide.Down) { exit.myExitLocation = ExitHex.ExitLocation.Middle; }
            else { exit.myExitLocation = ExitHex.ExitLocation.Middle; }
            exit.BuildExit();
            exit.exit.gameObject.SetActive(false); 
            hex.GetComponent<HexAdjuster>().SetHexToFull();
        }

    }

    void AddPlayerCharactersToRoom(List<Hex> hexes)
    {
        List<Hex> AvailableHexes = GetNonEdgeHexes(hexes);
        List<Hex> CharacterHexes = new List<Hex>();
        foreach(GameObject character in PlayerCharacters)
        {
            if (CharacterHexes.Count == 0)
            {
                int RandomIndex = Random.Range(0, AvailableHexes.Count);
                AvailableHexes[RandomIndex].EntityToSpawn = character.GetComponent<Entity>();
                CharacterHexes.Add(AvailableHexes[RandomIndex]);
            }
            else
            {
                for(int i = 0; i < CharacterHexes.Count; i++)
                {
                    List<Node> nodes = hexMap.GetNeighborsNoRoom(CharacterHexes[i].GetComponent<Node>());
                    bool characterSpawn = false;

                    foreach (Node node in nodes)
                    {
                        if (node.edge) { continue; }
                        if (!node.Used) { continue; }
                        if (node.GetComponent<Hex>().EntityToSpawn == null)
                        {
                            node.GetComponent<Hex>().EntityToSpawn = character.GetComponent<Entity>();
                            characterSpawn = true;
                            CharacterHexes.Add(node.GetComponent<Hex>());
                            break;
                        }
                    }
                    if (characterSpawn) { break; }
                }
            }
        }
    }

    List<GameObject> charactersAvailableForChest;
    void CreateNextRoom()
    {
        if (EdgesAvailable.Count <= 1) { return; }
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
                SetNexHexes(NewHexes);
                RoomIndex++;
                CollectAndSortEdges(NewHexes, edge.Edges);
            }
        }
        else
        {
            CreateDoorInBetweenRooms(hex);
            RemoveSharedEdges(edge);
        }
        CreateNextRoom();
    }

    void AddEnemies(List<Hex> hexes)
    {
        if (CurrentChallengeRating <= 0) { return; }
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
        CurrentChallengeRating -= RoomChallengeRating;
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

    List<GameObject> CharactersThatNeedAChest = new List<GameObject>();
    void AddChestsToRoom(List<Hex> hexes)
    {
        if (CurrentTreasureAmount <= 0) { return; }
        List<Hex> NonEdgeHexes = GetNonEdgeHexes(hexes);
        if (NonEdgeHexes.Count <= 0) { return; }
        Hex RandomHex = NonEdgeHexes[Random.Range(0, NonEdgeHexes.Count)];
        if (HexNotNextToOtherObstacleOrDoor(RandomHex))
        {
            CurrentTreasureAmount--;
            string chestCharacter = RepopulateAndSubtractCharacterFromList();
            if (Random.Range(0, 2) == 0) {
                RandomHex.chestFor = chestCharacter;
                RandomHex.EntityToSpawn = CombatChestPrefab.GetComponent<Entity>(); }
            else {
                RandomHex.chestFor = chestCharacter;
                RandomHex.EntityToSpawn = ExplorationChestPrefab.GetComponent<Entity>();
            }
        }
        else
        {
            Hex NewRandomHex = NonEdgeHexes[Random.Range(0, NonEdgeHexes.Count)];
            if (HexNotNextToOtherObstacleOrDoor(NewRandomHex))
            {
                CurrentTreasureAmount--;
                string chestCharacter = RepopulateAndSubtractCharacterFromList();
                if (Random.Range(0, 2) == 0) {
                    NewRandomHex.chestFor = chestCharacter;
                    NewRandomHex.EntityToSpawn = CombatChestPrefab.GetComponent<Entity>();
                }
                else {
                    NewRandomHex.chestFor = chestCharacter;
                    NewRandomHex.EntityToSpawn = ExplorationChestPrefab.GetComponent<Entity>();
                }
            }
        }
    }

    string RepopulateAndSubtractCharacterFromList()
    {
        if (CharactersThatNeedAChest.Count == 0)
        {
            foreach(GameObject character in PlayerCharacters)
            {
                CharactersThatNeedAChest.Add(character);
            }
        }

        int randomIndex = Random.Range(0, CharactersThatNeedAChest.Count);
        GameObject chestCharacter = CharactersThatNeedAChest[randomIndex];
        CharactersThatNeedAChest.RemoveAt(randomIndex);
        return chestCharacter.GetComponent<PlayerCharacter>().CharacterName;
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
            Door door = hex.gameObject.AddComponent<Door>();
            if (hex.GetComponent<Node>().RoomName.Count <= 1) { return; }
            RoomSide roomSideTowards = GetRoomSideToBuildTowards(hex.GetComponent<HexAdjuster>());
            door.RoomNameToBuild = hex.GetComponent<Node>().RoomName[1];
            door.GetComponent<HexAdjuster>().ClearSides();
            hex.GetComponent<HexWallAdjuster>().DestroyAllWalls();
            if (roomSideTowards == RoomSide.Right) { door.myDoorLocation = Door.DoorLocation.Right; }
            else if (roomSideTowards == RoomSide.Left) { door.myDoorLocation = Door.DoorLocation.Left; }
            else if (roomSideTowards == RoomSide.Down) { door.myDoorLocation = Door.DoorLocation.Middle; }
            else { door.myDoorLocation = Door.DoorLocation.Middle; }
            door.BuildDoor();
            if (!hex.GetComponent<Node>().isAvailable)
            {
                door.door.transform.parent.gameObject.SetActive(false);
            }
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
            if (!hex.GetComponent<Node>().isAvailable) {
                door.door.transform.parent.gameObject.SetActive(false);
            }
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
            foreach (Hex hex in hexes) { hex.GetComponent<Node>().Used = true; }
            if (RoomName != "A")
            {
                Rooms NewRoom = new Rooms();
                foreach (Hex hex in hexes) { NewRoom.AddHex(hex); }
                NewRoom.RoomName = RoomName;
                RoomsMade.Add(NewRoom);
            }
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
            hex.ShowHexEditor();
            hex.ShowHexEnd();
            hex.ShowMoney();
            hex.GetComponent<Node>().isAvailable = true;
            if (hex.EntityToSpawn != null)
            {
                hex.GenerateCharacter();
            }
        }
    }

    void SetStartNode(Node node, string RoomName)
    {
        node.GetComponent<HexAdjuster>().UpRoomSide = RoomName;
        node.SetRoomName(RoomName);
        node.GetComponent<HexAdjuster>().SetHexToFragment();
        node.GetComponent<HexAdjuster>().RotateHexToTopMiddle();
        node.GetComponent<HexWallAdjuster>().CreateHexWall(2, 0, RoomName);
        node.Shown = true;
    }
}

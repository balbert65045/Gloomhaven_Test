using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public enum DoorLocation
    {
        Right = 1,
        Left = 2,
        Middle = 3
    }
    public DoorLocation myDoorLocation;
    public GameObject DoorPrefab;


    public bool isOpen = false;

    public List<Hex> HexesInRoom;
    public List<Hex> hexesToOpenTo;

    public GameObject door;

    public RoomType RoomToBuild;
    public string RoomNameToBuild;

    public bool CanMoveOnDoor()
    {
        if (isOpen) { return true; }
        else
        {
            if (GetComponent<doorConnectionHex>() != null)
            {
                return true;
            }
        }
        return false;
    }

    public void BuildDoor()
    {
        HexMapController controller = FindObjectOfType<HexMapController>();
        controller.CreateTable();
        InteractionObjects parent = FindObjectOfType<InteractionObjects>();
        GameObject doorMade = Instantiate(DoorPrefab, parent.transform);
        switch (myDoorLocation)
        {
            case DoorLocation.Left:
                if (GetComponent<doorConnectionHex>() == null)
                {
                    doorConnectionHex connectionHex = gameObject.AddComponent<doorConnectionHex>();
                    connectionHex.door = this;
                }

                doorMade.transform.localPosition = transform.position + Vector3.right + (Vector3.up * .9f);
                Node leftNode = controller.GetNode(GetComponent<Node>().q + 1, (GetComponent<Node>().r));
                if (leftNode.GetComponent<doorConnectionHex>() == null)
                {
                    doorConnectionHex connectionHex = leftNode.gameObject.AddComponent<doorConnectionHex>();
                    connectionHex.door = this;
                }
                break;
            case DoorLocation.Right:
                if (GetComponent<doorConnectionHex>() == null)
                {
                    doorConnectionHex connectionHex = gameObject.AddComponent<doorConnectionHex>();
                    connectionHex.door = this;
                }

                doorMade.transform.localPosition = transform.position + Vector3.left + (Vector3.up * .9f);
                Node rightNode = controller.GetNode(GetComponent<Node>().q - 1, (GetComponent<Node>().r));
                if (rightNode.GetComponent<doorConnectionHex>() == null)
                {
                    doorConnectionHex connectionHex = rightNode.gameObject.AddComponent<doorConnectionHex>();
                    connectionHex.door = this;
                }
                break;
            case DoorLocation.Middle:
                doorMade.transform.localPosition = transform.position + (Vector3.up * .9f);
                doorMade.transform.rotation = Quaternion.Euler(Vector3.zero);
                List<Node> nodes = controller.GetRealNeighbors(GetComponent<Node>());
                foreach (Node node in nodes)
                {
                    if (node == null) { continue; }
                    if (node.isAvailable)
                    {
                        if (node.GetComponent<doorConnectionHex>() == null)
                        {
                            doorConnectionHex connectionHex = node.gameObject.AddComponent<doorConnectionHex>();
                            connectionHex.door = this;
                        }
                    }
                }
                break;
        }
        door = doorMade.GetComponent<DoorWall>().Door;
        door.GetComponent<DoorObject>().door = this;
        GetHexesInRoom();
    }

    public void GetHexesInRoom()
    {
        HexesInRoom.Clear();
        HexMapController controller = FindObjectOfType<HexMapController>();
        HexesInRoom = controller.GetAllHexesInThisRoom(GetComponent<Node>().RoomName[0], GetComponent<Node>());
    }

    public void BuildRoom()
    {
        HexRoomBuilder builder = FindObjectOfType<HexRoomBuilder>();
        hexesToOpenTo = builder.BuildRoom(RoomToBuild, GetComponent<Node>(), RoomNameToBuild);
        if (hexesToOpenTo != null) {
            ShowHexes();
            if (!GetComponent<Node>().RoomName.Contains(RoomNameToBuild))
            {
                GetComponent<Node>().RoomName.Add(RoomNameToBuild);
            }
        }
    }


    public void OpenHexes(string RoomComingFrom)
    {
        GetComponent<Node>().isAvailable = true;
        door.GetComponent<Animator>().SetTrigger("Open");
        if (RoomComingFrom == GetComponent<Node>().RoomName[0])
        {
            foreach (Hex hex in hexesToOpenTo)
            {
                //if (hex.GetComponent<Door>() != null && hex.GetComponent<doorConnectionHex>() == null)
                //{
                //    continue;
                //}
                hex.GetComponent<Node>().isAvailable = true;
            }
            foreach (Hex hex in hexesToOpenTo)
            {
                if (!hex.GetComponent<Node>().edge)
                {
                    hex.setUpHexes();
                }
            }
        }
        else
        {
            foreach (Hex hex in HexesInRoom)
            {
                if (hex.GetComponent<Door>() != null && hex.GetComponent<doorConnectionHex>() == null)
                {
                    continue;
                }
                hex.GetComponent<Node>().isAvailable = true;
            }
            foreach (Hex hex in HexesInRoom)
            {
                if (!hex.GetComponent<Node>().edge)
                {
                    hex.setUpHexes();
                }
            }
        }
        isOpen = true;
    }

    public void ShowHexes()
    {
        foreach (Hex hex in hexesToOpenTo)
        {
            hex.ShowHexEditor();
            hex.ShowHexEnd();
            hex.GetComponent<Node>().isAvailable = true; 
            if (hex.EntityToSpawn != null)
            {
                hex.GenerateCharacter();
            }
        }
    }

    public void HideHexes()
    {
        foreach (Hex hex in hexesToOpenTo)
        {
            hex.HideHexEditor();
            hex.GetComponent<Node>().isAvailable = false;
            if (hex.EntityHolding != null)
            {
                DestroyImmediate(hex.EntityHolding.gameObject);
            }
            hex.HideHexEnd();
        }
    }

    public void ShowHexesInRoom()
    {
        foreach (Hex hex in HexesInRoom)
        {
            hex.ShowHexEditor();
            hex.ShowHexEnd();
            hex.GetComponent<Node>().isAvailable = true;
            if (hex.EntityToSpawn != null)
            {
                hex.GenerateCharacter();
            }
        }
    }

    public void HideHexesInRoom()
    {
        foreach (Hex hex in HexesInRoom)
        {
            hex.HideHexEditor();
            hex.GetComponent<Node>().isAvailable = false;
            if (hex.EntityHolding != null)
            {
                DestroyImmediate(hex.EntityHolding.gameObject);
            }
            hex.HideHexEnd();
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxialHexController : MonoBehaviour {

    public Hashtable HexMap = new Hashtable();

    public Node[] GetNeighbors(Node node)
    {
        Node[] Neighbors = new Node[6];
        Neighbors[0] = (Node)HexMap[GetHexHash(node.X+1,node.Y)];
        Neighbors[1] = (Node)HexMap[GetHexHash(node.X + 1, node.Y-1)];
        Neighbors[2] = (Node)HexMap[GetHexHash(node.X, node.Y-1)];
        Neighbors[3] = (Node)HexMap[GetHexHash(node.X-1, node.Y)];
        Neighbors[4] = (Node)HexMap[GetHexHash(node.X - 1, node.Y+1)];
        Neighbors[5] = (Node)HexMap[GetHexHash(node.X, node.Y + 1)];
        return Neighbors;
    }

    public void AddHex(Node node)
    {
        HexMap.Add(GetHexHash(node.X, node.Y), node);
    }

    public string GetHexHash(int x, int y) { return x.ToString() + ","  + y.ToString(); }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public bool isOpen = false;
    public List<Hex> hexesToOpenTo;
    public GameObject door;

    public void OpenHexes()
    {
        door.GetComponent<Animator>().SetTrigger("Open");
        foreach (Hex hex in hexesToOpenTo)
        {
            hex.HexNode.isAvailable = true;
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
            hex.HideHexEnd();
            if (hex.EntityHolding != null)
            {
                DestroyImmediate(hex.EntityHolding.gameObject);
            }
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public bool isOpen = false;
    public List<Hex> hexesToOpenTo;
    public GameObject door;

    public void OpenHexes()
    {
        door.layer = 0;
        GetComponent<Node>().isAvailable = true;
        door.GetComponent<Animator>().SetTrigger("Open");
        foreach (Hex hex in hexesToOpenTo)
        {
            hex.GetComponent<Node>().isAvailable = true;
        }
        foreach (Hex hex in hexesToOpenTo)
        {
            if (!hex.GetComponent<Node>().edge)
            {
                hex.setUpHexes();
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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

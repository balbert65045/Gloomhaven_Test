using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour {

    public GameObject Door;
    public GameObject Stairs;

    public void ShowStairsAndDoor()
    {
        Door.SetActive(true);
        Stairs.SetActive(true);
    }

    public void HideStairsAndDoor()
    {
        Door.SetActive(false);
        Stairs.SetActive(false);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerActionButton : MonoBehaviour {

    public void allowOpenDoorAction()
    {
        GetComponentInChildren<Text>().text = "Open Door";
    }

    public void OpenDoor()
    {
        FindObjectOfType<PlayerController>().OpenDoor();
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

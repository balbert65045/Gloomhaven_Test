using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour {


    public void allowEndTurn()
    {
        GetComponent<Button>().interactable = true;
    }

    public void disableEndTurn()
    {
        GetComponent<Button>().interactable = false;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

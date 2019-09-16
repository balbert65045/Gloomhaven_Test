using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestPanel : MonoBehaviour {

    public GameObject Panel; 

    public void ActivePanel()
    {
        Panel.SetActive(true);
    }

    public void DeActivePanel()
    {
        Panel.SetActive(false);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

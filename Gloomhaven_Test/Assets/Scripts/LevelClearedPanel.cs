using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelClearedPanel : MonoBehaviour {

    public GameObject panel;

    // Use this for initialization
    void Start()
    {
        panel.SetActive(false);
    }

    public void TurnOnPanel()
    {
        panel.SetActive(true);
    }

    // Update is called once per frame
    void Update () {
		
	}
}

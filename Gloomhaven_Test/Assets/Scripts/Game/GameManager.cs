using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public bool DebugMap = false; 
    

    public void BeginGame(List<GameObject> PlayerCharacters)
    {
        FindObjectOfType<ProceduralMapCreator>().BuildMapToStart(PlayerCharacters);
        FindObjectOfType<HexMapController>().SetHexes();
        FindObjectOfType<PlayerController>().BeginGame();
    }

    public void StartGameWithMapMade()
    {
        FindObjectOfType<HexMapController>().SetHexes();
        FindObjectOfType<PlayerController>().BeginGame();
    }

    private void Start()
    {
        if (DebugMap) { StartGameWithMapMade(); }
    }

    // Update is called once per frame
    void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionController : MonoBehaviour {

    public int TurnsUntilCorruptionBegins = 20;
    public int currentCorruptionState;
    CorruptionCounter Counter;

    void Start () {
        Counter = FindObjectOfType<CorruptionCounter>();
        Counter.StartCounterAt(TurnsUntilCorruptionBegins);
        currentCorruptionState = TurnsUntilCorruptionBegins;
    }

    public void TurnPassed()
    {
        currentCorruptionState--;
        if (currentCorruptionState >= 0)
        {
            Counter.CorruptionChanged(currentCorruptionState);
        }
        else
        {
            DamageAllPlayers();
        }
    }

    void DamageAllPlayers()
    {
        PlayerCharacter[] characters = FindObjectsOfType<PlayerCharacter>();
        foreach(PlayerCharacter character in characters)
        {
            character.TakeTrueDamage(1);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

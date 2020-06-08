using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterDeck : MonoBehaviour {

    public CombatPlayerHand combatHand;
    public OutOfCombatHand outOfCombatHand;

    public void ShowCombatHand()
    {
        outOfCombatHand.HideHand();
        PlayerController PC = FindObjectOfType<PlayerController>();
        if (PC.SelectPlayerCharacter.InCombat())
        {
            combatHand.ShowHand();
        }
        else
        {
            combatHand.ShowHandTemp();
        }
    }

    public void ShowOutOfCombatHand()
    {
        combatHand.HideHand();
        PlayerController PC = FindObjectOfType<PlayerController>();
        if (!PC.SelectPlayerCharacter.InCombat())
        {
            outOfCombatHand.ShowHand();
        }
        else
        {
            outOfCombatHand.ShowHandTemp();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

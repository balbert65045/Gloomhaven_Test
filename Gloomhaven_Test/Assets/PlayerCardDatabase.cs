using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardDatabase : MonoBehaviour {

    public GameObject[] KightCards;
    public GameObject[] BarbarianCards;

    public GameObject SelectRandomCard(PlayerCharacter character)
    {
        switch (character.myType)
        {
            case PlayerCharacterType.Knight:
                return KightCards[0];
            case PlayerCharacterType.Barbarian:
                return BarbarianCards[0];
        }
        return null;
    }

    public GameObject FindCardInDatabase(PlayerCharacter character, Card card)
    {
        switch (character.myType)
        {
            case PlayerCharacterType.Knight:
                foreach (GameObject cardPrefab in KightCards)
                {
                    if (cardPrefab.GetComponent<Card>() == card) { return cardPrefab; }
                }
                break;
            case PlayerCharacterType.Barbarian:
                foreach (GameObject cardPrefab in BarbarianCards)
                {
                    if (cardPrefab.GetComponent<Card>() == card) { return cardPrefab; }
                }
                break;
        }
        return null;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardDatabase : MonoBehaviour {

    public GameObject[] KightCombatCards;
    public GameObject[] KightOutOfCombatCards;
    public GameObject[] BarbarianCombatCards;
    public GameObject[] BarbarianOutOfCombatCards;

    public GameObject SelectRandomCard(PlayerCharacter character, CardType CT)
    {
        switch (character.myType)
        {
            case PlayerCharacterType.Knight:
                switch (CT)
                {
                    case CardType.Combat:
                        return KightCombatCards[0];
                    case CardType.OutOfCombat:
                        return KightOutOfCombatCards[0];
                }
                break;
            case PlayerCharacterType.Barbarian:
                switch (CT)
                {
                    case CardType.Combat:
                        return BarbarianCombatCards[0];
                    case CardType.OutOfCombat:
                        return BarbarianOutOfCombatCards[0];
                }
                break;
        }
        return null;
    }

    public GameObject FindCardInDatabase(PlayerCharacter character, Card card)
    {
        switch (character.myType)
        {
            case PlayerCharacterType.Knight:
                if (card.GetComponent<CombatPlayerCard>() != null)
                {
                    foreach (GameObject cardPrefab in KightCombatCards)
                    {
                        if (cardPrefab.GetComponent<Card>() == card) { return cardPrefab; }
                    }
                }
                else if (card.GetComponent<OutOfCombatCard>() != null)
                {
                    foreach (GameObject cardPrefab in KightOutOfCombatCards)
                    {
                        if (cardPrefab.GetComponent<Card>() == card) { return cardPrefab; }
                    }
                }
                break;
            case PlayerCharacterType.Barbarian:
                if (card.GetComponent<CombatPlayerCard>() != null)
                {
                    foreach (GameObject cardPrefab in BarbarianCombatCards)
                    {
                        if (cardPrefab.GetComponent<Card>() == card) { return cardPrefab; }
                    }
                }
                else if (card.GetComponent<OutOfCombatCard>() != null)
                {
                    foreach (GameObject cardPrefab in BarbarianOutOfCombatCards)
                    {
                        if (cardPrefab.GetComponent<Card>() == card) { return cardPrefab; }
                    }
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

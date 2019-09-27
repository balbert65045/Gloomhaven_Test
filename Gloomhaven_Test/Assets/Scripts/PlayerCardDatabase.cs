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
        int randomIndex = 0;
        switch (character.myType)
        {
            case PlayerCharacterType.Knight:
                switch (CT)
                {
                    case CardType.Combat:
                        randomIndex = Random.Range(0, KightCombatCards.Length);
                        return KightCombatCards[randomIndex];
                    case CardType.OutOfCombat:
                        randomIndex = Random.Range(0, KightOutOfCombatCards.Length);
                        return KightOutOfCombatCards[randomIndex];
                }
                break;
            case PlayerCharacterType.Barbarian:
                switch (CT)
                {
                    case CardType.Combat:
                        randomIndex = Random.Range(0, BarbarianCombatCards.Length);
                        return BarbarianCombatCards[randomIndex];
                    case CardType.OutOfCombat:
                        randomIndex = Random.Range(0, BarbarianOutOfCombatCards.Length);
                        return BarbarianOutOfCombatCards[randomIndex];
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

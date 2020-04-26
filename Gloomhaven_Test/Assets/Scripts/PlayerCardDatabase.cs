using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardDatabase : MonoBehaviour {

    public GameObject[] ItemCards;
    public GameObject[] KightCombatCards;
    public GameObject[] KightOutOfCombatCards;
    public GameObject[] BarbarianCombatCards;
    public GameObject[] BarbarianOutOfCombatCards;
    public GameObject[] HuntressCombatCards;
    public GameObject[] HuntressOutOfCombatCards;
    public GameObject[] MageCombatCards;
    public GameObject[] MageOutOfCombatCards;

    private void Awake()
    {
        PlayerCardDatabase[] cardDatabases = FindObjectsOfType<PlayerCardDatabase>();
        if (cardDatabases.Length > 1) { Destroy(cardDatabases[1].gameObject); }
        DontDestroyOnLoad(this.gameObject);
    }

    public GameObject FindCardByID(Card card)
    {
        GameObject CardFound = null;
        switch (card.myType)
        {
            case PlayerCharacterType.Knight:
                if (card.GetComponent<CombatPlayerCard>() != null)
                {
                    CardFound = CardInSubSectionByID(KightCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                else if (card.GetComponent<OutOfCombatCard>() != null)
                {
                    CardFound = CardInSubSectionByID(KightOutOfCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                break;
            case PlayerCharacterType.Barbarian:
                if (card.GetComponent<CombatPlayerCard>() != null)
                {
                    CardFound = CardInSubSectionByID(BarbarianCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                else if (card.GetComponent<OutOfCombatCard>() != null)
                {
                    CardFound = CardInSubSectionByID(BarbarianOutOfCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                break;
            case PlayerCharacterType.Crossbow:
                if (card.GetComponent<CombatPlayerCard>() != null)
                {
                    CardFound = CardInSubSectionByID(HuntressCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                else if (card.GetComponent<OutOfCombatCard>() != null)
                {
                    CardFound = CardInSubSectionByID(HuntressOutOfCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                break;
            case PlayerCharacterType.Mage:
                if (card.GetComponent<CombatPlayerCard>() != null)
                {
                    CardFound = CardInSubSectionByID(MageCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                else if (card.GetComponent<OutOfCombatCard>() != null)
                {
                    CardFound = CardInSubSectionByID(MageOutOfCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                break;
        }
        return FindItemCardByID(card);
    }

    GameObject CardInSubSectionByID(GameObject[] Subsection, Card card)
    {
        foreach (GameObject cardPrefab in Subsection)
        {
            if (cardPrefab.GetComponent<Card>().ID == card.ID) { return cardPrefab; }
        }
        return null;
    }

    GameObject FindItemCardByID(Card card)
    {
        foreach (GameObject cardPrefab in ItemCards)
        {
            if (cardPrefab.GetComponent<Card>().ID == card.ID) { return cardPrefab; }
        }
        return null;
    }


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
            case PlayerCharacterType.Crossbow:
                switch (CT)
                {
                    case CardType.Combat:
                        randomIndex = Random.Range(0, HuntressCombatCards.Length);
                        return HuntressCombatCards[randomIndex];
                    case CardType.OutOfCombat:
                        randomIndex = Random.Range(0, HuntressOutOfCombatCards.Length);
                        return HuntressOutOfCombatCards[randomIndex];
                }
                break;
            case PlayerCharacterType.Mage:
                switch (CT)
                {
                    case CardType.Combat:
                        randomIndex = Random.Range(0, HuntressCombatCards.Length);
                        return MageCombatCards[randomIndex];
                    case CardType.OutOfCombat:
                        randomIndex = Random.Range(0, HuntressOutOfCombatCards.Length);
                        return MageOutOfCombatCards[randomIndex];
                }
                break;
        }
        return null;
    }

    GameObject FindItemCard(Card card)
    {
        foreach (GameObject cardPrefab in ItemCards)
        {
            if (cardPrefab.GetComponent<Card>() == card) { return cardPrefab; }
        }
        return null;
    }


    public GameObject FindCardInDatabase(Card card)
    {
        GameObject CardFound = null;
        switch (card.myType)
        {
            case PlayerCharacterType.Knight:
                if (card.GetComponent<CombatPlayerCard>() != null)
                {
                    CardFound = CardInSubSection(KightCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                else if (card.GetComponent<OutOfCombatCard>() != null)
                {
                    CardFound = CardInSubSection(KightOutOfCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                break;
            case PlayerCharacterType.Barbarian:
                if (card.GetComponent<CombatPlayerCard>() != null)
                {
                    CardFound = CardInSubSection(BarbarianCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                else if (card.GetComponent<OutOfCombatCard>() != null)
                {
                    CardFound = CardInSubSection(BarbarianOutOfCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                break;
            case PlayerCharacterType.Crossbow:
                if (card.GetComponent<CombatPlayerCard>() != null)
                {
                    CardFound = CardInSubSection(HuntressCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                else if (card.GetComponent<OutOfCombatCard>() != null)
                {
                    CardFound = CardInSubSection(HuntressOutOfCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                break;
            case PlayerCharacterType.Mage:
                if (card.GetComponent<CombatPlayerCard>() != null)
                {
                    CardFound = CardInSubSection(MageCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                else if (card.GetComponent<OutOfCombatCard>() != null)
                {
                    CardFound = CardInSubSection(MageOutOfCombatCards, card);
                    if (CardFound != null) { return CardFound; }
                }
                break;
        }
        return FindItemCard(card);
    }

    GameObject CardInSubSection(GameObject[] Subsection, Card card)
    {
        foreach (GameObject cardPrefab in Subsection)
        {
            if (cardPrefab.GetComponent<Card>() == card) { return cardPrefab; }
        }
        return null;
    }
}

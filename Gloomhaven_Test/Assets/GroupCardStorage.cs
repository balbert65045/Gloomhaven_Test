using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterCardStorage
{
    public string CharacterName;
    public List<GameObject> CombatCardsObtained = new List<GameObject>();
    public List<GameObject> CombatCardsHolding = new List<GameObject>();
    public List<GameObject> ExplorationCardsObtained = new List<GameObject>();
    public List<GameObject> ExplorationCardsHolding = new List<GameObject>();

    public void AddCombatCardObtained(GameObject Card) { CombatCardsObtained.Add(Card); }
    public void AddCombatCardHolding(GameObject Card) { CombatCardsHolding.Add(Card); }
    public void ReplaceCombatCard(GameObject CardInHand, GameObject CardOutOfHand)
    {
        CombatCardsHolding.Add(CardInHand);
        CombatCardsHolding.Remove(CardOutOfHand);
        CombatCardsObtained.Add(CardOutOfHand);
    }

    public void AddExplorationCardObtained(GameObject Card) { ExplorationCardsObtained.Add(Card); }
    public void AddExplorationCardHolding(GameObject Card) { ExplorationCardsHolding.Add(Card); }
    public void ReplaceExplorationCard(GameObject CardInHand, GameObject CardOutOfHand)
    {
        ExplorationCardsHolding.Add(CardInHand);
        ExplorationCardsHolding.Remove(CardOutOfHand);
        ExplorationCardsObtained.Add(CardOutOfHand);
    }

    public void TakeOutOfHandIntoStorage(GameObject Card, string type)
    {
        switch (type)
        {
            case "Combat":
                CombatCardsHolding.Remove(Card);
                CombatCardsObtained.Add(Card);
                break;
            case "Exploration":
                ExplorationCardsHolding.Remove(Card);
                ExplorationCardsObtained.Add(Card);
                break;
        }
    }

    public void TakeOutOfStorageIntoHand(GameObject Card, string type)
    {
        switch (type)
        {
            case "Combat":
                CombatCardsHolding.Add(Card);
                CombatCardsObtained.Remove(Card);
                break;
            case "Exploration":
                ExplorationCardsHolding.Add(Card);
                ExplorationCardsObtained.Remove(Card);
                break;
        }
    }

}

public class GroupCardStorage : MonoBehaviour {

    public CharacterCardStorage[] MyGroupCardStorage = new CharacterCardStorage[4];
    int index = 0;

    public void CreateCharacterStorage(string name)
    {
        if (index < 4)
        {
            CharacterCardStorage CCS = new CharacterCardStorage();
            CCS.CharacterName = name;
            MyGroupCardStorage[index] = CCS;
            index++;
        }
    }

    CharacterCardStorage GetStorageFromName(string name)
    {
        for (int i = 0; i < 4; i++)
        {
            if (MyGroupCardStorage[i].CharacterName == name)
            {
                return MyGroupCardStorage[i];
            }
        }
        return null;
    }

    public void AddCardToHandFromObtained(string name, GameObject card, string type)
    {
        CharacterCardStorage myStorage = GetStorageFromName(name);
        myStorage.TakeOutOfStorageIntoHand(card, type);
    }

    public void RemoveCardFromHandToObtained(string name, GameObject card, string type)
    {
        CharacterCardStorage myStorage = GetStorageFromName(name);
        myStorage.TakeOutOfHandIntoStorage(card, type);
    }

    public List<GameObject> GetAllCombatCardsObtained(string name)
    {
        for (int i = 0; i < 4; i++)
        {
            if (MyGroupCardStorage[i].CharacterName == name)
            {
                return MyGroupCardStorage[i].CombatCardsObtained;
            }
        }
        return null;
    }
    public List<GameObject> GetAllCombatCardsInHand(string name)
    {
        for (int i = 0; i < 4; i++)
        {
            if (MyGroupCardStorage[i].CharacterName == name)
            {
                return MyGroupCardStorage[i].CombatCardsHolding;
            }
        }
        return null;
    }

    public List<GameObject> GetAllExplorationCardsObtained(string name)
    {
        for (int i = 0; i < 4; i++)
        {
            if (MyGroupCardStorage[i].CharacterName == name)
            {
                return MyGroupCardStorage[i].ExplorationCardsObtained;
            }
        }
        return null;
    }
    public List<GameObject> GetAllExplorationCardsInHand(string name)
    {
        for (int i = 0; i < 4; i++)
        {
            if (MyGroupCardStorage[i].CharacterName == name)
            {
                return MyGroupCardStorage[i].ExplorationCardsHolding;
            }
        }
        return null;
    }

    public void AddCardStored(GameObject card, string CharacterName)
    {
        if (card.GetComponent<CombatPlayerCard>() != null) { FindCardStorageFromName(CharacterName).AddCombatCardObtained(card); }
        else { FindCardStorageFromName(CharacterName).AddExplorationCardObtained(card); }
    }

    public void AddCardHolding(GameObject card, string CharacterName)
    {
        if (card.GetComponent<CombatPlayerCard>() != null) { FindCardStorageFromName(CharacterName).AddCombatCardHolding(card); }
        else { FindCardStorageFromName(CharacterName).AddExplorationCardHolding(card); }
    }

    public void ReplaceCard(GameObject cardToBeInHand, GameObject cardToBeOutOfHand, string CharacterName)
    {
        if (cardToBeInHand.GetComponent<CombatPlayerCard>() != null) { FindCardStorageFromName(CharacterName).ReplaceCombatCard(cardToBeInHand, cardToBeOutOfHand); }
        else { FindCardStorageFromName(CharacterName).ReplaceExplorationCard(cardToBeInHand, cardToBeOutOfHand); }
    }

    CharacterCardStorage FindCardStorageFromName(string name)
    {
        CharacterCardStorage TargetCCS = null;
        foreach (CharacterCardStorage CCS in MyGroupCardStorage)
        {
            if (CCS.CharacterName == name) { TargetCCS = CCS; }
        }
        return TargetCCS;
    }

    void Start () {
        index = 0;
    }
}

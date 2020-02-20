using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestPanel : MonoBehaviour {

    public GameObject Panel;
    public GameObject CardPosition;

    public Button AddButton;
    public Button ReplaceButton;
    public Button StoreButton;

    private PlayerCharacter CharacterOpeningChest;
    private GameObject cardFound;
    private CardChest chestOpened;

    private Card cardSelectedForReplace;

    public Card ActivePanel(PlayerCharacter Character, GameObject cardPrefab, CardChest chest)
    {
        chestOpened = chest;
        CharacterOpeningChest = Character;
        if (Character.OutOfActions()) { Character.GetMyOutOfCombatHand().RefeshActions(); }
        cardFound = cardPrefab;
        Panel.SetActive(true);
        GameObject card = Instantiate(cardPrefab, CardPosition.transform);
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = new Vector3(.87f, .87f, 2.18f);

        if (GetCorrectHand().CardsHolding.Count >= GetCorrectHand().handSize) { AddButton.interactable = false; }
        ReplaceButton.interactable = false;
        return card.GetComponent<Card>();
    }

    public void DeActivePanel()
    {
        Panel.SetActive(false);
    }

    public void CardSelected(Card card)
    {
        if (card != cardSelectedForReplace)
        {
            cardSelectedForReplace = card;
            ReplaceButton.interactable = true;
        }
        else
        {
            cardSelectedForReplace = null;
            ReplaceButton.interactable = false;
        }
    }

    public void Add()
    {
        Destroy(CardPosition.GetComponentInChildren<Card>().gameObject);
        GetCorrectHand().AddCard(cardFound);
        CharacterOpeningChest.AddCardToBeStored(cardFound.name);
        DoneOpeningChest();
    }

    public void Replace()
    {
        Destroy(CardPosition.GetComponentInChildren<Card>().gameObject);
        GetCorrectHand().ReplaceCard(cardFound, cardSelectedForReplace.gameObject);
        CharacterOpeningChest.AddCardToBeStored(cardFound.name);
        DoneOpeningChest();
    }

    public void Store()
    {
        Destroy(CardPosition.GetComponentInChildren<Card>().gameObject);
        CharacterOpeningChest.AddCardToBeStored(cardFound.name);
        DoneOpeningChest();
    }

    void DoneOpeningChest()
    {
        Panel.SetActive(false);
        if (CharacterOpeningChest.OutOfActions()) { CharacterOpeningChest.GetMyOutOfCombatHand().ActionsUsedForHand(); }
        CharacterOpeningChest.Selected();
        FindObjectOfType<PlayerController>().AllowEndTurn();
        FindObjectOfType<PlayerController>().ReturnToNormal();
    }

    Hand GetCorrectHand()
    {
        if (cardFound.GetComponent<CombatPlayerCard>() != null)
        {
            return CharacterOpeningChest.GetMyCombatHand();
        }
        else if (cardFound.GetComponent<OutOfCombatCard>() != null)
        {
            return CharacterOpeningChest.GetMyOutOfCombatHand();
        }
        return null;
    }
}

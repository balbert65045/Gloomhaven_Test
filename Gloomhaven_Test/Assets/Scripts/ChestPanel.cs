using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestPanel : MonoBehaviour {

    public GameObject SlectionPanel;

    public GameObject Panel;
    public GameObject CardPosition1;
    public GameObject CardPosition2;
    public GameObject CardPosition3;

    public Button AddButton;
    public Button ReplaceButton;
    public Button StoreButton;

    private PlayerCharacter CharacterOpeningChest;
    private GameObject cardFound1;
    private GameObject cardFound2;
    private GameObject cardFound3;

    private CardChest chestOpened;

    private Card cardSelectedForReplace;

    public List<Card> ActivePanel(PlayerCharacter Character, GameObject cardPrefab1, GameObject cardPrefab2, GameObject cardPrefab3, CardChest chest)
    {
        chestOpened = chest;
        CharacterOpeningChest = Character;
        if (Character.OutOfActions()) { Character.GetMyOutOfCombatHand().RefeshActions(); }
        cardFound1 = cardPrefab1;
        cardFound2 = cardPrefab2;
        cardFound3 = cardPrefab3;
        Panel.SetActive(true);
        GameObject card1 = Instantiate(cardPrefab1, CardPosition1.transform);
        GameObject card2 = Instantiate(cardPrefab2, CardPosition2.transform);
        GameObject card3 = Instantiate(cardPrefab3, CardPosition3.transform);
        card1.AddComponent<ChestCard>().SlectionPanel = SlectionPanel;
        card1.transform.localPosition = Vector3.zero;
        card1.transform.localScale = new Vector3(.87f, .87f, 2.18f);
        card2.AddComponent<ChestCard>().SlectionPanel = SlectionPanel;
        card2.transform.localPosition = Vector3.zero;
        card2.transform.localScale = new Vector3(.87f, .87f, 2.18f);
        card3.AddComponent<ChestCard>().SlectionPanel = SlectionPanel;
        card3.transform.localPosition = Vector3.zero;
        card3.transform.localScale = new Vector3(.87f, .87f, 2.18f);

        List<Card> cards = new List<Card>{};
        cards.Add(card1.GetComponent<Card>());
        cards.Add(card2.GetComponent<Card>());
        cards.Add(card3.GetComponent<Card>());

        if (GetCorrectHand().CardsHolding.Count >= GetCorrectHand().handSize) { AddButton.interactable = false; }
        ReplaceButton.interactable = false;
        return cards;
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

    public void SelectCard()
    {

    }

    public void Add()
    {
        //Destroy(CardPosition.GetComponentInChildren<Card>().gameObject);
        //GetCorrectHand().AddCard(cardFound);
        //CharacterOpeningChest.AddCardToHand(cardFound);
        //DoneOpeningChest();
    }

    public void Replace()
    {
        //Destroy(CardPosition.GetComponentInChildren<Card>().gameObject);
        //GetCorrectHand().ReplaceCard(cardFound, cardSelectedForReplace.gameObject);
        //GameObject CardReplacing = FindObjectOfType<PlayerCardDatabase>().FindCardInDatabase(cardSelectedForReplace);
        //CharacterOpeningChest.ReplacingCardInHand(cardFound, CardReplacing);
        //DoneOpeningChest();
    }

    public void Store()
    {
        //Destroy(CardPosition.GetComponentInChildren<Card>().gameObject);
        //CharacterOpeningChest.AddCardToBeStored(cardFound);
        //DoneOpeningChest();
    }

    void DoneOpeningChest()
    {
        Destroy(CardPosition1.GetComponentInChildren<Card>().gameObject);
        Destroy(CardPosition2.GetComponentInChildren<Card>().gameObject);
        Destroy(CardPosition3.GetComponentInChildren<Card>().gameObject);
        Panel.SetActive(false);
        if (CharacterOpeningChest.OutOfActions()) { CharacterOpeningChest.GetMyOutOfCombatHand().ActionsUsedForHand(); }
        CharacterOpeningChest.Selected();
        FindObjectOfType<PlayerController>().AllowEndTurn();
        FindObjectOfType<PlayerController>().ReturnToNormal();
    }

    Hand GetCorrectHand()
    {
        if (cardFound1.GetComponent<CombatPlayerCard>() != null)
        {
            return CharacterOpeningChest.GetMyCombatHand();
        }
        else if (cardFound1.GetComponent<OutOfCombatCard>() != null)
        {
            return CharacterOpeningChest.GetMyOutOfCombatHand();
        }
        return null;
    }
}

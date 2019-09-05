using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatPlayerHand : Hand {

    public OutOfCombatHand outOfCombatHand;
    public CombatPlayerCard SelectedPlayerCard;
    public GameObject Hand;
    public Button ShortRestButton;

    private PlayerController playerController;

    public List<CombatPlayerCardButton> AllCards = new List<CombatPlayerCardButton>();
    public List<CombatPlayerCardButton> myCardsInHand;

    public CombatPlayerCardButton selectedCardLinkedButton;
    public CombatPlayerCard getSelectedCard()
    {
        return SelectedPlayerCard;
    }

    public int GetTotalCardsInHand()
    {
        return myCardsInHand.Count;
    }

    public override void SelectCard(Card card)
    {
        if (card == null) { Debug.LogWarning("Null card was chosen make sure button is wired properly"); }
        SelectedPlayerCard = (CombatPlayerCard)card;
        CombatPlayerCardButton[] cardButtons = GetComponentsInChildren<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton button in cardButtons)
        {
            if (button.getMyCard() != SelectedPlayerCard) { button.Unhighlight(); }
            else { selectedCardLinkedButton = button; }
        }
        ShowPotential(SelectedPlayerCard);
        playerController.SelectCard();
    }

    public void ShowPotential(CombatPlayerCard card)
    {
        Character myCharacter = playerController.SelectPlayerCharacter;
        Action[] actions = card.CardAbility.Actions;
        FindObjectOfType<MyActionBoard>().showActions(actions, myCharacter);
    }

    public void HidePotential()
    {
        FindObjectOfType<MyActionBoard>().hideActions();
    }

    public void HideHand()
    {
        Hand.SetActive(false);
        HidePotential();
        FindObjectOfType<MyActionBoard>().HidePanel();
    }

    public bool HasLostOrDiscardCard()
    {
        CombatPlayerCardButton[] outOfCombatCards = GetComponentsInChildren<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton cardButton in outOfCombatCards)
        {
            if (cardButton.Discarded || cardButton.Lost) { return true; }
        }
        return false;
    }

    bool ShortRestReady()
    {
        CombatPlayerCardButton[] outOfCombatCards = GetComponentsInChildren<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton cardButton in outOfCombatCards)
        {
            if (cardButton.Discarded) { return true; }
        }
        return false;
    }

    public void ShowHand()
    {
        Hand.SetActive(true);
        FindObjectOfType<MyActionBoard>().ShowPanel();
        CombatPlayerCardButton[] outOfCombatCards = GetComponentsInChildren<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton cardButton in outOfCombatCards)
        {
            if (!cardButton.Discarded && !cardButton.Lost) {
                cardButton.GetComponent<Button>().interactable = true;
            }
        }
        ShortRestButton.interactable = ShortRestReady();
    }

    public void ShowHandTemp()
    {
        Hand.SetActive(true);
        CombatPlayerCardButton[] outOfCombatCards = GetComponentsInChildren<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton cardButton in outOfCombatCards)
        {
            cardButton.GetComponent<Button>().interactable = false;
        }
        ShortRestButton.interactable = false;
    }

    public void discardSelectedCard()
    {
        selectedCardLinkedButton.Unhighlight();
        if (!selectedCardLinkedButton.basicAttack) {
            if (SelectedPlayerCard.LostAbilityUsed) {
                selectedCardLinkedButton.LoseCard();
            }
            else
            {
                ShortRestButton.interactable = true;
                selectedCardLinkedButton.DiscardCard();
            }
            myCardsInHand.Remove(selectedCardLinkedButton);
            outOfCombatHand.allowLongRest();
        }
        SelectedPlayerCard.transform.SetParent(selectedCardLinkedButton.transform);
        SelectedPlayerCard = null;
    }

    public void ShortRest()
    {
        ShortRestButton.interactable = false;
        CombatPlayerCardButton[] cardButtons = GetComponentsInChildren<CombatPlayerCardButton>();
        List<CombatPlayerCardButton> discardedCards = new List<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton cardButton in cardButtons)
        {
            if (cardButton.Discarded) { discardedCards.Add(cardButton); }
        }

        int randomCardIndex = Random.Range(0, discardedCards.Count);
        CombatPlayerCardButton cardToLose = discardedCards[randomCardIndex];
        cardToLose.LoseCard();
        discardedCards.Remove(cardToLose);
        foreach (CombatPlayerCardButton cardButton in discardedCards)
        {
            cardButton.putBackInHand();
            myCardsInHand.Add(cardButton);
        }
        playerController.SetHandSize(myCardsInHand.Count - 1);
    }

    public void LongRest()
    {
        foreach(CombatPlayerCardButton cardButton in AllCards)
        {
            if (cardButton.Lost || cardButton.Discarded)
            {
                cardButton.putBackInHand();
                myCardsInHand.Add(cardButton);
            }
        }
        playerController.SetHandSize(myCardsInHand.Count - 1);
    }

    //Change this to better code for this functunallity
    public bool LoseRandomCard()
    {
        List<CombatPlayerCardButton> cardsInHand = new List<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton cardButton in myCardsInHand)
        {
            if (!cardButton.Discarded && !cardButton.Lost  && !cardButton.basicAttack && selectedCardLinkedButton != cardButton) { cardsInHand.Add(cardButton); }
        }
        if (cardsInHand.Count <= 0) { return false; }

        int randomCardIndex = Random.Range(0, cardsInHand.Count);
        CombatPlayerCardButton cardToLose = cardsInHand[randomCardIndex];
        cardToLose.LoseCard();
        myCardsInHand.Remove(cardToLose);
        outOfCombatHand.allowLongRest();
        return true;
    }

    public void showSelectedCard()
    {
        FindObjectOfType<MyActionBoard>().ShowPanel();
        ShowPotential(SelectedPlayerCard);
        Vector3 currentScale = SelectedPlayerCard.transform.localScale;
        PlayArea showArea = FindObjectOfType<PlayArea>();
        SelectedPlayerCard.transform.SetParent(showArea.transform);
        SelectedPlayerCard.transform.localPosition = Vector3.zero;
        SelectedPlayerCard.transform.localScale = currentScale;
        SelectedPlayerCard.gameObject.SetActive(true);
    }

    public void unShowAnyCards()
    {
        CombatPlayerCardButton[] cardButtons = GetComponentsInChildren<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton button in cardButtons)
        {
            button.unShowCard();
        }
    }

    public void HideSelectedCard()
    {
        if (SelectedPlayerCard != null)
        {
            HidePotential();
            FindObjectOfType<MyActionBoard>().HidePanel();
            SelectedPlayerCard.CardAbility.UnHighlightAbility();
            SelectedPlayerCard.gameObject.SetActive(false);
        }
    }

    // Use this for initialization
    void Start () {
        playerController = FindObjectOfType<PlayerController>();
    }

    public int CheckCardsInHand()
    {
        myCardsInHand.AddRange(GetComponentsInChildren<CombatPlayerCardButton>());
        return (myCardsInHand.Count - 1);
    }
	
}

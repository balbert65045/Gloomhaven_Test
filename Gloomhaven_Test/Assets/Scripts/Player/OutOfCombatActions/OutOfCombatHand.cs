using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutOfCombatHand : Hand {

    public GameObject Hand;
    public Button LongRestButton;
    public CombatPlayerHand combatHand;
    OutOfCombatCard myCard = null;
    OutOfCombatCardButton linkedButton = null;

    public void HideHand()
    {
        OutOfCombatCardButton[] outOfCombatCards = GetComponentsInChildren<OutOfCombatCardButton>();
        foreach(OutOfCombatCardButton cardButton in outOfCombatCards)
        {
            cardButton.unShowCard();
        }
        Hand.SetActive(false);
    }

    public void unShowAnyCards()
    {
        OutOfCombatCardButton[] outOfCombatCards = GetComponentsInChildren<OutOfCombatCardButton>();
        foreach (OutOfCombatCardButton cardButton in outOfCombatCards)
        {
            cardButton.unShowCard();
        }
    }

    bool LongRestReady()
    {
        OutOfCombatCardButton[] outOfCombatCards = GetComponentsInChildren<OutOfCombatCardButton>();
        foreach (OutOfCombatCardButton cardButton in outOfCombatCards)
        {
            if (cardButton.Discarded) { return true; }
        }
        return combatHand.HasLostOrDiscardCard();
    }

    public void ShowHand()
    {
        Hand.SetActive(true);
        OutOfCombatCardButton[] outOfCombatCards = GetComponentsInChildren<OutOfCombatCardButton>();
        foreach (OutOfCombatCardButton cardButton in outOfCombatCards)
        {
            if (!cardButton.Discarded && !cardButton.Lost) { cardButton.GetComponent<Button>().interactable = true; }
        }
    }

    public void ShowHandTemp()
    {
        Hand.SetActive(true);
        OutOfCombatCardButton[] outOfCombatCards = GetComponentsInChildren<OutOfCombatCardButton>();
        foreach (OutOfCombatCardButton cardButton in outOfCombatCards)
        {
            cardButton.GetComponent<Button>().interactable = false;
        }
        LongRestButton.interactable = false;
    }

    public OutOfCombatCard GetSelectecdCard()
    {
        return myCard;
    }

    public void UnSelectCard()
    { 
        if (myCard != null)
        {
            linkedButton.UnHighlight();
            linkedButton.unShowCard();
            myCard = null;
            linkedButton = null;
        }
    }

    public void DiscardSelectedCard()
    {
        if (myCard != null)
        {
            linkedButton.DiscardCard();
            linkedButton.unShowCard();
            myCard = null;
            linkedButton = null;
            allowLongRest();
        }
    }

    public override void SelectCard(Card card)
    {
        if (myCard != null)
        {
            linkedButton.unShowCard();
        }
        myCard = (OutOfCombatCard)card;

        OutOfCombatCardButton[] cardButtons = GetComponentsInChildren<OutOfCombatCardButton>();
        foreach (OutOfCombatCardButton cardButton in cardButtons)
        {
            if (cardButton.myCard != myCard ) { cardButton.UnHighlight(); }
            else { linkedButton = cardButton; }
        }
        FindObjectOfType<PlayerController>().SelectCard();
    }

    public void allowLongRest()
    {
        LongRestButton.interactable = true;
    }

    public void LongRest()
    {
        LongRestButton.interactable = false;
        OutOfCombatCardButton[] cardButtons = GetComponentsInChildren<OutOfCombatCardButton>();
        List<OutOfCombatCardButton> discardedCards = new List<OutOfCombatCardButton>();
        foreach (OutOfCombatCardButton cardButton in cardButtons)
        {
            if (cardButton.Discarded) { discardedCards.Add(cardButton); }
        }
        if (discardedCards.Count != 0)
        {
            int randomCardIndex = Random.Range(0, discardedCards.Count);
            OutOfCombatCardButton cardToLose = discardedCards[randomCardIndex];
            cardToLose.LoseCard();
            discardedCards.Remove(cardToLose);
            foreach (OutOfCombatCardButton cardButton in discardedCards)
            {
                cardButton.putBackInHand();
            }
        }
        else
        {
            List<OutOfCombatCardButton> cardsAvailable = new List<OutOfCombatCardButton>();
            foreach (OutOfCombatCardButton cardButton in cardButtons)
            {
                if (!cardButton.Lost) { cardsAvailable.Add(cardButton); }
            }
            int randomCardIndex = Random.Range(0, cardsAvailable.Count);
            OutOfCombatCardButton cardToLose = cardsAvailable[randomCardIndex];
            cardToLose.LoseCard();
        }
        combatHand.LongRest();
    }
}

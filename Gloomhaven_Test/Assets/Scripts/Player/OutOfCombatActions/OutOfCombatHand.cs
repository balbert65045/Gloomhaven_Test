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

    bool AllActionsUsed = false;

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
        return false;
    }

    public void ShowHand()
    {
        Hand.SetActive(true);
        if (AllActionsUsed) { return; }
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

    public void ActionsUsedForHand()
    {
        AllActionsUsed = true;
        ShowHandTemp();
    }

    public void RefeshActions()
    {
        AllActionsUsed = false;
        OutOfCombatCardButton[] outOfCombatCards = GetComponentsInChildren<OutOfCombatCardButton>();
        foreach (OutOfCombatCardButton cardButton in outOfCombatCards)
        {
            if (!cardButton.Discarded && !cardButton.Lost) { cardButton.GetComponent<Button>().interactable = true; }
        }
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
            LoseCard(myCard);
            linkedButton.unShowCard();
            myCard = null;
            linkedButton = null;
        }
    }

    public override void SelectCard(Card card)
    {
        if (myCard != null)
        {
            linkedButton.unShowCard();
        }
        if (card == myCard) { UnSelectCard(); }
        else
        {
            myCard = (OutOfCombatCard)card;
            OutOfCombatCardButton[] cardButtons = GetComponentsInChildren<OutOfCombatCardButton>();
            foreach (OutOfCombatCardButton cardButton in cardButtons)
            {
                if (cardButton.myCard != myCard) { cardButton.UnHighlight(); }
                else { linkedButton = cardButton; }
            }
        }
    }

    public void allowLongRest()
    {
        LongRestButton.interactable = true;
    }

    public void LongRest()
    {
        LongRestButton.interactable = false;
        combatHand.Hand.SetActive(true);
        combatHand.ShortRest();
        combatHand.Hand.SetActive(false);
    }
}

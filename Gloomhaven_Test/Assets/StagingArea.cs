using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagingArea : MonoBehaviour {

    public Action CurrentStagedAction;

    DiscardPile discardPile;

    private void Start()
    {
        discardPile = FindObjectOfType<DiscardPile>();
    }

    public void DiscardCards()
    {
        NewCard[] cards = GetComponentsInChildren<NewCard>();
        foreach(NewCard card in cards)
        {
            discardPile.DiscardCard(card);
        }
        FindObjectOfType<NewHand>().MakeAllCardsPlayable();
        ClearStagedAction();
    }

    void ShowAction()
    {
        FindObjectOfType<PlayerController>().ShowStagedAction(CurrentStagedAction);
    }

    void SetFirstAction(Action CardAction)
    {
        CurrentStagedAction.thisActionType = CardAction.thisActionType;
        CurrentStagedAction.Range = CardAction.Range;
        CurrentStagedAction.thisAOE = CardAction.thisAOE;
        CurrentStagedAction.Duration = CardAction.Duration;
        ShowAction();
    }


    void AddToStagedAction(Action CardAction)
    {
        switch (CardAction.thisActionType)
        {
            case ActionType.Movement:
                CurrentStagedAction.Range += CardAction.Range;
                break;
            case ActionType.Attack:
                CurrentStagedAction.thisAOE.Damage += CardAction.thisAOE.Damage;
                break;
        }
        ShowAction();
    }

    void SubtractToStagedAction(Action CardAction)
    {
        switch (CardAction.thisActionType)
        {
            case ActionType.Movement:
                CurrentStagedAction.Range -= CardAction.Range;
                break;
            case ActionType.Attack:
                CurrentStagedAction.thisAOE.Damage -= CardAction.thisAOE.Damage;
                break;
        }
        ShowAction();
    }

    void ClearStagedAction()
    {
        CurrentStagedAction.thisActionType = ActionType.None;
        CurrentStagedAction.Range = 0;
        CurrentStagedAction.Duration = 0;
        ShowAction();
    }

    public GameObject[] Positions;

    public void ReturnLastCardToHand()
    {
        NewCard card = GetLastCard();
        int index = card.transform.parent.GetSiblingIndex();
        FindObjectOfType<NewHand>().PlaceCardOnNextAvailableSpot(card);
        if (index == 0) {
            ClearStagedAction();
            FindObjectOfType<NewHand>().MakeAllCardsPlayable();
        }
        else { SubtractToStagedAction(card.cardAbility.Actions[0]); }
    }

    public NewCard GetLastCard()
    {
        for (int i = Positions.Length - 1; i >= 0; i--)
        {
            if (Positions[i].GetComponentInChildren<NewCard>() != null)
            {
                return Positions[i].GetComponentInChildren<NewCard>();
            }
        }
        return null;
    }

    public void PlaceCardOnNextAvailableSpot(NewCard card)
    {
        for(int i = 0; i < Positions.Length; i++)
        {
            if (Positions[i].GetComponentInChildren<NewCard>() == null)
            {
                PlaceCard(i, card);
                if (i == 0) {
                    SetFirstAction(card.cardAbility.Actions[0]);
                    FindObjectOfType<NewHand>().ShowStackables(card.cardAbility.Actions[0]);
                }
                else { AddToStagedAction(card.cardAbility.Actions[0]); }
                return;
            }
        }
    }

    void PlaceCard(int index, NewCard card)
    {
        card.transform.SetParent(Positions[index].transform);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        card.SetCurrentParent(card.transform.parent);
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewHand : MonoBehaviour {

    public GameObject[] Positions;
    DrawPile drawPile;
    DiscardPile discardPile;

    private void Start()
    {
        drawPile = FindObjectOfType<DrawPile>();
        discardPile = FindObjectOfType<DiscardPile>();
        DrawNewHand();
    }

    public void DrawNewHand()
    {
        for (int i = 0; i < 5; i++)
        {
            DrawCard();
        }
    }

    public void DiscardHand()
    {
        NewCard[] cards = GetComponentsInChildren<NewCard>();
        foreach(NewCard card in cards)
        {
            discardPile.DiscardCard(card);
        }
    }

    void DrawCard()
    {
        if (GetComponentsInChildren<NewCard>().Length >= 9) { return; }
        NewCard card = drawPile.DrawCard();
        card.gameObject.SetActive(true);
        PlaceCardOnNextAvailableSpot(card);
    }

    public void PutCardInStaging(NewCard card)
    {
        FindObjectOfType<StagingArea>().PlaceCardOnNextAvailableSpot(card);
        ShiftHand();
    }

    public void ShowStackables(Action currentAction)
    {
        NewCard[] cards = GetComponentsInChildren<NewCard>();
        foreach(NewCard card in cards)
        {
            Action cardAction = card.cardAbility.Actions[0];
            if (currentAction.thisActionType != cardAction.thisActionType ||
                currentAction.Range != cardAction.Range ||
                currentAction.thisAOE.thisAOEType != cardAction.thisAOE.thisAOEType)
            {
                card.SetUnPlayable();
            }
        }
    }

    public void MakeAllCardsUnPlayable()
    {
        NewCard[] cards = GetComponentsInChildren<NewCard>();
        foreach (NewCard card in cards)
        {
            card.SetUnPlayable();
        }
    }

    public void MakeAllCardsPlayable()
    {
        NewCard[] cards = GetComponentsInChildren<NewCard>();
        foreach (NewCard card in cards)
        {
            card.SetPlayable();
        }
    }

    public void ShiftHand()
    {
        for (int i = 4; i < Positions.Length - 1; i++)
        {
            if (Positions[i].GetComponentInChildren<NewCard>() == null)
            {
                if (Positions[i + 1].GetComponentInChildren<NewCard>() != null)
                {
                    NewCard card = Positions[i + 1].GetComponentInChildren<NewCard>();
                    PlaceCard(i, card);
                }
            }
        }

        for(int i = 3; i > 0; i--)
        {
            if (Positions[i].GetComponentInChildren<NewCard>() == null)
            {
                if (Positions[i - 1].GetComponentInChildren<NewCard>() != null)
                {
                    NewCard card = Positions[i - 1].GetComponentInChildren<NewCard>();
                    PlaceCard(i, card);
                }
            }
        }
    }

    public void PlaceCardOnNextAvailableSpot(NewCard card)
    {
        for (int i = 0; i < Positions.Length; i++)
        {
            int index = 0;
            if (i == 0) { index = 4; }
            if (i == 1) { index = 5; }
            if (i == 2) { index = 3; }
            if (i == 3) { index = 6; }
            if (i == 4) { index = 2; }
            if (i == 5) { index = 7; }
            if (i == 6) { index = 1; }
            if (i == 7) { index = 0; }
            if (i == 8) { index = 8; }
            if (Positions[index].GetComponentInChildren<NewCard>() == null)
            {
                PlaceCard(index, card);
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
        card.transform.localScale = new Vector3(1, 1, 1);
    }
}

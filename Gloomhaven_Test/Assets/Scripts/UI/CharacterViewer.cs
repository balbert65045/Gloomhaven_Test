﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterViewer : CharacterCard
{
    public GameObject CurrentActionPanel;
    public GameObject CurrentActionPosition;

    private GameObject currentActionCard;

    public void ShowActionCard(GameObject card)
    {
        CurrentActionPanel.SetActive(true);
        card.transform.SetParent(CurrentActionPosition.transform);
        card.transform.localPosition = Vector3.zero;
        currentActionCard = card;
        currentActionCard.SetActive(true);
    }

    public void HideActionCard()
    {
        CurrentActionPanel.SetActive(false);
        if (currentActionCard != null) { currentActionCard.SetActive(false); }
    }

}

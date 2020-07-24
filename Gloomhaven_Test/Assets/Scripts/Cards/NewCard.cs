﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NewCard : Card, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    public Text EnergyText;
    public GameObject PrefabAssociatedWith;
    public int EnergyAmount;
    public CardAbility cardAbility;
    bool Showing = false;
    bool Dragging = false;
    Transform CurrentParent;
    public void SetCurrentParent(Transform t) { CurrentParent = t; }
    Vector3 originalScale;
    Vector3 originalPosition;

    bool playable = true;
    public GameObject UnPlayablePanel;
    public void SetUnPlayable() {
        playable = false;
        UnPlayablePanel.SetActive(true);
    }
    public void SetPlayable()
    {
        playable = true;
        UnPlayablePanel.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (inHand() && playable && FindObjectOfType<PlayerController>().CardsPlayable)
        {
            Dragging = true;
        }
        else if (inLoot())
        {
            GetComponentInParent<CardLoot>().AddCardToStorage(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Dragging = false;
        if (inStaging() && FindObjectOfType<PlayerController>().CardsPlayable)
        {
            FindObjectOfType<StagingArea>().ReturnLastCardToHand();
            return;
        }
        else if (inHand() && playable && FindObjectOfType<PlayerController>().CardsPlayable)
        {
            if (EnergyAmount > FindObjectOfType<EnergyAmount>().CurrentEnergyAmount) { unShowCard(); }
            else if (cardAbility.Staging) { FindObjectOfType<NewHand>().PutCardInStaging(this); }
            else{ FindObjectOfType<NewHand>().UseCard(this); }
        }
        else
        {
            unShowCard();
        }
    }

    bool inLoot() { return GetComponentInParent<CardLoot>() != null; }
    bool inHand(){ return GetComponentInParent<NewHand>() != null; }
    bool inStaging() { return GetComponentInParent<StagingArea>() != null; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inHand() && playable && FindObjectOfType<PlayerController>().CardsPlayable)
        {
            Showing = true;
            showCard();
        }
    }

    public virtual void PointerExited()
    {
        Showing = false;
        unShowCard();
    }

    public void showCard()
    {
        transform.localScale = new Vector3( 1.3f, 1.3f);
        transform.SetParent(transform.parent.parent);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 2f); 
    }

    public void unShowCard()
    {
        if (GetComponentInParent<CardsPanel>() != null) { return; }
        transform.SetParent(CurrentParent);
        transform.localScale = new Vector3(1,1,1);
        transform.localPosition = Vector3.zero;
    }

    bool OverThisCard(List<RaycastResult> results)
    {
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<NewCard>() != null && result.gameObject.GetComponent<NewCard>() == this)
            {
                return true;
            }
        }
        return false;
    }

    private void Start()
    {
        SetCurrentParent(transform.parent);
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }

    void Update () {
        if (Showing)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            if (!OverThisCard(raysastResults)) { PointerExited(); }
        }
        if (Dragging)
        {
            transform.position = Input.mousePosition;
        }
    }
}

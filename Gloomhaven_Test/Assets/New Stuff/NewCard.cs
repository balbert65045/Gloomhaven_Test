using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewCard : Card, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
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
            FindObjectOfType<NewHand>().PutCardInStaging(this);
        }
        else
        {
            unShowCard();
        }
    }

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
        transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 1.3f);
        transform.SetParent(transform.parent.parent);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 2f); 
    }

    public void unShowCard()
    {
        transform.SetParent(CurrentParent);
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
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

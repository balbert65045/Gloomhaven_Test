using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardButton : MonoBehaviour, IPointerEnterHandler
{
    public Text nameText;
    public bool Discarded = false;
    public bool Lost = false;

    public Color OGColor;
    public Card myCard;
    public HandCardShowArea showArea;
    private Vector3 OldScale;

    protected bool Showing = false;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Showing = true;
        showCard();
    }

    public virtual void PointerExited()
    {
        Showing = false;
        if (GetComponentInParent<OutOfCombatHand>().GetSelectecdCard() != myCard)
        {
            unShowCard();
        }
    }

    public void putBackInHand()
    {
        Lost = false;
        Discarded = false;
        GetComponent<Button>().interactable = true;
        GetComponent<Image>().color = Color.white;
    }

    public void DiscardCard()
    {
        Discarded = true;
        GetComponent<Image>().color = Color.red;
        GetComponent<Button>().interactable = false;
    }

    public void LoseCard()
    {
        Discarded = false;
        GetComponent<Image>().color = Color.black;
        Lost = true;
    }

    public virtual void showCard()
    {
        myCard.gameObject.SetActive(true);
        myCard.transform.SetParent(showArea.transform);
        myCard.transform.localPosition = Vector3.zero;
        myCard.transform.localScale = OldScale;
    }

    public void unShowCard()
    {
        if (myCard != null)
        {
            myCard.transform.SetParent(this.transform);
            myCard.transform.localPosition = Vector3.zero;
            myCard.transform.localScale = OldScale;
            myCard.gameObject.SetActive(false);
        }
    }

    public void SelectCard()
    {
        GetComponent<Image>().color = Color.blue;
        GetComponentInParent<Hand>().SelectCard(myCard);
    }

    // Use this for initialization
    public virtual void Start () {
        myCard = GetComponentInChildren<Card>();
        showArea = FindObjectOfType<HandCardShowArea>();
        //OGColor = GetComponent<Image>().color;
        OldScale = myCard.transform.localScale;
        myCard.gameObject.SetActive(false);
    }
	
    bool OverThisButton(List<RaycastResult> results)
    {
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<CardButton>() != null && result.gameObject.GetComponent<CardButton>() == this)
            {
                return true;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update () {
		if (Showing)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            if (!OverThisButton(raysastResults)) { PointerExited(); }
            
        }
	}
}

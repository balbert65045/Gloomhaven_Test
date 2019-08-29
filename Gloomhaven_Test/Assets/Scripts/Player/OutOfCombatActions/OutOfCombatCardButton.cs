using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OutOfCombatCardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{

    public OutOfCombatCard myCard;
    public HandCardShowArea showArea;

    public bool Discarded = false;
    public bool Lost = false;

    public Color OGColor;
    private Vector3 OldScale;

    public void OnPointerEnter(PointerEventData eventData)
    {
        showCard();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GetComponentInParent<OutOfCombatHand>().GetSelectecdCard() != myCard)
        {
            unShowCard();
        }
    }

    public void SelectCard()
    {
        GetComponent<Image>().color = Color.blue;
        GetComponentInParent<OutOfCombatHand>().SelectCard(myCard);
        //DiscardCard();
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

    public void putBackInHand()
    {
        Lost = false;
        Discarded = false;
        GetComponent<Button>().interactable = true;
        GetComponent<Image>().color = OGColor;
    }

    public void UnHighlight()
    {
        if (!Discarded && !Lost)
        {
            GetComponent<Image>().color = OGColor;
        }
    }

    public void showCard()
    {
        myCard.transform.SetParent(showArea.transform);
        myCard.transform.localPosition = Vector3.zero;
        myCard.transform.localScale = OldScale;
        myCard.gameObject.SetActive(true);
    }

    public void unShowCard()
    {
        if (myCard == null) { return; }
        myCard.transform.SetParent(this.transform);
        myCard.transform.localPosition = Vector3.zero;
        myCard.transform.localScale = OldScale;
        myCard.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Start () {
        myCard = GetComponentInChildren<OutOfCombatCard>();
        OldScale = myCard.transform.localScale;
        myCard.gameObject.SetActive(false);
        //OGColor = GetComponent<Image>().color;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

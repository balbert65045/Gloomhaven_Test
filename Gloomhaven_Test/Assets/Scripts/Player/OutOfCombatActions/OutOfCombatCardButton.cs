using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OutOfCombatCardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{

    public OutOfCombatCard myCard;
    public HandCardShowArea showArea;

    private Color OGColor;
    private Vector3 OldScale;

    public void OnPointerEnter(PointerEventData eventData)
    {
        showCard();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        unShowCard();
    }

    public void SelectCard()
    {
        GetComponentInParent<OutOfCombatHand>().SelectCard(myCard);
        GetComponent<Button>().interactable = false;
    }

    void showCard()
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
        OGColor = GetComponent<Image>().color;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

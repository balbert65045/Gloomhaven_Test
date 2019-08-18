using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CombatPlayerCardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{

    public CombatPlayerCard myCard;
    public HandCardShowArea showArea;

    private Color OGColor;
    private Vector3 OldScale;

    public Text InitText;

    public bool basicAttack = false;
    public bool Discarded = false;
    public bool Lost = false;

    public CombatPlayerCard getMyCard()
    {
        return myCard;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        showCard();
        FindObjectOfType<CombatPlayerHand>().ShowPotential(myCard);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CombatPlayerHand hand = FindObjectOfType<CombatPlayerHand>();
        if (hand.getSelectedCard() == null)
        {
            hand.HidePotential();
        }
        else
        {
            hand.HidePotential();
            hand.ShowPotential(hand.getSelectedCard());
        }
        unShowCard();
    }

    public void DiscardCard()
    {
        Discarded = true;
        GetComponent<Button>().interactable = false;
        Unhighlight();
    }

    public void LoseCard()
    {
        Discarded = false;
        Lost = true;
        GetComponent<Image>().color = Color.black;
        GetComponent<Button>().interactable = false;
        //InitText.GetComponent<Image>().color = Color.black;
    }

    public void putBackInHand()
    {
        Lost = false;
        Discarded = false;
        GetComponent<Button>().interactable = true;
        //InitText.GetComponent<Image>().color = Color.white;
        Unhighlight();
    }

    public void Unhighlight()
    {
        if (Lost) { GetComponent<Image>().color = Color.black; }
        else { GetComponent<Image>().color = OGColor; }
    }

    public void SelectCard()
    {
        GetComponent<Image>().color = Color.blue;
        GetComponentInParent<CombatPlayerHand>().SelectCard(myCard);
    }

    void showCard()
    {
        myCard.transform.SetParent(showArea.transform);
        myCard.transform.localPosition = Vector3.zero;
        myCard.transform.localScale = OldScale;
        myCard.gameObject.SetActive(true);
        myCard.SetUpCardActions();
    }

    public void unShowCard()
    {
        myCard.transform.SetParent(this.transform);
        myCard.transform.localPosition = Vector3.zero;
        myCard.transform.localScale = OldScale;
        myCard.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Start () {
        myCard = GetComponentInChildren<CombatPlayerCard>();
        OldScale = myCard.transform.localScale;
        myCard.gameObject.SetActive(false);
        OGColor = GetComponent<Image>().color;

        InitText.text = myCard.Initiative.ToString();

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

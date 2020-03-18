using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CombatPlayerCardButton : CardButton{

    public Text InitText;

    public bool basicAttack = false;

    public CombatPlayerCard getMyCard()
    {
        return (CombatPlayerCard)myCard;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        FindObjectOfType<CombatPlayerHand>().ShowPotential((CombatPlayerCard)myCard);
    }

    public override void PointerExited()
    {
        Showing = false;
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

        if (GetComponentInParent<CombatPlayerHand>().getSelectedCard() != myCard)
        {
            unShowCard();
        }
    }

    public void ReturnToNormalColor()
    {
        if (Lost) { GetComponent<Image>().color = Color.black; }
        else { GetComponent<Image>().color = Color.white; }
    }

    public void Unhighlight()
    {
        if (Lost) { GetComponent<Image>().color = Color.black; }
        else if (Discarded) { GetComponent<Image>().color = Color.red; }
        else { GetComponent<Image>().color = OGColor; }
    }

    public override void showCard()
    {
        base.showCard();
        ((CombatPlayerCard)myCard).SetUpCardActions();
    }

    // Use this for initialization
    public override void Start () {
        base.Start();
        InitText.text = ((CombatPlayerCard)myCard).Initiative.ToString();
    }

}

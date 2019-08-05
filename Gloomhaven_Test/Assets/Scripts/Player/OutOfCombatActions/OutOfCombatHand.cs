using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfCombatHand : MonoBehaviour {

    public GameObject Hand;
    OutOfCombatCard myCard;

    public void HideHand()
    {
        Hand.SetActive(false);
    }

    public void ShowHand()
    {
        Hand.SetActive(true);
    }

    public OutOfCombatCard GetSelectecdCard()
    {
        return myCard;
    }

    public void SelectCard(OutOfCombatCard card)
    {
        myCard = card;
        FindObjectOfType<PlayerController>().SelectCard();
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CCSCard : MonoBehaviour, IPointerClickHandler {

    public CardMaskPanel cardMask;
    bool InHand = false;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (!InHand) {
            if (PutInHand())
            {
                PutCardInHandForStorage();
            }
        }
    }

    void PutCardInHandForStorage()
    {
        string name = FindObjectOfType<CCSCardPanel>().NameText.text;
        GameObject prefab = FindObjectOfType<PlayerCardDatabase>().FindCardByID(GetComponent<Card>());
        string type = GetComponent<CombatPlayerCard>() != null ? "Combat" : "Exploration";
        FindObjectOfType<GroupCardStorage>().AddCardToHandFromObtained(name, prefab, type);
    }

    public void PutCardOutOfHandForStorage()
    {
        string name = FindObjectOfType<CCSCardPanel>().NameText.text;
        GameObject prefab = FindObjectOfType<PlayerCardDatabase>().FindCardByID(GetComponent<Card>());
        string type = GetComponent<CombatPlayerCard>() != null ? "Combat" : "Exploration";
        FindObjectOfType<GroupCardStorage>().RemoveCardFromHandToObtained(name, prefab, type);
    }

    public void SetPanelMask()
    {
        cardMask = transform.parent.GetComponentInChildren<CardMaskPanel>();
    }

    public bool PutInHand()
    {
        CCSCardButtonPanel CardPanel = FindObjectOfType<CCSCardButtonPanel>();
        if (!CardPanel.AtMaxSize())
        {
            string myName = GetComponent<Card>().NameText.text;
            GameObject prefab = FindObjectOfType<PlayerCardDatabase>().FindCardByID(GetComponent<Card>());
            CardPanel.AddCardButton(myName, prefab);
            InHand = true;
            cardMask.gameObject.SetActive(true);
            return true;
        }
        return false;
    }

    public void TakeOutOfHand()
    {
        if (InHand)
        {
            string myName = GetComponent<Card>().NameText.text;
            FindObjectOfType<CCSCardButtonPanel>().RemoveCardButton(myName);
            OutOfHand();
            PutCardOutOfHandForStorage();
        }
    }

    public void OutOfHand()
    {
        InHand = false;
        cardMask.gameObject.SetActive(false);
    }

	// Use this for initialization
	void Start () {
        if (!InHand)
        {
            cardMask = transform.parent.GetComponentInChildren<CardMaskPanel>();
            cardMask.gameObject.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

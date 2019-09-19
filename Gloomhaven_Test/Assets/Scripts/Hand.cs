using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hand : MonoBehaviour {

    public GameObject CardButtonPrefab;
    public GameObject[] ButtonPositions;
    public Text CurrentHandSizeText;

    public int handSize;
    public void SetHandSize(int value) { handSize = value; }

    public List<Card> CardsHolding = new List<Card>();

    public GameObject HandViewButton;

    public virtual void SelectCard(Card card)
    {
    }

    public void AddCard(GameObject cardPrefab)
    {
        int index = GetNextAvailableOpenSpot();
        if (index == -1) { return; }
        SetUpCard(index, cardPrefab);

        CurrentHandSizeText.text = CardsHolding.Count + "/" + handSize;
    }

    void SetUpCard(int index, GameObject cardPrefab)
    {
        GameObject button = Instantiate(CardButtonPrefab, ButtonPositions[index].transform);
        GameObject card = Instantiate(cardPrefab, button.transform);
        card.transform.localScale = new Vector3(.308f, .308f, .771f);
        CardsHolding.Add(card.GetComponent<Card>());
        button.name = cardPrefab.name;
        button.GetComponent<CardButton>().nameText.text = cardPrefab.name;
    }

    public void ReplaceCard(GameObject cardPrefab, GameObject cardReplacing)
    {
        int cardIndex = findCardIndex(cardReplacing.GetComponent<Card>());
        Destroy(ButtonPositions[cardIndex].GetComponentInChildren<CardButton>().gameObject);
        Destroy(cardReplacing);
        SetUpCard(cardIndex, cardPrefab);
    }

    public void LoseCard(Card card)
    {
        int cardIndex = findCardIndex(card);
        CardsHolding.Remove(card);
        Destroy(ButtonPositions[cardIndex].GetComponentInChildren<CardButton>().gameObject);
        CurrentHandSizeText.text = CardsHolding.Count + "/" + handSize;
    }

    int findCardIndex(Card card)
    {
        for (int i = 0; i < ButtonPositions.Length; i++)
        {
            if (ButtonPositions[i].GetComponentInChildren<CardButton>().myCard == card) { return i; }
        }
        return -1;
    }

    public int GetNextAvailableOpenSpot()
    {
        for (int i = 0; i < handSize; i++)
        {
            if (ButtonPositions[i].GetComponentInChildren<CardButton>() == null) { return i; }
        }
        return -1;
    }

    public void DisableViewButton()
    {
        HandViewButton.SetActive(false);
    }

    public void EnableViewButton()
    {
        HandViewButton.SetActive(true);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

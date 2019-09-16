using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour {

    public GameObject CardButtonPrefab;
    public GameObject[] ButtonPositions;

    public int handSize;
    public void SetHandSize(int value) { handSize = value; }

    public virtual void SelectCard(Card card)
    {
    }

    public void AddCard(GameObject cardPrefab)
    {
        int index = GetNextAvailableOpenSpot();
        if (index == -1) { return; }
        GameObject button = Instantiate(CardButtonPrefab, ButtonPositions[index].transform);
        GameObject card = Instantiate(cardPrefab, button.transform);
        button.name = cardPrefab.name;
        button.GetComponent<CardButton>().nameText.text = cardPrefab.name;
    }

    public int GetNextAvailableOpenSpot()
    {
        for (int i = 0; i < handSize; i++)
        {
            if (ButtonPositions[i].GetComponentInChildren<CardButton>() == null) { return i; }
        }
        return -1;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

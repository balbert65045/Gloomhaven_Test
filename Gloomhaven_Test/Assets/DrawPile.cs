using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawPile : MonoBehaviour {

    public List<GameObject> InitialDeck;
    public List<NewCard> CardsCurrentlyInDrawPile = new List<NewCard>();
    public Text DrawPileNumber;

    DiscardPile discardPile;
	// Use this for initialization
	void Awake () {
        discardPile = FindObjectOfType<DiscardPile>();

        foreach (GameObject prefabCard in InitialDeck)
        {
            GameObject newCard = Instantiate(prefabCard, this.transform);
            CardsCurrentlyInDrawPile.Add(newCard.GetComponent<NewCard>());
            newCard.SetActive(false);
        }
	}

    public NewCard DrawCard()
    {
        if (CardsCurrentlyInDrawPile.Count == 0) { PutDiscardIntoDrawPile(); }
        NewCard card = CardsCurrentlyInDrawPile[Random.Range(0, CardsCurrentlyInDrawPile.Count)];
        CardsCurrentlyInDrawPile.Remove(card);
        DrawPileNumber.text = CardsCurrentlyInDrawPile.Count.ToString();
        return card;
    }

    void PutDiscardIntoDrawPile()
    {
        NewCard[] cards = discardPile.GetAllDiscardedCards();
        foreach(NewCard card in cards)
        {
            card.transform.SetParent(this.transform);
            CardsCurrentlyInDrawPile.Add(card);
        }
    }
}

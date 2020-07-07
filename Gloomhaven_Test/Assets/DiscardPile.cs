using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiscardPile : MonoBehaviour {

    public List<NewCard> CardsCurrentlyInDiscardPile = new List<NewCard>();
    public Text DiscardPileNumber;

    DrawPile drawPile;

    public NewCard[] GetAllDiscardedCards()
    {
        NewCard[] cards = CardsCurrentlyInDiscardPile.ToArray();
        CardsCurrentlyInDiscardPile.Clear();
        DiscardPileNumber.text = CardsCurrentlyInDiscardPile.Count.ToString();
        return cards;
    }

    public void DiscardCard(NewCard card)
    {
        CardsCurrentlyInDiscardPile.Add(card);
        card.transform.SetParent(this.transform);
        card.gameObject.SetActive(false);
        DiscardPileNumber.text = CardsCurrentlyInDiscardPile.Count.ToString();
    }

    void Start () {
        drawPile = FindObjectOfType<DrawPile>();
    }
}

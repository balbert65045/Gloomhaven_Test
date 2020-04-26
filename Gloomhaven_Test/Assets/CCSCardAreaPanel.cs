using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCSCardAreaPanel : MonoBehaviour {

    public GameObject ContentPanel;
    public GameObject PanelMask;
    public GameObject Pos;

    public List<GameObject> Positions;
    public int TotalCardsObtained = 0;
    

    public void CardRemoved(string name)
    {
        CCSCard[] cards = GetComponentsInChildren<CCSCard>();
        foreach(CCSCard card in cards)
        {
            if (card.GetComponent<Card>().NameText.text == name)
            {
                card.OutOfHand();
                card.PutCardOutOfHandForStorage();
                return;
            }
        }
    }

    public void ShowExplorationCards(string characterName)
    {
        TotalCardsObtained = 0;
        FindObjectOfType<CCSCardButtonPanel>().ClearHand();
        List<GameObject> CardsObtained = FindObjectOfType<GroupCardStorage>().GetAllExplorationCardsObtained(characterName);
        AddCardsToPanel(CardsObtained);
        List<GameObject> CardsInHand = FindObjectOfType<GroupCardStorage>().GetAllExplorationCardsInHand(characterName);
        AddHandCardsToPanel(CardsInHand);
    }

    public void ShowCombatCards(string characterName)
    {
        TotalCardsObtained = 0;
        FindObjectOfType<CCSCardButtonPanel>().ClearHand();
        List<GameObject> CardsObtained = FindObjectOfType<GroupCardStorage>().GetAllCombatCardsObtained(characterName);
        AddCardsToPanel(CardsObtained);
        List<GameObject> CardsInHand = FindObjectOfType<GroupCardStorage>().GetAllCombatCardsInHand(characterName);
        AddHandCardsToPanel(CardsInHand);
    }

    void AddHandCardsToPanel(List<GameObject> Cards)
    {
        int index = TotalCardsObtained;
        for (int i = 0; i < Cards.Count; i++)
        {
            GameObject Card = AddCardToPanel(Cards[i], i + index);
            Card.GetComponent<CCSCard>().SetPanelMask();
            Card.GetComponent<CCSCard>().PutInHand();
        }

        AdjustContentSize(Cards.Count + index);
    }

    void AddCardsToPanel(List<GameObject> Cards)
    {
        foreach (GameObject pos in Positions) { Destroy(pos); }
        Positions.Clear();
        for (int i = 0; i < Cards.Count; i++)
        {
            AddCardToPanel(Cards[i], i);
        }
    }

    void AdjustContentSize(int cards)
    {
        foreach(GameObject pos in Positions) { pos.transform.SetParent(null); }
        if (cards > 6)
        {
            int CardsOverThreshhold = cards - 6;
            int AdditionalRows = (CardsOverThreshhold / 3) + 1;
            float yIncrease = AdditionalRows * .3f;
            ContentPanel.transform.localScale = new Vector3(1, 1+yIncrease, 1);
        }
        else
        {
            ContentPanel.transform.localScale = new Vector3(1, 1, 1);
        }
        foreach (GameObject pos in Positions) { pos.transform.SetParent(ContentPanel.transform); }
    }

    GameObject AddCardToPanel(GameObject prefab, int index)
    {
        GameObject newPos = Instantiate(Pos, ContentPanel.transform);
        int column = index % 3;
        int row = index / 3;
        float x = ((column * 250f) + 250);
        float y = (-200f - (row * 370f));
        //newPos.transform.localPosition = new Vector3(x, y, 0);
        Positions.Add(newPos);

        GameObject card = Instantiate(prefab, newPos.transform);
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = new Vector3(.954f, .954f, 2.385f);
        card.AddComponent<CCSCard>();

        GameObject mask = Instantiate(PanelMask, newPos.transform);
        mask.transform.localPosition = Vector3.zero;

        newPos.transform.localPosition = new Vector3(x, y, 0);
        TotalCardsObtained++;
        return card;
    }
}

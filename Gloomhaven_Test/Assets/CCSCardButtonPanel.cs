using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCSCardButtonPanel : MonoBehaviour {

    public int HandCurrentSize = 0;
    public int HandMaxSize = 5;

    public bool AtMaxSize() { return HandCurrentSize == HandMaxSize; }

    public void IncreaseHandSize()
    {
        HandCurrentSize++;
        RefreshHandSizeText();
    }
    public void DecreaseHandSize()
    {
        HandCurrentSize--;
        RefreshHandSizeText();
    }
    void RefreshHandSizeText() { FindObjectOfType<CCSHandSize>().SetHandSize(HandCurrentSize, HandMaxSize); }

    public GameObject[] Positions;
    public GameObject CardButtonPrefab;
    public int HandPosIndex = 0;

    public void ClearHand()
    {
        foreach(GameObject Position in Positions)
        {
            if (Position.GetComponentInChildren<CCSCardButton>() != null) { Destroy(Position.GetComponentInChildren<CCSCardButton>().gameObject); }
        }
        HandCurrentSize = 0;
        HandPosIndex = 0;
        RefreshHandSizeText();
    }

    public void RemoveCardButton(CCSCardButton button)
    {
        int index = FindButtonIndexByName(button.Name.text);
        FindObjectOfType<CCSCardAreaPanel>().CardRemoved(button.Name.text);
        Destroy(button.gameObject);
        BumpDown(index);
        DecreaseHandSize();
        HandPosIndex--;
    }

    public void AddCardButton(string name, GameObject prefab)
    {
        int index = HandPosIndex;
        if (index == -1) { return; }
        GameObject button = Instantiate(CardButtonPrefab, Positions[index].transform);
        button.transform.localPosition = new Vector3(10, 0, 0);
        button.GetComponent<CCSCardButton>().SetName(name);
        if (prefab.GetComponent<CombatPlayerCard>() != null) { button.GetComponent<CCSCombatCardButton>().SetInit(prefab.GetComponent<CombatPlayerCard>().Initiative); }
        button.GetComponent<CCSCardButton>().MyCardPrefab = prefab;
        IncreaseHandSize();
        HandPosIndex++;
    }

    public void RemoveCardButton(string name)
    {
        int index = FindButtonIndexByName(name);
        if (index >= 0)
        {
            Destroy(Positions[index].GetComponentInChildren<CCSCardButton>().gameObject);
        }
        BumpDown(index);
        DecreaseHandSize();
        HandPosIndex--;
    }

    int FindButtonIndexByName(string name)
    {
        for (int i = 0; i < PositionsInUse(); i++)
        {
            if (Positions[i].GetComponentInChildren<CCSCardButton>().Name.text == name) { return i; }
        }
        return -1;
    }

    int PositionsInUse() { return GetComponentsInChildren<CCSCardButton>().Length;}

    void Reorder()
    {
        for (int i = 0; i < Positions.Length - 1; i++)
        {
            if (Positions[i].GetComponentInChildren<CCSCardButton>() == null && Positions[i + 1].GetComponentInChildren<CCSCardButton>() != null)
            {
                BumpDown(i);
                return;
            }
        }
    }

    void BumpDown(int index)
    {
        CCSCardButton button = Positions[index + 1].GetComponentInChildren<CCSCardButton>();
        if (button != null)
        {
            button.transform.SetParent(Positions[index].transform);
            button.transform.localPosition = new Vector3(10, 0, 0);
            BumpDown(index + 1);
        }
    }
}

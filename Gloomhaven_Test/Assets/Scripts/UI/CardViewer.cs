using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardViewer : MonoBehaviour {

    public GameObject Panel;
    public GameObject[] Positions;
    public bool showing = false;

    GameObject OldParent;

    public void AddCards(CombatPlayerCard[] cards, GameObject parent)
    {
        showing = true;
        Panel.SetActive(true);
        OldParent = parent;
        int index = 0;
        foreach (CombatPlayerCard card in cards)
        {
            card.transform.SetParent(Positions[index].transform);
            card.transform.localPosition = Vector3.zero;
            card.transform.localRotation = Quaternion.Euler(Vector3.zero);
            index++;
        }
    }

    public void PutCardsBack()
    {
        showing = false;
        CombatPlayerCard[] cards = GetComponentsInChildren<CombatPlayerCard>();
        foreach(CombatPlayerCard card in cards)
        {
            card.transform.SetParent(OldParent.transform);
            card.transform.localPosition = new Vector3(0, -200, 0);
        }
        Panel.SetActive(false);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

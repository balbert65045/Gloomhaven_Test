using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitiativePosition : MonoBehaviour {

    public string CharacterNameLinkedTo;
    public Image InitiativeCharacterImage;
    public Text InitiativeValueText;
    public int InitValue;
    public GameObject Init;

    public GameObject myCard;

    public void Selected()
    {
        FindObjectOfType<CombatManager>().SelectCharacter(CharacterNameLinkedTo);
    }

    public void AddCard(GameObject card)
    {
        myCard = card;
    }

    public GameObject GetCard() { return myCard; }

    public void unLinkCharacter()
    {
        InitiativeCharacterImage.sprite = null;
        CharacterNameLinkedTo = null;
        InitiativeValueText.text = "?";
        Init.SetActive(false);
    }

    public void LinkCharacter(Sprite characterIcon, string name)
    {
        InitiativeCharacterImage.sprite = characterIcon;
        CharacterNameLinkedTo = name;
        InitiativeValueText.text = "?";
        Init.SetActive(true);
    }

    public void SetInitiative(int initiativeValue)
    {
        InitValue = initiativeValue;
        InitiativeValueText.text = initiativeValue.ToString();
    }

    public void ResetNewTurn()
    {
        InitValue = 0;
        InitiativeValueText.text = "?";
        CharacterNameLinkedTo = "";
        Init.SetActive(false);
        myCard = null;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionButton : MonoBehaviour {

    public PlayerCharacter characterLinkedTo;
    private PlayerController playerController;
    private Color OGcolor;

	// Use this for initialization
	void Start () {
        playerController = FindObjectOfType<PlayerController>();
        OGcolor = GetComponent<Image>().color;
    }

    public void SelectCharacter()
    {
        playerController.SelectCharacter(characterLinkedTo);
    }

    public void CharacterSelected()
    {
        GetComponent<Image>().color = Color.green;
        CharacterSelectionButton[] otherButtons = FindObjectsOfType<CharacterSelectionButton>();
        foreach (CharacterSelectionButton button in otherButtons)
        {
            if (button != this) { button.ReturnToColor(); }
        }
    }

    public void ReturnToColor()
    {
        GetComponent<Image>().color = OGcolor;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

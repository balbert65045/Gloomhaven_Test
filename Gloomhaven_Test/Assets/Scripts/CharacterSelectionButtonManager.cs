using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionButtonManager : MonoBehaviour {

    public CharacterSelectionButton[] characterButtons;

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void SetUpCharacterSelectionButtons()
    {
        characterButtons = FindObjectsOfType<CharacterSelectionButton>();
    }

    public void ShowCharacterButtonSelected(PlayerCharacter character)
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            if (button.characterLinkedTo == character)
            {
                button.CharacterSelected();
                return;
            }
        }
    }

    public void HideCharacterSelection()
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            button.hideCardIndicators();
            button.gameObject.SetActive(false);
        }
    }

    public void ShowCardSelected(PlayerCharacter character)
    {
        //character.myCharacterSelectionButton.CardForCharacterSelected();
    }

    public void ShowCardUnselected(PlayerCharacter character)
    {
        //character.myCharacterSelectionButton.CardForCharacterUnselected();
    }

    public void ShowCardIndicators()
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            if (button.CharacterDead) { continue; }
            button.showCardIndicators();
            button.HideActions();
        }
    }

    public void ShowActions()
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            button.ShowActions();
        }
    }

    public void ShowCharacterSelection()
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            if (button.CharacterDead) { continue; }
            button.gameObject.SetActive(true);
            button.ReturnToColor();
        }
    }

    public void RemoveCharacterButton(PlayerCharacter character)
    {
        //character.myCharacterSelectionButton.SetCharacterDeadValue(true);
        //character.myCharacterSelectionButton.gameObject.SetActive(false);
    }

    public void FilterCharacter(PlayerCharacter character)
    {
       // character.myCharacterSelectionButton.Disable();
    }

    public void ReturnButtonsToNormal()
    {
        foreach (CharacterSelectionButton CSB in characterButtons)
        {
            CSB.Enable();
        }
    }

}

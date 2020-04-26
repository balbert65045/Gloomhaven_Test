using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPickerTypeButton : MonoBehaviour {

    public Button OtherButton;

    public void ExplorationButtonSelected()
    {
        GetComponent<Button>().interactable = false;
        OtherButton.interactable = true;
        FindObjectOfType<CCSCardAreaPanel>().ShowExplorationCards(FindObjectOfType<CCSCardPanel>().GetCharacterName());
    }

    public void CombatButtonSelected()
    {
        GetComponent<Button>().interactable = false;
        OtherButton.interactable = true;
        FindObjectOfType<CCSCardAreaPanel>().ShowCombatCards(FindObjectOfType<CCSCardPanel>().GetCharacterName());
    }
}

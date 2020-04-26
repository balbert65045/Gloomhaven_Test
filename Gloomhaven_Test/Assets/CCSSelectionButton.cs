using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCSSelectionButton : MonoBehaviour {

	 public GameObject Character;

    public void FocusOnCharacter()
    {
        FindObjectOfType<CSCharacterGroupPanel>().HidePanel();
        FindObjectOfType<CCSCardPanel>().ShowCharacterCards(Character.GetComponent<CCSCharacter>().CharacterName);
        FindObjectOfType<CSCharacterManager>().HideAllOtherCharacters(Character);
        FindObjectOfType<CharacterSelectionCamera>().setTarget(Character.transform.position);
    }
}

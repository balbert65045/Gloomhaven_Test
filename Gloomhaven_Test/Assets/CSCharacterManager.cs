using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSCharacterManager : MonoBehaviour {

    public GameObject[] Characters;

    public void SetCharactersForGroup()
    {
        CSCharacter[] characters = GetComponentsInChildren<CSCharacter>();
        List<GameObject> groupList = new List<GameObject>();
        foreach(CSCharacter character in characters) {
            groupList.Add(character.PrefabAssociatedWith);
            FindObjectOfType<GroupCardStorage>().CreateCharacterStorage(character.Name);
        }
        FindObjectOfType<GroupManager>().SetCharacters(groupList);
    }

    public void HideAllOtherCharacters(GameObject character)
    {
        foreach(GameObject charact in Characters)
        {
            if (charact != character)
            {
                charact.SetActive(false);
            }
        }
    }

    public void RevealAllOtherCharacters()
    {
        foreach (GameObject charact in Characters)
        {
            charact.SetActive(true);
        }
    }

}

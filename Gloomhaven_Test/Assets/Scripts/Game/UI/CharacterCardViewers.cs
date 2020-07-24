using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCardViewers : MonoBehaviour {

    public CharacterCardViewer[] MyViewers;
    int index = 0;
	
    public void AddCharacter(PlayerCharacter character)
    {
        MyViewers[index].SetUpCharacter(character);
        MyViewers[index].gameObject.SetActive(true);
        index++;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterCardStorage
{
    public string CharacterName;
    public List<GameObject> CardsObtained = new List<GameObject>();
    public List<GameObject> CardsHolding = new List<GameObject>();
}

public class GroupCardStorage : MonoBehaviour {

    public CharacterCardStorage[] MyGroupCardStorage = new CharacterCardStorage[4];
    int index = 0;

    public void CreateCharacterStorage(string name)
    {
        if (index < 4)
        {
            CharacterCardStorage CCS = new CharacterCardStorage();
            CCS.CharacterName = name;
            MyGroupCardStorage[index] = CCS;
            index++;
        }
    }

	void Start () {
        index = 0;
    }
}

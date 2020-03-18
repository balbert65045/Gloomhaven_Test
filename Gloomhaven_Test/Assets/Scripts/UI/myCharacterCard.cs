using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class myCharacterCard : MonoBehaviour
{
    public void HideCharacterCards()
    {
        CharacterCard[] cards = GetComponentsInChildren<CharacterCard>();
        foreach (CharacterCard card in cards)
        {
            card.HideCharacterStats();
        }
    }
}

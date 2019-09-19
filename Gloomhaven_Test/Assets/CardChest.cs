using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardChest : MonoBehaviour {

    public bool isOpen = false;

    public void OpenChest(PlayerCharacter character)
    {
        isOpen = true;
        FindObjectOfType<HexVisualizer>().DeactivateHex(GetComponent<Entity>().HexOn);
        GameObject cardPrefab = FindObjectOfType<PlayerCardDatabase>().SelectRandomCard(character);
        Card card = FindObjectOfType<ChestPanel>().ActivePanel(character, cardPrefab, this);
        FindObjectOfType<PlayerController>().ChestOpenedFor(card);
    }

}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionButton : MonoBehaviour {

    public PlayerCharacter characterLinkedTo;
    private PlayerController playerController;
    private Color OGcolor;

    public Sprite NoCardSprite;
    public Sprite HasCardSprite;

    public Image CardIndicatorImage;

	// Use this for initialization
	void Start () {
        playerController = FindObjectOfType<PlayerController>();
        OGcolor = GetComponent<Image>().color;
        CardIndicatorImage.gameObject.SetActive(false);
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

    public void showCardIndicators()
    {
        CardIndicatorImage.gameObject.SetActive(true);
    }

    public void hideCardIndicators()
    {
        CardIndicatorImage.sprite = NoCardSprite;
        CardIndicatorImage.gameObject.SetActive(false);
    }

    public void CardForCharacterSelected()
    {
        CardIndicatorImage.sprite = HasCardSprite;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

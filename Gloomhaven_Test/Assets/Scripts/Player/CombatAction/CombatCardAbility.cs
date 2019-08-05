using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum AbilityType
{
    Top = 1,
    Bottom = 2,
}

public class CombatCardAbility : MonoBehaviour {

    public bool LostAbility = false;
    public AbilityType ThisAbilityType;
    //GetAbilityType
    public Action[] Actions;
    //GetActions

    Color OGColor;

	// Use this for initialization
	void Start () {
        OGColor = GetComponent<Image>().color;
    }

    public void HideAbility()
    {
        Color HideColor = new Color(0, 0, 0, .5f);
        //GetComponent<Image>().color = HideColor;
    }

    public void HighlightAbility()
    {
        Color HighlightColor = new Color(0, 1, 0, .5f);
        GetComponent<Image>().color = HighlightColor;
    }

    public void UnHighlightAbility()
    {
        GetComponent<Image>().color = OGColor;
    }


	
	// Update is called once per frame
	void Update () {
		
	}
}

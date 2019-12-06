using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CombatPlayerCard : Card
{
    public Text InitiativeText;

    public bool LostAbilityUsed = false;

    public int Initiative;
    public CardAbility CardAbility;

    Color OGcolor;

    Vector3 OldPosition;
    Vector3 OldScale;
    int OldSiblingIndex;

    bool CardIncreased = false;

    public void HighlightCurrentAction(int ActionIndex)
    {
        List<ActionLine> lines = new List<ActionLine>();
        for (int i = 0; i < AbilityLinePositions.Length; i++)
        {
            if (AbilityLinePositions[i].GetComponentInChildren<ActionLine>() != null)
            {
                lines.Add(AbilityLinePositions[i].GetComponentInChildren<ActionLine>());
            }
        }
        lines[ActionIndex].HighlightAction();
    }

    public void UnHighlightCurrentAction(int ActionIndex)
    {
        List<ActionLine> lines = new List<ActionLine>();
        for (int i = 0; i < AbilityLinePositions.Length; i++)
        {
            if (AbilityLinePositions[i].GetComponentInChildren<ActionLine>() != null)
            {
                lines.Add(AbilityLinePositions[i].GetComponentInChildren<ActionLine>());
            }
        }
        lines[ActionIndex].ActionBackToNormal();
    }

    public void UnHighlightAllActions()
    {
        for (int i = 0; i < AbilityLinePositions.Length; i++)
        {
            if (AbilityLinePositions[i].GetComponentInChildren<ActionLine>() != null)
            {
                AbilityLinePositions[i].GetComponentInChildren<ActionLine>().ActionBackToNormal();
            }
        }
    }

    public void DisableCurrentAction(int ActionIndex)
    {
        List<ActionLine> lines = new List<ActionLine>();
        for (int i = 0; i < AbilityLinePositions.Length; i++)
        {
            if (AbilityLinePositions[i].GetComponentInChildren<ActionLine>() != null)
            {
                lines.Add(AbilityLinePositions[i].GetComponentInChildren<ActionLine>());
            }
        }
        lines[ActionIndex].ActionUsed();
    }

    public void SetUpCardActions()
    {      
        Character character = FindObjectOfType<PlayerController>().SelectPlayerCharacter;
        CardAbility.setUpCard(character.GetStrength(), character.GetAgility(), character.GetDexterity());
    }

    void IncreaseSize()
    {
        CardIncreased = true;
        OldSiblingIndex = transform.parent.GetSiblingIndex();
        OldPosition = transform.localPosition;
        OldScale = transform.localScale;
        transform.parent.SetAsLastSibling();
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = new Vector3(transform.localScale.x * 1.5f, transform.localScale.y * 1.5f, transform.localScale.z);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 30, transform.localPosition.z);
    }

    public void DecreaseSize()
    {
        CardIncreased = false;
        transform.parent.SetSiblingIndex(OldSiblingIndex);
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = OldScale;
        transform.localPosition = OldPosition;
    }

    public void ChangeColor(Color color)
    {
        GetComponent<Image>().color = color;
    }

    public void RevertToNormalColor()
    {
        GetComponent<Image>().color = OGcolor;
    }

    // Use this for initialization
    void Start () {
        OGcolor = GetComponent<Image>().color;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

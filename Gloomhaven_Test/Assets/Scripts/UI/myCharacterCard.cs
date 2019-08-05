using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class myCharacterCard : MonoBehaviour {

    public GameObject ViewPanel;

    public Text Name;
    public Image CharacterIcon;
    public CharacterAbilityPanel AttackPanel;
    public Text AttackValue;
    public CharacterAbilityPanel MovePanel;
    public Text MoveValue;
    public CharacterAbilityPanel RangePanel;
    public Text RangeValue;
    public CharacterAbilityPanel ShieldPanel;
    public Text ShieldValue;

    public void ShowCharacterStats(string name, Sprite charIcon, Character character)
    {
        AttackPanel.ClearBuffs();
        MovePanel.ClearBuffs();
        RangePanel.ClearBuffs();
        ShieldPanel.ClearBuffs();

        Name.text = name;
        CharacterIcon.sprite = charIcon;
        setValue(AttackPanel, AttackValue, character.Strength, character.baseStrength, -10);
        setValue(MovePanel, MoveValue, character.Agility, character.baseAgility, - 10);
        setValue(RangePanel, RangeValue, character.baseAttackRange, character.baseAttackRange, 1);
        setValue(ShieldPanel, ShieldValue, character.Armor, character.baseShield, 0);
        ViewPanel.SetActive(true);

        foreach (Buff buff in character.Buffs)
        {
            switch (buff.myBuffType)
            {
                case BuffType.Strength:
                    AttackPanel.AddBuff(buff);
                    break;
                case BuffType.Agility:
                    MovePanel.AddBuff(buff);
                    break;
                case BuffType.Armor:
                    ShieldPanel.AddBuff(buff);
                    break;
                case BuffType.Range:
                    RangePanel.AddBuff(buff);
                    break;
            }
        }
    }

    public void HideCharacterStats()
    {
        ViewPanel.SetActive(false);
    }

    void setValue(CharacterAbilityPanel panel, Text valueString, int value, int baseValue, int threshold)
    {
        if (value > threshold)
        {
            panel.gameObject.SetActive(true);
            valueString.text = value.ToString();
            if (value > baseValue) { valueString.color = Color.green; }
            else { valueString.color = Color.white; }
        }
        else
        {
            panel.gameObject.SetActive(false);
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

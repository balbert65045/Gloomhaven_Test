using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour
{

    public GameObject ViewPanel;
    public UIXPBar xpBar;
    public Text Level;

    public Text Name;
    public Image CharacterIcon;
    public CharacterAbilityPanel AttackPanel;
    public Text AttackValue;
    public CharacterAbilityPanel MovePanel;
    public Text MoveValue;
    public CharacterAbilityPanel DexterityPanel;
    public Text RangeValue;
    public CharacterAbilityPanel ShieldPanel;
    public Text ShieldValue;

    public void ShowCharacterStats(string name, Sprite charIcon, Character character)
    {
        AttackPanel.ClearBuffs();
        MovePanel.ClearBuffs();
        DexterityPanel.ClearBuffs();
        ShieldPanel.ClearBuffs();

        Name.text = name;
        CharacterIcon.sprite = charIcon;
        setValue(AttackPanel, AttackValue, character.GetStrength(), character.baseStrength, -10);
        setValue(MovePanel, MoveValue, character.GetAgility(), character.baseAgility, -10);
        setValue(DexterityPanel, RangeValue, character.GetDexterity(), character.baseDexterity, 0);
        setValue(ShieldPanel, ShieldValue, character.GetArmor(), character.baseArmor, 0);
        ViewPanel.SetActive(true);

        foreach (Buff buff in character.GetBuffs())
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
                case BuffType.Dexterity:
                    DexterityPanel.AddBuff(buff);
                    break;
            }
        }
        if (character.GetComponent<PlayerCharacter>() != null)
        {
            ShowXP((PlayerCharacter)character);
        }
    }

    public void ShowXP(PlayerCharacter character)
    {
        Level.text = character.CharacterLevel.ToString();
        xpBar.SetXP(character.CurrentXP, character.XpUntilNextLevel);
    }

    public void GainXP(PlayerCharacter character)
    {
        xpBar.GainXp(character.CurrentXP, character.XpUntilNextLevel);
    }

    public void LevelUpAndGainXP(PlayerCharacter character)
    {
        xpBar.LevelUpAndGainXP(character.CurrentXP, character.XpUntilNextLevel);
        Level.text = character.CharacterLevel.ToString();
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
}
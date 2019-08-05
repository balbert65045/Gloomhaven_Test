using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfCombatActionController : MonoBehaviour {

    private LayerMask MapLayer;
    private PlayerController playerController;
    private Character myCharacter;
    private Character characterSelected;
    // Use this for initialization
    void Start () {
        playerController = GetComponent<PlayerController>();
        MapLayer = playerController.MapLayer;
        myCharacter = playerController.myCharacter;
    }

    public void CheckToShowCharacterStats()
    {
        RaycastHit Hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out Hit, 100f, MapLayer))
        {
            if (Hit.transform.GetComponent<Hex>().EntityHolding != null && Hit.transform.GetComponent<Hex>().EntityHolding.GetComponent<EnemyCharacter>())
            {
                if (characterSelected != null) { characterSelected.HexOn.UnHighlight(); }
                Hit.transform.GetComponent<Hex>().HighlightSelection();
                characterSelected = Hit.transform.GetComponent<Hex>().EntityHolding.GetComponent<Character>();
                EnemyCharacter character = Hit.transform.GetComponent<Hex>().EntityHolding.GetComponent<EnemyCharacter>();
                FindObjectOfType<CharacterViewer>().ShowCharacterStats(character.CharacterName, character.enemySprite, character);
            }
            else if (Hit.transform.GetComponent<Hex>().EntityHolding != null && Hit.transform.GetComponent<Hex>().EntityHolding.GetComponent<Character>())
            {
                if (characterSelected != null) { characterSelected.HexOn.UnHighlight(); }
                Hit.transform.GetComponent<Hex>().HighlightSelection();
                characterSelected = Hit.transform.GetComponent<Hex>().EntityHolding.GetComponent<Character>();
                FindObjectOfType<CharacterViewer>().ShowCharacterStats(playerController.CharacterName, playerController.characterIcon, characterSelected);
            }
            else
            {
                if (characterSelected != null)
                {
                    FindObjectOfType<CharacterViewer>().HideCharacterStats();
                    FindObjectOfType<CharacterViewer>().HideActionCard();
                    characterSelected.HexOn.UnHighlight();
                }
            }
        }
    }

    public void CheckToMoveOutOfCombat(Character myCharacter)
    {
        RaycastHit Hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out Hit, 100f, MapLayer))
        {
            Hex hexSelected = Hit.transform.GetComponent<Hex>();
            if (!hexSelected.EntityHolding && !myCharacter.GetComponent<CharacterAnimationController>().Moving)
            {
                myCharacter.MoveOnPath(hexSelected);
                return;
            }
        }
    }

    public void UseOutOfCombatAbility(OutOfCombatCard card)
    {
        switch (card.actions[0].thisActionType)
        {
            case OutOfCombatActionType.Scout:
                Scout(card.actions[0].Value);
                break;
            case OutOfCombatActionType.BuffAttack:
                BuffAttack(card.actions[0].Value, card.actions[0].Duration);
                break;
            case OutOfCombatActionType.BuffMove:
                BuffMove(card.actions[0].Value, card.actions[0].Duration);
                break;
            case OutOfCombatActionType.BuffRange:
                BuffRange(card.actions[0].Value, card.actions[0].Duration);
                break;
            case OutOfCombatActionType.BuffArmor:
                BuffArmor(card.actions[0].Value, card.actions[0].Duration);
                break;
        }
    }

    void BuffRange(int value, int duration)
    {
        myCharacter.ApplyBuff(value, duration, BuffType.Dexterity);
    }

    void BuffMove(int value, int duration)
    {
        myCharacter.ApplyBuff(value, duration, BuffType.Agility);
    }

    void BuffAttack(int value, int duration)
    {
        myCharacter.ApplyBuff(value, duration, BuffType.Strength);
    }

    void BuffArmor(int value, int duration)
    {
        myCharacter.ApplyBuff(value, duration, BuffType.Armor);
    }

    void Scout(int value)
    {
        myCharacter.ShowViewAreaAndCheckToFight(myCharacter.HexOn, myCharacter.ViewDistance + value);
    }

    // Update is called once per frame
    void Update () {
		
	}
}

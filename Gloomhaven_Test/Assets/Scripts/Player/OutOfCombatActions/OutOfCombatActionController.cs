using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfCombatActionController : MonoBehaviour {

    private LayerMask MapLayer;
    private PlayerController playerController;
    //private Character myCharacter;
    private Character characterSelected;

    public OutOfCombatCard cardUsing = null;

    public bool MovingIntoCombat;
    public void FinishedMovingIntoCombat() { MovingIntoCombat = false; }

    // Use this for initialization
    void Start () {
        playerController = GetComponent<PlayerController>();
        MapLayer = playerController.MapLayer;
        //myCharacter = playerController.myCharacter;
    }

    public void UnHighlightHexes()
    {
        Hex[] hexes = FindObjectOfType<HexMapController>().AllHexes;
        foreach (Hex hex in hexes)
        {
            if (hex.HexNode.Shown)
            {
                hex.returnToPreviousColor();
            }
        }
    }

    void HighlightHexOver()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, 100, playerController.MapLayer);
        if (raycastHits.Length == 0) { return; }
        foreach (RaycastHit hit in raycastHits)
        {
            if (hit.transform.GetComponent<Hex>())
            {
                FindObjectOfType<HexVisualizer>().HighlightMovePath(hit.transform.GetComponent<Hex>());
                return;
            }
        }
    }

    public void FinishedMoving()
    {
        //UnHighlightHexes();
        playerController.SelectPlayerCharacter.Selected();
        //HighlightHexOver();
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
            else if (Hit.transform.GetComponent<Hex>().EntityHolding != null && Hit.transform.GetComponent<Hex>().EntityHolding.GetComponent<PlayerCharacter>())
            {
                if (characterSelected != null) { characterSelected.HexOn.UnHighlight(); }
                Hit.transform.GetComponent<Hex>().HighlightSelection();
                characterSelected = Hit.transform.GetComponent<Hex>().EntityHolding.GetComponent<Character>();
                FindObjectOfType<CharacterViewer>().ShowCharacterStats(characterSelected.GetComponent<PlayerCharacter>().CharacterName, characterSelected.GetComponent<PlayerCharacter>().characterIcon, characterSelected);
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
        if (myCharacter.Moving) { return; }
        RaycastHit Hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out Hit, 100f, MapLayer))
        {
            Hex hexSelected = Hit.transform.GetComponent<Hex>();
            if (hexSelected == null || !hexSelected.HexNode.Shown) { return; }
            if (!hexSelected.EntityHolding && !hexSelected.MovedTo)
            {
                if (hexSelected.InEnemySeight) { MovingIntoCombat = true; }
                myCharacter.MoveOnPath(hexSelected);
                UnHighlightHexes();
                return;
            }
        }
    }

    public void UseAction(PlayerCharacter character)
    {
        if (cardUsing == null)
        {
            CheckToMoveOutOfCombat(character);
        }
        else
        {
            UseOutOfCombatAbility(character);
        }
    }

    public void ShowOutOfCombatAbility(OutOfCombatCard card)
    {
        cardUsing = card;
    }

    public void UseOutOfCombatAbility(PlayerCharacter character)
    {
        RaycastHit Hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Hex hexSelected = null;
        if (Physics.Raycast(ray, out Hit, 100f, MapLayer))
        {
            if (Hit.transform.GetComponent<Hex>() != null)
            {
                hexSelected = Hit.transform.GetComponent<Hex>();
            }
        }

        if (hexSelected == null) { return; }

        switch (cardUsing.actions[0].thisActionType)
        {
            case OutOfCombatActionType.Scout:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    Scout(cardUsing.actions[0].Value);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case OutOfCombatActionType.Stealth:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    Stealth(cardUsing.actions[0].Value);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case OutOfCombatActionType.BuffAttack:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    BuffAttack(cardUsing.actions[0].Value, cardUsing.actions[0].Duration);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case OutOfCombatActionType.BuffMove:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    BuffMove(cardUsing.actions[0].Value, cardUsing.actions[0].Duration);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case OutOfCombatActionType.BuffRange:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    BuffRange(cardUsing.actions[0].Value, cardUsing.actions[0].Duration);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case OutOfCombatActionType.BuffArmor:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    BuffArmor(cardUsing.actions[0].Value, cardUsing.actions[0].Duration);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case OutOfCombatActionType.Heal:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    Heal(cardUsing.actions[0].Value);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
        }
    }

    void Heal(int value)
    {
        playerController.SelectPlayerCharacter.Heal(value);
    }

    void BuffRange(int value, int duration)
    {
        playerController.SelectPlayerCharacter.ApplyBuff(value, duration, BuffType.Dexterity);
    }

    void BuffMove(int value, int duration)
    {
        playerController.SelectPlayerCharacter.ApplyBuff(value, duration, BuffType.Agility);
    }

    void BuffAttack(int value, int duration)
    {
        playerController.SelectPlayerCharacter.ApplyBuff(value, duration, BuffType.Strength);
    }

    void BuffArmor(int value, int duration)
    {
        playerController.SelectPlayerCharacter.ApplyBuff(value, duration, BuffType.Armor);
    }

    void Scout(int value)
    {
        playerController.SelectPlayerCharacter.ShowViewArea(playerController.SelectPlayerCharacter.HexOn, playerController.SelectPlayerCharacter.ViewDistance + value);
    }

    void Stealth(int value)
    {
        playerController.SelectPlayerCharacter.Stealth(value);
    }
}

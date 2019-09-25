using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfCombatActionController : MonoBehaviour {

    private CharacterSelectionButtonManager CSBM;
    private PlayerController playerController;
    private CameraRaycaster raycaster;
    private HexVisualizer hexVisualizer;
    private MyCameraController myCamera;
    //private Character myCharacter;
    private Character characterSelected;

    public OutOfCombatCard cardUsing = null;

    public bool MovingIntoCombat;
    public void FinishedMovingIntoCombat() { MovingIntoCombat = false; }

    public bool LookingInChest = false;
    public void StopLookingInChest() { LookingInChest = false; }

    // Use this for initialization
    void Start () {
        CSBM = GetComponent<CharacterSelectionButtonManager>();
        playerController = GetComponent<PlayerController>();
        raycaster = FindObjectOfType<CameraRaycaster>();
        hexVisualizer = FindObjectOfType<HexVisualizer>();
        myCamera = FindObjectOfType<MyCameraController>();
    }

    public void UnHighlightHexes()
    {
        hexVisualizer.ReturntHexesToPreviousColor();
    }

    void HighlightHexOver()
    {
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            hexVisualizer.HighlightMovePath(HexHit.GetComponent<Hex>());
            return;
        }
    }

    public void FinishedMoving()
    {
        playerController.SelectPlayerCharacter.Selected();
    }

    public void CheckToShowCharacterStats()
    {
        playerController.CheckToSelectCharacter();
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hex = HexHit.GetComponent<Hex>();
            if (hex.EntityHolding != null && hex.EntityHolding.GetComponent<EnemyCharacter>())
            {
                if (characterSelected != null) { hexVisualizer.UnHighlightHex(characterSelected.HexOn); }
                hexVisualizer.HighlightSelectionHex(hex);
                characterSelected = hex.EntityHolding.GetComponent<Character>();
                EnemyCharacter character = hex.EntityHolding.GetComponent<EnemyCharacter>();
                FindObjectOfType<CharacterViewer>().ShowCharacterStats(character.CharacterName, character.enemySprite, character);
            }
            else if (hex.EntityHolding != null && hex.EntityHolding.GetComponent<PlayerCharacter>())
            {
                if (characterSelected != null) { hexVisualizer.UnHighlightHex(characterSelected.HexOn); }
                hexVisualizer.HighlightSelectionHex(hex);
                characterSelected = hex.EntityHolding.GetComponent<Character>();
                FindObjectOfType<CharacterViewer>().ShowCharacterStats(characterSelected.GetComponent<PlayerCharacter>().CharacterName, characterSelected.GetComponent<PlayerCharacter>().characterIcon, characterSelected);
            }
            else
            {
                if (characterSelected != null)
                {
                    FindObjectOfType<CharacterViewer>().HideCharacterStats();
                    FindObjectOfType<CharacterViewer>().HideActionCard();
                    hexVisualizer.UnHighlightHex(characterSelected.HexOn);
                }
            }
        }
    }

    public void SelectCard(Card card)
    {
        if (LookingInChest)
        {
            FindObjectOfType<ChestPanel>().CardSelected(card);
        }
        else
        {
          ShowOutOfCombatAbility((OutOfCombatCard)card);
        }
    }

    public void SelectCharacter(PlayerCharacter playerCharacter)
    {
        if (MovingIntoCombat || LookingInChest) { return; }
        if (playerCharacter.GetMoving()) { return; }

        UnHighlightHexes();
        if (playerController.SelectPlayerCharacter != null) {
            cardUsing = null;
            playerController.SelectPlayerCharacter.GetMyOutOfCombatHand().UnSelectCard();
            playerController.SelectPlayerCharacter.myDecks.SetActive(false);
        }
        playerController.SelectPlayerCharacter = playerCharacter;

        myCamera.SetTarget(playerCharacter.transform);

        hexVisualizer.HighlightSelectionHex(playerCharacter.HexOn);
        hexVisualizer.ResetLastHex();

        playerCharacter.myDecks.SetActive(true);
        playerCharacter.GetMyCombatHand().HideHand();
        playerCharacter.GetMyOutOfCombatHand().ShowHand();
        playerCharacter.myCharacterSelectionButton.CharacterSelected();
    }

    public void CheckToMoveOrInteractOutOfCombat(Character myCharacter)
    {
        if (myCharacter.GetMoving()) { return; }
        Transform InterActionHit = raycaster.InteractableRaycast();
        if (InterActionHit != null)
        {
            if (InterActionHit.GetComponent<DoorObject>() != null && !InterActionHit.GetComponent<DoorObject>().door.isOpen)
            {
                MoveToDoor((PlayerCharacter)myCharacter, InterActionHit.GetComponent<DoorObject>().door);
                UnHighlightHexes();
                return;
            }
            else if (InterActionHit.GetComponent<CardChest>() != null && !InterActionHit.GetComponent<CardChest>().isOpen)
            {
                CheckToMoveNearChest((PlayerCharacter)myCharacter, InterActionHit.GetComponent<Entity>().HexOn);
                UnHighlightHexes();
                return;
            }
        }
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hexSelected = HexHit.GetComponent<Hex>();
            if (hexSelected == null || !hexSelected.HexNode.Shown) { return; }
            if (hexSelected.GetComponent<Door>() != null && !hexSelected.GetComponent<Door>().CanMoveOnDoor()) { return; }
            if (!hexSelected.EntityHolding && !hexSelected.MovedTo)
            {
                if (hexSelected.InEnemySeight) { MovingIntoCombat = true; }
                myCharacter.MoveOnPath(hexSelected);
                UnHighlightHexes();
                return;
            }
        }
        
    }

    public void CheckToMoveNearChest(PlayerCharacter myCharacter, Hex chestHex)
    {
        Node closestNode = FindObjectOfType<HexMapController>().GetClosestNodeFromNeighbors(chestHex, myCharacter);
        if (closestNode == myCharacter.HexOn.HexNode) {
            LookingInChest = true;
            chestHex.EntityHolding.GetComponent<CardChest>().OpenChest(myCharacter);
        }
        else if (closestNode != null)
        {
            LookingInChest = true;
            myCharacter.SetChestToOpen(chestHex.EntityHolding.GetComponent<CardChest>());
            myCharacter.MoveOnPath(closestNode.NodeHex);
        }
    }

    public void MoveToDoor(PlayerCharacter myCharacter, Door doorHex)
    {
        if (myCharacter.GetMoving()) { return; }
        if (doorHex.GetComponent<doorConnectionHex>() != null)
        {
            CheckToMoveToDoor(myCharacter, doorHex);
        }
        else
        {
            CheckToMoveNearDoor(myCharacter, doorHex);
        }
    }

    public void CheckToMoveToDoor(PlayerCharacter myCharacter, Door doorHex)
    {
        if (myCharacter.HexOn == doorHex.GetComponent<Hex>())
        {
            myCharacter.OpenDoor();
            return;
        }
        if (doorHex.GetComponent<Hex>().EntityHolding == null && !doorHex.GetComponent<Hex>().MovedTo)
        {
            myCharacter.SetDoorToOpen(doorHex);
            myCharacter.MoveOnPath(doorHex.GetComponent<Hex>());
        }
    }

    public void CheckToMoveNearDoor(PlayerCharacter myCharacter, Door doorHex)
    {
        Node closestNode= FindObjectOfType<HexMapController>().GetClosestNodeFromNeighbors(doorHex.GetComponent<Hex>(), myCharacter);
        if (closestNode == myCharacter.HexOn.HexNode) { myCharacter.OpenDoor(); }
        else if (closestNode != null)
        {
            myCharacter.SetDoorToOpen(doorHex);
            myCharacter.MoveOnPath(closestNode.NodeHex);
        }
    }

    public void UseAction(PlayerCharacter character)
    {
        if (LookingInChest) { return; }
        if (cardUsing == null)
        {
            CheckToMoveOrInteractOutOfCombat(character);
        }
        else
        {
            UseOutOfCombatAbility(character);
        }
    }

    public void ShowOutOfCombatAbility(OutOfCombatCard card)
    {
        if (cardUsing != card) { cardUsing = card; }
        else{ cardUsing = null; }
    }

    public void UseOutOfCombatAbility(PlayerCharacter character)
    {
        Transform HexHit = raycaster.HexRaycast();
        Hex hexSelected = null;
        if (HexHit != null && HexHit.GetComponent<Hex>()){ hexSelected = HexHit.GetComponent<Hex>(); }
        if (hexSelected == null) { return; }

        Action action = cardUsing.cardAbility.Actions[0];
        switch (action.thisActionType)
        {
            case ActionType.Scout:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    Scout(action.Range);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case ActionType.Stealth:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    Stealth(action.Duration);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case ActionType.BuffAttack:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    BuffAttack(action.thisAOE.Damage, action.Duration);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case ActionType.BuffMove:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    BuffMove(action.thisAOE.Damage, action.Duration);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case ActionType.BuffRange:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    BuffRange(action.thisAOE.Damage, action.Duration);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case ActionType.BuffArmor:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    BuffArmor(action.thisAOE.Damage, action.Duration);
                    character.GetMyOutOfCombatHand().DiscardSelectedCard();
                    cardUsing = null;
                }
                break;
            case ActionType.Heal:
                if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
                {
                    Heal(action.thisAOE.Damage);
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

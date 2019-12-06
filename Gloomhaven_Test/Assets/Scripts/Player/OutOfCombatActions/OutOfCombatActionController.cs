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
        Transform WallHit = raycaster.WallRaycast();
        if (WallHit != null)
        {
            if (WallHit.GetComponent<DoorObject>() != null && !WallHit.GetComponent<DoorObject>().door.isOpen)
            {
                MoveToDoor((PlayerCharacter)myCharacter, WallHit.GetComponent<DoorObject>().door);
                UnHighlightHexes();
                return;
            }
        }

        Transform InterActionHit = raycaster.InteractableRaycast();
        if (InterActionHit != null)
        {
            if (InterActionHit.GetComponent<CardChest>() != null && !InterActionHit.GetComponent<CardChest>().isOpen)
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
            if (hexSelected.GetComponent<Door>() != null && !hexSelected.GetComponent<Door>().isOpen) {
                if (((PlayerCharacter)myCharacter).CharacterLeading != null)
                {
                    FindObjectOfType<CharacterSelectionButtons>().MoveCharacterOutOfFollow(((PlayerCharacter)myCharacter).myCharacterSelectionButton);
                }
                MoveToDoor((PlayerCharacter)myCharacter, hexSelected.GetComponent<Door>());
                return;
            }
            if (!hexSelected.EntityHolding && !hexSelected.MovedTo)
            {
                if (hexSelected.InEnemySeight) { MovingIntoCombat = true; }
                if (((PlayerCharacter)myCharacter).CharacterLeading != null)
                {
                    FindObjectOfType<CharacterSelectionButtons>().MoveCharacterOutOfFollow(((PlayerCharacter)myCharacter).myCharacterSelectionButton);
                }
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
        if (cardUsing != card) {
            cardUsing = card;
            PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
            myCharacter.ShowAction(cardUsing.cardAbility.Actions[0].Range, cardUsing.cardAbility.Actions[0].thisActionType);
        }
        else {
            cardUsing = null;
            UnHighlightHexes();
        }
    }

    public void UseOutOfCombatAbility(PlayerCharacter character)
    {
        Transform HexHit = raycaster.HexRaycast();
        Hex hexSelected = null;
        if (HexHit != null && HexHit.GetComponent<Hex>()) { hexSelected = HexHit.GetComponent<Hex>(); }
        if (hexSelected == null) { return; }

        Action action = cardUsing.cardAbility.Actions[0];
        bool success = false;
        if (action.thisActionType == ActionType.Scout)
        {
            if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
            {
                success = true;
                Scout(action.Range);
            }
        }
        else if (action.thisActionType == ActionType.Stealth)
        {
            if (hexSelected.EntityHolding != null && hexSelected.EntityHolding == character)
            {
                success = true;
                Stealth(action.Duration);
            }
        }
        else if (action.thisActionType == ActionType.Attack)
        {
            List<Character> charactersActingOn = CheckForNegativeAction(action, character, hexSelected);
            if (charactersActingOn.Count != 0)
            {
                success = true;
                PerformAction(action, charactersActingOn);
            }
        }
        else
        {
            List<Character> charactersActingOn = CheckForPositiveAction(action, character, hexSelected);
            if (charactersActingOn.Count != 0)
            {
                success = true;
                PerformAction(action, charactersActingOn);
            }
        }

        if (success)
        {
            character.GetMyOutOfCombatHand().DiscardSelectedCard();
            cardUsing = null;
        }
    }

    void PerformAction(Action action, List<Character> characters)
    {
        switch (action.thisActionType)
        {
            case ActionType.Attack:
                Attack(action.thisAOE.Damage, characters);
                break;
            case ActionType.Heal:
                Heal(action.thisAOE.Damage, characters);
                break;
            case ActionType.BuffArmor:
                BuffArmor(action.thisAOE.Damage, action.Duration, characters);
                break;
            case ActionType.BuffAttack:
                BuffAttack(action.thisAOE.Damage, action.Duration, characters);
                break;
            case ActionType.BuffMove:
                BuffMove(action.thisAOE.Damage, action.Duration, characters);
                break;
            case ActionType.BuffRange:
                BuffRange(action.thisAOE.Damage, action.Duration, characters);
                break;
        }
    }

    List<Character> CheckForNegativeAction(Action action, Character character, Hex hexSelected)
    {
        List<Node> nodes = FindObjectOfType<HexMapController>().GetAOE(action.thisAOE.thisAOEType, character.HexOn.HexNode, hexSelected.HexNode);
        List<Character> characterActingUpon = new List<Character>();

        if (!character.HexInActionRange(hexSelected)) { return null; }
        foreach (Node node in nodes)
        {
            if (node == null) { break; }
            if (character.HexNegativeActionable(node.NodeHex))
            {
                UnHighlightHexes();
                foreach (Node node_highlight in nodes)
                {
                    hexVisualizer.HighlightActionPointHex(node_highlight.NodeHex, action.thisActionType);
                }
                characterActingUpon.Add(node.NodeHex.EntityHolding.GetComponent<Character>());
            }
        }
        return characterActingUpon;
    }

    List<Character> CheckForPositiveAction(Action action, Character character, Hex hexSelected)
    {
        List<Node> nodes = FindObjectOfType<HexMapController>().GetAOE(action.thisAOE.thisAOEType, character.HexOn.HexNode, hexSelected.HexNode);
        List<Character> characterActingUpon = new List<Character>();

        if (!character.HexInActionRange(hexSelected)) { return null; }
        foreach (Node node in nodes)
        {
            if (node == null) { break; }
            if (character.HexPositiveActionable(node.NodeHex))
            {
                UnHighlightHexes();
                foreach (Node node_highlight in nodes)
                {
                    hexVisualizer.HighlightActionPointHex(node_highlight.NodeHex, action.thisActionType);
                }
                characterActingUpon.Add(node.NodeHex.EntityHolding.GetComponent<Character>());
            }
        }
        return characterActingUpon;
    }

    void Attack(int value, List<Character> characters)
    {
        playerController.SelectPlayerCharacter.Attack(value, characters);
    }

    void Heal(int value, List<Character> characters)
    {
        playerController.SelectPlayerCharacter.PerformHeal(value, characters);
    }

    void BuffRange(int value, int duration, List<Character> characters)
    {
        playerController.SelectPlayerCharacter.GiveBuff(value, duration, BuffType.Dexterity, characters);
    }

    void BuffMove(int value, int duration, List<Character> characters)
    {
        playerController.SelectPlayerCharacter.GiveBuff(value, duration, BuffType.Agility, characters);
    }

    void BuffAttack(int value, int duration, List<Character> characters)
    {
        playerController.SelectPlayerCharacter.GiveBuff(value, duration, BuffType.Strength, characters);
    }

    void BuffArmor(int value, int duration, List<Character> characters)
    {
        playerController.SelectPlayerCharacter.GiveBuff(value, duration, BuffType.Armor, characters);
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

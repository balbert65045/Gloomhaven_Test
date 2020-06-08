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

    //public void EndTurn()
    //{
    //    foreach(PlayerCharacter character in playerController.myCharacters)
    //    {
    //        character.RefreshActions();
    //        character.DecreaseBuffsDuration();
    //    }
    //    playerController.SelectPlayerCharacter.Selected();
    //}

    public void FinishedMoving(PlayerCharacter character)
    {
        if (NoOtherPlayerMoving()) { playerController.AllowEndTurn(); }
        if (character != playerController.SelectPlayerCharacter) { return; }
        playerController.SelectPlayerCharacter.Selected();
    }

    bool NoOtherPlayerMoving()
    {
        foreach(PlayerCharacter character in playerController.myCharacters)
        {
            if (character.GetMoving()) { return false; }
        }
        return true;
    }

    public void FinishedAction(PlayerCharacter character)
    {
        if (character != playerController.SelectPlayerCharacter) { return; }
        if (playerController.SelectPlayerCharacter.GetMoving()) { return; }
        playerController.SelectPlayerCharacter.Selected();
    }

    public void CheckToShowCharacterStats()
    {
        playerController.CheckToSelectCharacter();
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hex = HexHit.GetComponent<Hex>();
            if (hex.EntityHolding != null && hex.EntityHolding.GetComponent<Character>())
            {
                if (characterSelected != null) { hexVisualizer.UnHighlightHex(characterSelected.HexOn); }
                hexVisualizer.HighlightSelectionHex(hex);
                characterSelected = hex.EntityHolding.GetComponent<Character>();
                characterSelected.ShowStats();
            }
            else
            {
                if (characterSelected != null)
                {
                    FindObjectOfType<CharacterViewer>().HideCharacterCards();
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
            if (playerController.SelectPlayerCharacter.InCombat())
            {
                playerController.SelectPlayerCharacter.GetMyCombatHand().UnShowCard();
                playerController.SelectPlayerCharacter.myCombatZone.HideInitiativeBoard();
            }
            else
            {
                cardUsing = null;
                playerController.SelectPlayerCharacter.GetMyOutOfCombatHand().UnSelectCard();
            }
            playerController.SelectPlayerCharacter.myDecks.SetActive(false);
        }
        playerController.SelectPlayerCharacter = playerCharacter;

        myCamera.SetTarget(playerCharacter.transform);

        playerCharacter.myDecks.SetActive(true);
        playerCharacter.GetMyCombatHand().HideHand();
        playerCharacter.GetMyOutOfCombatHand().ShowHand();
        if (playerCharacter.GetMyCombatHand().DiscardedCards > 0) { playerCharacter.GetMyOutOfCombatHand().allowLongRest(); }
        playerCharacter.myCharacterSelectionButton.CharacterSelected();
        playerCharacter.ShowStats();
        playerCharacter.Selected();
        CombatZone[] combatZones = FindObjectsOfType<CombatZone>();
        foreach(CombatZone combatZone in combatZones)
        {
            combatZone.ShowZone();
        }
    }

    public bool CheckToMoveOrInteractOutOfCombat(Character myCharacter)
    {
        if (myCharacter.GetMoving()) { return false; }
        Transform WallHit = raycaster.WallRaycast();
        if (WallHit != null)
        {
            if (WallHit.GetComponent<DoorObject>() != null && !WallHit.GetComponent<DoorObject>().door.isOpen)
            {
                MoveToDoor((PlayerCharacter)myCharacter, WallHit.GetComponent<DoorObject>().door);
                return true;
            }
        }

        Transform InterActionHit = raycaster.InteractableRaycast();
        if (InterActionHit != null)
        {
            if (InterActionHit.GetComponent<CardChest>() != null && !InterActionHit.GetComponent<CardChest>().isOpen)
            {
                CheckToMoveNearChest((PlayerCharacter)myCharacter, InterActionHit.GetComponent<Entity>().HexOn);
                return true;
            }
        }

        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hexSelected = HexHit.GetComponent<Hex>();
            if (hexSelected == null || !hexSelected.HexNode.Shown) { return false; }
            if (!myCharacter.HexInMoveRange(hexSelected, myCharacter.CurrentMoveDistance)) { return false; }
            if (hexSelected.GetComponent<Door>() != null && !hexSelected.GetComponent<Door>().isOpen) {
                if (((PlayerCharacter)myCharacter).CharacterLeading != null)
                {
                    FindObjectOfType<CharacterSelectionButtons>().MoveCharacterOutOfFollow(((PlayerCharacter)myCharacter).myCharacterSelectionButton);
                }
                MoveToDoor((PlayerCharacter)myCharacter, hexSelected.GetComponent<Door>());
                return true;
            }
            if (hexSelected.EntityHolding == null && !hexSelected.MovedTo)
            {
                if (hexSelected.InThreatArea()) { MovingIntoCombat = true; }
                if (((PlayerCharacter)myCharacter).CharacterLeading != null) { FindObjectOfType<CharacterSelectionButtons>().MoveCharacterOutOfFollow(((PlayerCharacter)myCharacter).myCharacterSelectionButton); }
                myCharacter.MoveOnPath(hexSelected);
                return true;
            }
        }
        return false;      
    }

    public void CheckToMoveNearChest(PlayerCharacter myCharacter, Hex chestHex)
    {
        Node closestNode = FindObjectOfType<HexMapController>().GetClosestNodeFromNeighbors(chestHex, myCharacter);
        if (closestNode == myCharacter.HexOn.HexNode) {
            LookingInChest = true;
            myCharacter.ActionUsed();
            chestHex.EntityHolding.GetComponent<CardChest>().OpenChest();
        }
        else if (closestNode != null)
        {
            if (!myCharacter.HexInMoveRange(closestNode.NodeHex, myCharacter.CurrentMoveDistance)) { return; }
            LookingInChest = true;
            myCharacter.SetChestToOpen(chestHex.EntityHolding.GetComponent<CardChest>());
            myCharacter.MoveOnPath(closestNode.NodeHex);
        }
    }

    public void MoveToDoor(PlayerCharacter myCharacter, Door doorHex)
    {
        if (myCharacter.GetMoving()) { return; }
        if (!myCharacter.HexInMoveRange(doorHex.GetComponent<Hex>(), myCharacter.CurrentMoveDistance)) { return; }
        if (doorHex.GetComponent<Hex>().EntityHolding == null && !doorHex.GetComponent<Hex>().MovedTo)
        {
            myCharacter.SetDoorToOpen(doorHex);
            myCharacter.MoveOnPath(doorHex.GetComponent<Hex>());
        }
    }

    public Action GetMyCurrentAction()
    {
        return cardUsing.cardAbility.Actions[0];
    }

    public void UseAction(PlayerCharacter character)
    {
        if (LookingInChest) { return; }
        if (cardUsing == null)
        {
            if (CheckToMoveOrInteractOutOfCombat(character))
            {
                UnHighlightHexes();
                playerController.RemoveArea();
                playerController.DisableEndTurn();
                playerController.DisableExit();
            }
        }
        else
        {
            UseOutOfCombatAbility(character);
        }
    }

    public void ShowOutOfCombatAbility(OutOfCombatCard card)
    {
        if (cardUsing != card) {
            UnHighlightHexes();
            cardUsing = card;
            PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
            myCharacter.ShowAction(cardUsing.cardAbility.Actions[0].Range, cardUsing.cardAbility.Actions[0].thisActionType);
        }
        else {
            cardUsing = null;
            playerController.SelectPlayerCharacter.Selected();
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
            if (cardUsing.cardAbility.LostAbility) { cardUsing.LostAbilityUsed = true; }
            character.GetMyOutOfCombatHand().DiscardSelectedCard();
            cardUsing = null;
            character.ActionUsed();
        }
    }

    void PerformAction(Action action, List<Character> characters)
    {
        playerController.RemoveArea();
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
        playerController.SelectPlayerCharacter.GetComponent<CharacterAnimationController>().DoBuff(ActionType.Heal, value, 0, characters);
    }

    void BuffRange(int value, int duration, List<Character> characters)
    {
        playerController.SelectPlayerCharacter.GetComponent<CharacterAnimationController>().DoBuff(ActionType.BuffRange, value, duration, characters);
    }

    void BuffMove(int value, int duration, List<Character> characters)
    {
        playerController.SelectPlayerCharacter.GetComponent<CharacterAnimationController>().DoBuff(ActionType.BuffMove, value, duration, characters);
    }

    void BuffAttack(int value, int duration, List<Character> characters)
    {
        playerController.SelectPlayerCharacter.GetComponent<CharacterAnimationController>().DoBuff(ActionType.BuffAttack, value, duration, characters);
    }

    void BuffArmor(int value, int duration, List<Character> characters)
    {
        playerController.SelectPlayerCharacter.GetComponent<CharacterAnimationController>().DoBuff(ActionType.BuffArmor, value, duration, characters);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour {

    public List<PlayerCharacter> myCharacters = new List<PlayerCharacter>();
    public PlayerCharacter SelectPlayerCharacter;

    private OutOfCombatActionController outOfCombatController;
    private CombatActionController combatController;
    private EndTurnButton endTurnButton;
    private CharacterSelectionButtonManager CSBM;
    private PlayerActionButton actionButton;
    private InitiativeBoard initBoard;
    private MyCameraController myCamera;
    private CameraRaycaster raycaster;
    private HexVisualizer hexVisualizer;

    public List<Hex> HexesMovingTo = new List<Hex>();
    public void AddHexMovingTo(Hex hex) { HexesMovingTo.Add(hex); }
    public void ClearHexesMovingTo() { HexesMovingTo.Clear(); }

    public int GoldHolding = 0;

    public bool CardsPlayable = true;
    public Action StagedAction;
    public void ShowStagedAction(Action action)
    {
        StagedAction = action;
        if (action.thisActionType == ActionType.None)
        {
            SelectPlayerCharacter.ClearActions();
            RemoveArea();
        }
        else if (action.thisActionType == ActionType.Movement)
        {
            SelectPlayerCharacter.ShowMoveDistance(action.Range);
            SelectPlayerCharacter.ShowActionOnHealthBar(action);
        }
        else
        {
            SelectPlayerCharacter.ShowAction(action.Range, action.thisActionType);
            SelectPlayerCharacter.ShowActionOnHealthBar(action);
        }
    }

    public void BeginGame()
    {
        StartCoroutine("StartGame");
    }

    private void Start()
    {
        hexVisualizer = FindObjectOfType<HexVisualizer>();
        raycaster = FindObjectOfType<CameraRaycaster>();
        myCamera = FindObjectOfType<MyCameraController>();
        combatController = GetComponent<CombatActionController>();
        outOfCombatController = GetComponent<OutOfCombatActionController>();
        CSBM = FindObjectOfType<CharacterSelectionButtonManager>();
        endTurnButton = FindObjectOfType<EndTurnButton>();
        actionButton = FindObjectOfType<PlayerActionButton>();
        initBoard = FindObjectOfType<InitiativeBoard>();


        //..
        myCamera.SetTarget(SelectPlayerCharacter.transform);
        //actionButton.gameObject.SetActive(false);
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(.5f);
        myCharacters.Clear();
        myCharacters.AddRange(FindObjectsOfType<PlayerCharacter>());
        FindObjectOfType<PlayerCurrency>().SetGoldValue(GoldHolding);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            UseAction();
        }
    }

    void UseAction()
    {
        if (StagedAction.thisActionType == ActionType.None) { return; }
        Transform HexHit = raycaster.HexRaycast();
        Hex hexSelected = null;
        if (HexHit != null && HexHit.GetComponent<Hex>()) { hexSelected = HexHit.GetComponent<Hex>(); }
        if (hexSelected == null) { return; }
        bool usingAction = false;
        if (StagedAction.thisActionType == ActionType.Movement)
        {
            usingAction = CheckToMoveOrInteract();
        }
        else if (StagedAction.thisActionType == ActionType.Attack)
        {
            List<Character> charactersActingOn = CheckForNegativeAction(StagedAction, SelectPlayerCharacter, hexSelected);
            if (charactersActingOn.Count != 0)
            {
                usingAction = true;
                PerformAction(StagedAction, charactersActingOn);
            }
        }
        else
        {
            List<Character> charactersActingOn = CheckForPositiveAction(StagedAction, SelectPlayerCharacter, hexSelected);
            if (charactersActingOn.Count != 0)
            {
                usingAction = true;
                PerformAction(StagedAction, charactersActingOn);
            }
        }

        if (usingAction)
        {
            DisableEndTurn();
        }
    }

    bool CheckToMoveOrInteract()
    {
        if (SelectPlayerCharacter.GetMoving()) { return false; }
        Transform WallHit = raycaster.WallRaycast();
        if (WallHit != null)
        {
            if (WallHit.GetComponent<DoorObject>() != null && !WallHit.GetComponent<DoorObject>().door.isOpen)
            {
                MoveToDoor(WallHit.GetComponent<DoorObject>().door);
                return true;
            }
        }

        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hexSelected = HexHit.GetComponent<Hex>();
            if (hexSelected == null || !hexSelected.HexNode.Shown) { return false; }
            if (!SelectPlayerCharacter.HexInMoveRange(hexSelected, StagedAction.Range)) { return false; }
            if (hexSelected.GetComponent<Door>() != null && !hexSelected.GetComponent<Door>().isOpen)
            {
                MoveToDoor(hexSelected.GetComponent<Door>());
                return true;
            }
            if (hexSelected.EntityHolding == null && !hexSelected.MovedTo)
            {
                myCamera.SetTarget(SelectPlayerCharacter.transform);
                SelectPlayerCharacter.MoveOnPath(hexSelected);
                return true;
            }
        }
        return false;
    }

    void MoveToDoor(Door doorHex)
    {
        if (SelectPlayerCharacter.GetMoving()) { return; }
        if (!SelectPlayerCharacter.HexInMoveRange(doorHex.GetComponent<Hex>(), StagedAction.Range)) { return; }
        if (doorHex.GetComponent<Hex>().EntityHolding == null && !doorHex.GetComponent<Hex>().MovedTo)
        {
            myCamera.SetTarget(SelectPlayerCharacter.transform);
            SelectPlayerCharacter.SetDoorToOpen(doorHex);
            SelectPlayerCharacter.MoveOnPath(doorHex.GetComponent<Hex>());
        }
    }

    void PerformAction(Action action, List<Character> characters)
    {
        RemoveArea();
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

    void Attack(int value, List<Character> characters)
    {
        SelectPlayerCharacter.Attack(value, characters);
    }

    void Heal(int value, List<Character> characters)
    {
        SelectPlayerCharacter.GetComponent<CharacterAnimationController>().DoBuff(ActionType.Heal, value, 0, characters);
    }

    void BuffRange(int value, int duration, List<Character> characters)
    {
        SelectPlayerCharacter.GetComponent<CharacterAnimationController>().DoBuff(ActionType.BuffRange, value, duration, characters);
    }

    void BuffMove(int value, int duration, List<Character> characters)
    {
        SelectPlayerCharacter.GetComponent<CharacterAnimationController>().DoBuff(ActionType.BuffMove, value, duration, characters);
    }

    void BuffAttack(int value, int duration, List<Character> characters)
    {
        SelectPlayerCharacter.GetComponent<CharacterAnimationController>().DoBuff(ActionType.BuffAttack, value, duration, characters);
    }

    void BuffArmor(int value, int duration, List<Character> characters)
    {
        SelectPlayerCharacter.GetComponent<CharacterAnimationController>().DoBuff(ActionType.BuffArmor, value, duration, characters);
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

    public void AddGold(int gold)
    {
        GoldHolding += gold;
        FindObjectOfType<PlayerCurrency>().SetGoldValue(GoldHolding);
    }

    public void RemoveGold(int gold)
    {
        GoldHolding -= gold;
        FindObjectOfType<PlayerCurrency>().SetGoldValue(GoldHolding);
    }

    public void RemoveArea()
    {
        GetComponentInChildren<MeshGenerator>().DeleteMesh();
        GetComponentInChildren<EdgeLine>().DestroyLine();
    }

    public void CreateArea(List<Vector3> points, ActionType type)
    {
        GetComponentInChildren<MeshGenerator>().CreateMesh(points);
        GetComponentInChildren<MeshGenerator>().SetCurrentMaterial(type);
        GetComponentInChildren<EdgeLine>().CreateLine(points.ToArray());
        GetComponentInChildren<EdgeLine>().SetCurrentMaterial(type);
    }

    public void SelectCharacterByName(string characterName)
    {
        foreach(PlayerCharacter character in myCharacters)
        {
            if (character.CharacterName == characterName)
            {
                SelectCharacter(character);
            }
        }
    }

    public void AllowNewTurns()
    {
        SelectPlayerCharacter.MyNewDeck.SetActive(true);
        SelectPlayerCharacter.GetMyNewHand().DrawNewHand();
        AllowEndTurn();
    }

    public void EndPlayerTurn()
    {
        SelectPlayerCharacter.GetMyNewHand().DiscardHand();
        SelectPlayerCharacter.MyNewDeck.SetActive(false);
        TurnOrder turnOrder = FindObjectOfType<TurnOrder>();
        turnOrder.EndTurn();
        DisableEndTurn();

        if (turnOrder.GetCurrentCharacter().IsPlayer())
        {
            //Move to next player
        }
        else
        {
            FindObjectOfType<EnemyController>().DoEnemyActions();
        }
    }

    void ActionFinished()
    {
        myCamera.UnLockCamera();
        FindObjectOfType<StagingArea>().DiscardCards();
        AllowEndTurn();
        UnHighlightHexes();
    }

    //Callbacks
    public void FinishedMoving(PlayerCharacter characterFinished)
    {
        ActionFinished();
    }

    public void FinishedAttacking(PlayerCharacter characterFinished)
    {
        ActionFinished();
    }

    public void FinishedBuffing(PlayerCharacter characterFinished)
    {
        if (characterFinished.InCombat()) { GetComponent<CombatActionController>().FinishedBuffing(); }
        else { outOfCombatController.FinishedAction(characterFinished); }
    }

    public void FinishedHealing(PlayerCharacter characterFinished)
    {
        if (characterFinished.InCombat()) { GetComponent<CombatActionController>().FinishedHealing(); }
        else { outOfCombatController.FinishedAction(characterFinished); }
    }

    public void FinishedShielding(PlayerCharacter characterFinished)
    {
        if (characterFinished.InCombat()) { GetComponent<CombatActionController>().FinishedShielding(); }
        else { outOfCombatController.FinishedAction(characterFinished); }
    }

    //Character selection
    public void selectAnotherCharacter()
    {
        for(int i = 0; i < myCharacters.Count; i++)
        {
            if (myCharacters[i] == SelectPlayerCharacter)
            {
                int nextCharindex = i + 1 >= myCharacters.Count ? 0 : i + 1;
                SelectCharacter(myCharacters[nextCharindex]);
                return;
            }
        }
    }

    public void EnemyVanquished(float XP)
    {
        for (int i = 0; i < myCharacters.Count; i++)
        {
            myCharacters[i].GainXP(XP);
        }
    }

    public void selectNextAvailableCharacter()
    {
        foreach (PlayerCharacter character in myCharacters)
        {
            SelectCharacter(character);
        }
    }

    public void SelectCharacter(PlayerCharacter playerCharacter)
    {
       
    }

    public Character CheckIfOverCharacter()
    {
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hex = HexHit.GetComponent<Hex>();
            if (hex.EntityHolding != null && hex.EntityHolding.GetComponent<Character>())
            {
                return hex.EntityHolding.GetComponent<Character>();
            }
        }
        return null;
    }

    public void CheckToSelectCharacter()
    {
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hex = HexHit.GetComponent<Hex>();
            if (hex.EntityHolding != null && hex.EntityHolding.GetComponent<PlayerCharacter>())
            {
                SelectCharacter(hex.EntityHolding.GetComponent<PlayerCharacter>());
            }
        }
    }

    //Group character check
    bool AnyCharacterMoving()
    {
        foreach (PlayerCharacter character in myCharacters)
        {
            if (character.GetMoving()) { return true; }
        }
        return false;
    }

    //Map
    public void UnHighlightHexes() { hexVisualizer.UnhighlightHexes(); }

    public void ExitLevel()
    {
        actionButton.gameObject.SetActive(false);
        Time.timeScale = 0;
        FindObjectOfType<LevelClearedPanel>().TurnOnPanel();
    }

    //End Turn Button
    public void AllowEndTurn()
    {
        CardsPlayable = true;
        SelectPlayerCharacter.GetMyNewHand().MakeAllCardsPlayable();
        endTurnButton.AllowEndTurn();
    }

    public void DisableEndTurn()
    {
        CardsPlayable = false;
        SelectPlayerCharacter.GetMyNewHand().MakeAllCardsUnPlayable();
        endTurnButton.DisableEndTurn();
    }

    public void CharacterDied(PlayerCharacter character)
    {
        myCharacters.Remove(character);
        CSBM.RemoveCharacterButton(character);
        if (myCharacters.Count == 0)
        {
            Time.timeScale = 0;
            FindObjectOfType<LostScreen>().TurnOnPanel();
        }
    }

    public void ChestOpenedFor(List<Card> cards)
    {
        
    }
}

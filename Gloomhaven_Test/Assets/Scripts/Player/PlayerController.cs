using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour {

    public List<PlayerCharacter> myCharacters = new List<PlayerCharacter>();
    public PlayerCharacter SelectPlayerCharacter;

    public enum PlayerState { InCombat, OutofCombat }
    public PlayerState myState;
    public PlayerState GetPlayerState()
    {
        return myState;
    }
    public void ChangePlayerState(PlayerState state)
    {
        myState = state;
    }

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
        //actionButton.gameObject.SetActive(false);
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(.5f);
        myCharacters.Clear();
        myCharacters.AddRange(FindObjectsOfType<PlayerCharacter>());
        FindObjectOfType<PlayerCurrency>().SetGoldValue(GoldHolding);
        CSBM.SetUpCharacterSelectionButtons();
        GoOutOfCombat();
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

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        if (EventSystem.current.IsPointerOverGameObject() == true) {
    //            return;
    //        }
    //        Character characterOver = CheckIfOverCharacter();
    //        if (characterOver != null)
    //        {
    //            if (characterOver.GetComponent<EnemyCharacter>() != null)
    //            {
    //                if (SelectPlayerCharacter.InCombat())
    //                {
    //                    combatController.CheckToSelectCharacter();
    //                }
    //                else
    //                {
    //                    outOfCombatController.CheckToShowCharacterStats();
    //                }
    //            }
    //            else
    //            {
    //                if (characterOver.GetComponent<PlayerCharacter>().InCombat())
    //                {
    //                    combatController.CheckToSelectCharacter();
    //                }
    //                else
    //                {
    //                    outOfCombatController.CheckToShowCharacterStats();
    //                }
    //            }
    //        }
    //    }

    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        if (SelectPlayerCharacter.InCombat())
    //        {
    //            combatController.CheckToUseActions();
    //        }
    //        else
    //        {
    //            outOfCombatController.UseAction(SelectPlayerCharacter);
    //        }
    //    }

    //    if (Input.GetKeyDown(KeyCode.Tab))
    //    {
    //        if (SelectPlayerCharacter.InCombat())
    //        {
    //            combatController.SwitchActionOrSelectCharacter();
    //        } 
    //        else
    //        {
    //            selectAnotherCharacter();
    //        }
    //    }
    //}

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

    public void ChangeCombatState(CombatActionController.CombatState state) { combatController.ChangeCombatState(state); }

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

    // Large Game Actions
    public void SelectCard(Card card)
    {
        if (SelectPlayerCharacter.InCombat())
        {
            combatController.SelectCard(card);
        }
        else
        {
            outOfCombatController.SelectCard(card);
        }
    }

    public void BeginNewTurn()
    {
        CSBM.ShowCharacterSelection();
        foreach (PlayerCharacter character in myCharacters)
        {
            if (!character.InCombat()) {
                character.RefreshActions();
                continue;
            }
            character.myCharacterSelectionButton.showCardIndicators();
            character.DecreaseBuffsDuration();
            character.resetShield(character.GetArmor());
            character.GetMyCombatHand().discardSelectedCard();
            character.SetMyCurrentCombatCard(null);
        }
        combatController.ChangeCombatState(CombatActionController.CombatState.SelectingCombatCards);
        FindObjectOfType<CorruptionController>().TurnPassed();
        SelectCharacter(myCharacters[0]);
    }

    public void BeginActions(PlayerCharacter character)
    {
        SelectPlayerCharacter = character;
        SelectCharacter(character);
        SelectPlayerCharacter.ShowStatsActingWith();
        SelectPlayerCharacter.GetMyCombatHand().ShowSelectedCardToUse();
        CombatPlayerCard card = SelectPlayerCharacter.GetMyCombatHand().getSelectedCard();
        combatController.SetAbilities(card.CardAbility);
    }

    public void PlayerMovedIntoCombat()
    {
        outOfCombatController.FinishedMovingIntoCombat();
        combatController.SelectCharacter(SelectPlayerCharacter);
        DisableEndTurn();
        combatController.UnHighlightHexes();
        RemoveArea();
        hexVisualizer.HighlightSelectionHex(SelectPlayerCharacter.HexOn);
        FindObjectOfType<MyCameraController>().LookAt(SelectPlayerCharacter.transform);
    }

    public void GoIntoCombat()
    {
        if (myState == PlayerState.OutofCombat)
        {
            outOfCombatController.FinishedMovingIntoCombat();

            myState = PlayerState.InCombat;
            combatController.ChangeCombatState(CombatActionController.CombatState.SelectingCombatCards);
            //Animation
            foreach (PlayerCharacter character in myCharacters)
            {
                character.SwitchCombatState(true);
            }

            FindObjectOfType<CharacterSelectionButtons>().BreakAllLinks();

            CSBM.ShowCardIndicators();
            combatController.SelectCharacter(SelectPlayerCharacter);

            //UI
            DisableEndTurn();
            initBoard.gameObject.SetActive(true);

            //Game
            //FindObjectOfType<CombatManager>().ShowPeopleInCombat();

            //Board
            combatController.UnHighlightHexes();
            RemoveArea();
            hexVisualizer.HighlightSelectionHex(SelectPlayerCharacter.HexOn);
            //Camera
            FindObjectOfType<MyCameraController>().LookAt(SelectPlayerCharacter.transform);
        }
    }

    public Action GetCurrentAction()
    {
        if (SelectPlayerCharacter.InCombat())
        {
            return combatController.GetMyCurrectAction();
        }else
        {
            return outOfCombatController.GetMyCurrentAction();
        }
    }

    public void GoOutOfCombat()
    {
        myState = PlayerState.OutofCombat;
        foreach (PlayerCharacter character in myCharacters)
        {
            character.RefreshActions();
            character.DecreaseBuffsDuration();
            character.resetShield(character.GetArmor());
            character.SwitchCombatState(false);
            if (character.GetMyCombatHand().getSelectedCard() != null)
            {
                character.GetMyCombatHand().discardSelectedCard();
                character.SetMyCurrentCombatCard(null);
            }
        }

        CSBM.ShowCharacterSelection();
        CSBM.ShowActions();
        SelectCharacter(myCharacters[0]);

        //UI
        SelectPlayerCharacter.GetMyOutOfCombatHand().ShowHand();
        SelectPlayerCharacter.GetMyCombatHand().HideHand();
        AllowEndTurn();
        initBoard.ClearInitiativeBoard();
        initBoard.gameObject.SetActive(false);
    }

    public void EndPlayerTurn()
    {
        FindObjectOfType<NewHand>().DiscardHand();
        //This will rotate through players Than go to enemy turn
        FindObjectOfType<NewHand>().DrawNewHand();
    }

    bool PlayerInCombat()
    {
        foreach(PlayerCharacter character in myCharacters)
        {
            if (character.InCombat()) { return true; }
        }
        return false;
    }

    void ActionFinished()
    {
        FindObjectOfType<StagingArea>().DiscardCards();
        AllowEndTurn();
        UnHighlightHexes();
    }

    //Callbacks
    public void FinishedMoving(PlayerCharacter characterFinished)
    {
        //Allow end turn
        ActionFinished();
    }

    public void FinishedAttacking(PlayerCharacter characterFinished)
    {
        if (characterFinished.InCombat()) { GetComponent<CombatActionController>().FinishedAttacking(); }
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
                if (SelectPlayerCharacter.InCombat()) { combatController.UnShowSelectedPlayerCards(); }
                else{ SelectPlayerCharacter.GetMyOutOfCombatHand().unShowAnyCards(); }
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
        if (SelectPlayerCharacter.InCombat()) { combatController.ShowMoveArea(); }
    }

    public void selectNextAvailableCharacter()
    {
        foreach (PlayerCharacter character in myCharacters)
        {
            if (character.myCurrentCombatCard == null)
            {
                SelectPlayerCharacter.GetMyCombatHand().HideSelectedCard();
                SelectCharacter(character);
                return;
            }
        }
    }

    public void SelectCharacter(PlayerCharacter playerCharacter)
    {
        if (SelectPlayerCharacter != null && SelectPlayerCharacter != playerCharacter && SelectPlayerCharacter.InCombat() && combatController.UsingCards()) { return; }
        if (SelectPlayerCharacter != null && SelectPlayerCharacter.doorToOpen != null) { return; }
        RemoveArea();
        if (playerCharacter.InCombat())
        {
            combatController.SelectCharacter(playerCharacter);
        }
        else
        {
            outOfCombatController.SelectCharacter(playerCharacter);
        }
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

    public void ShowCharacterView()
    {
        if (!SelectPlayerCharacter.InCombat())
        {
            SelectPlayerCharacter.Selected();
        }
    }

    public bool ShowEnemyAreaAndCheckToFight(PlayerCharacter character)
    {
        if (!character.InCombat())
        {
            return FindObjectOfType<EnemyController>().ShowEnemyViewAreaAndCheckToFight(character);
        }
        return false;
    }

    void CheckToAllowExitFloorOrOpenDoor()
    {
    }

    void AllowExit()
    {
        actionButton.gameObject.SetActive(true);
        actionButton.AllowExit();
    }

    public void DisableExit()
    {
        actionButton.DisableExit();
        actionButton.gameObject.SetActive(false);
    }

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
        FindObjectOfType<NewHand>().MakeAllCardsPlayable();
        endTurnButton.AllowEndTurn();
    }

    public void DisableEndTurn()
    {
        CardsPlayable = false;
        FindObjectOfType<NewHand>().MakeAllCardsUnPlayable();
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
        if (cards[0].GetComponent<OutOfCombatCard>() != null)
        {
            SelectPlayerCharacter.GetMyOutOfCombatHand().DisableViewButton();
        }
        else if (cards[0].GetComponentInChildren<CombatPlayerCard>() != null) {
            ((CombatPlayerCard)cards[0]).SetUpCardActions();
            ((CombatPlayerCard)cards[1]).SetUpCardActions();
            ((CombatPlayerCard)cards[2]).SetUpCardActions();
            SelectPlayerCharacter.GetMyCombatHand().ShowHand();
            SelectPlayerCharacter.GetMyCombatHand().DisableBasicAttack();
            SelectPlayerCharacter.GetMyCombatHand().DisableViewButton();
            SelectPlayerCharacter.GetMyOutOfCombatHand().HideHand();
        }
        FilterCharacters();
    }

    public void FilterCharacters()
    {
        CSBM.FilterCharacter(SelectPlayerCharacter);
    }

    public void ReturnSelectionToNormal()
    {
        CSBM.ReturnButtonsToNormal();
    }

    public void ReturnToNormal()
    {
        outOfCombatController.StopLookingInChest();
        ReturnSelectionToNormal();
        SelectCharacter(SelectPlayerCharacter);
        //SelectPlayerCharacter.GetMyCombatHand().UnSelectCard();
        //SelectPlayerCharacter.GetMyOutOfCombatHand().UnSelectCard();

        //SelectPlayerCharacter.GetMyCombatHand().EnableViewButton();
        //SelectPlayerCharacter.GetMyCombatHand().EnableBasicAttack();
        //SelectPlayerCharacter.GetMyOutOfCombatHand().EnableViewButton();
        //SelectPlayerCharacter.GetMyCombatHand().HideHand();
        //SelectPlayerCharacter.GetMyOutOfCombatHand().ShowHand();
    }


}

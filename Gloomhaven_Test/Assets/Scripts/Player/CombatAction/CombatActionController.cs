using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatActionController : MonoBehaviour {

    private CharacterSelectionButtonManager CSBM;
    private MyCameraController myCamera;
    private CameraRaycaster raycaster;
    private HexVisualizer hexVisualizer;

    private CardAbility CardAbilityUsing;
    private List<Action> SelectedCardActions = new List<Action>();
    public List<bool> ActionsUsed = new List<bool>();

    public Action myCurrentAction;
    public Action GetMyCurrectAction()
    {
        return myCurrentAction;
    }

    public bool Attacking = false;

    private PlayerController playerController;

    private Character characterSelected;

    public int ActionIndex = 0;
    bool PerformingAction = false;

    public enum CombatState { SelectingCombatCards, UsingCombatCards, WaitingInCombat }
    public CombatState myCombatState;
    public CombatState GetCombatState()
    {
        return myCombatState;
    }
    public void ChangeCombatState(CombatState state)
    {
        myCombatState = state;
    }

    // Use this for initialization
    void Start () {
        playerController = GetComponent<PlayerController>();
        raycaster = FindObjectOfType<CameraRaycaster>();
        hexVisualizer = FindObjectOfType<HexVisualizer>();
        myCamera = FindObjectOfType<MyCameraController>();
        CSBM = GetComponent<CharacterSelectionButtonManager>();
    }

    public void SelectCharacterNotMoving()
    {
        foreach (PlayerCharacter character in playerController.myCharacters)
        {
            if (!character.GetMoving())
            {
                SelectCharacter(character);
                return;
            }
        }
    }

    public void SelectCharacter(PlayerCharacter playerCharacter)
    {
        if (playerCharacter.GetMoving()) { return; }
        PlayerCharacter SelectPlayerCharacter = playerController.SelectPlayerCharacter;
        if (myCombatState == CombatState.SelectingCombatCards)
        {
            UnHighlightHexes();
            if (SelectPlayerCharacter != null)
            {
                SelectPlayerCharacter.GetMyCombatHand().UnShowCard();
                SelectPlayerCharacter.myDecks.SetActive(false);
            }
            playerController.SelectPlayerCharacter = playerCharacter;

            myCamera.SetTarget(playerCharacter.transform);
            hexVisualizer.HighlightSelectionHex(playerCharacter.HexOn);
            playerCharacter.myDecks.SetActive(true);
            playerCharacter.GetMyCombatHand().ShowHand();
            playerCharacter.GetMyCombatHand().ShowSelectedCard();
            playerCharacter.GetMyOutOfCombatHand().HideHand();
            ShowActions(playerCharacter);
            CSBM.ShowCharacterButtonSelected(playerCharacter);
        }
        else if (myCombatState == CombatState.UsingCombatCards)
        {
            UnHighlightHexes();
            if (SelectPlayerCharacter != null) { SelectPlayerCharacter.myDecks.SetActive(false); }
            playerController.SelectPlayerCharacter = playerCharacter;

            myCamera.SetTarget(playerCharacter.transform);

            hexVisualizer.HighlightSelectionHex(playerCharacter.HexOn);
            CSBM.ShowCharacterButtonSelected(playerCharacter);
        }
    }

    public void UnShowSelectedPlayerCards()
    {
        if (myCombatState == CombatState.SelectingCombatCards)
        {
            playerController.SelectPlayerCharacter.GetMyCombatHand().unShowAnyCards();
        }
    }

    public void EndPlayerTurn()
    {
        PlayerCharacter SelectPlayerCharacter = playerController.SelectPlayerCharacter;
        switch (myCombatState)
        {
            case CombatState.WaitingInCombat:
                break;
            case CombatState.SelectingCombatCards:
                SelectPlayerCharacter.GetMyCombatHand().HideHand();
                SelectPlayerCharacter.GetMyCombatHand().HideSelectedCard();
                CSBM.HideCharacterSelection();
                FindObjectOfType<EndTurnButton>().DisableEndTurn();
                FindObjectOfType<CombatManager>().PlayerDonePickingCombatCards();
                break;
            case CombatState.UsingCombatCards:
                FindObjectOfType<EndTurnButton>().DisableEndTurn();
                SelectPlayerCharacter.GetMyCombatHand().HideSelectedCard();
                FindObjectOfType<myCharacterCard>().HideCharacterStats();
                FindObjectOfType<CombatManager>().PerformNextInInitiative();
                break;
        }
    }

    bool AllPlayersHaveCardSelected()
    {
        foreach (PlayerCharacter character in playerController.myCharacters)
        {
            if (character.GetMyCurrentCombatCard() == null) { return false; }
        }
        return true;
    }

    public void SelectCard(Card card)
    {
        PlayerCharacter SelectPlayerCharacter = playerController.SelectPlayerCharacter;
        if (SelectPlayerCharacter.GetMyCurrentCombatCard() != (CombatPlayerCard)card)
        {
            SelectPlayerCharacter.SetMyCurrentCombatCard((CombatPlayerCard)card);
            CSBM.ShowCardSelected(SelectPlayerCharacter);
            if (AllPlayersHaveCardSelected()) { playerController.AllowEndTurn(); }
            else { playerController.selectNextAvailableCharacter(); }
        }
        else
        {
            CSBM.ShowCardUnselected(SelectPlayerCharacter);
            SelectPlayerCharacter.SetMyCurrentCombatCard(null);
        }
    }

    public void CheckToSelectCharacter()
    {
        switch (myCombatState)
        {
            case CombatState.UsingCombatCards:
                CheckToShowCharacterStatsAndCard();
                break;
            case CombatState.SelectingCombatCards:
                playerController.CheckToSelectCharacter();
                CheckToShowCharacterStats();
                break;
        }
    }

    public Action NoAction()
    {
        return new Action(ActionType.None, new AOE(AOEType.SingleTarget, 0, 0), 0);
    }

    public void CheckToShowCharacterStatsAndCard()
    {
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hexSelected = HexHit.GetComponent<Hex>();
            if (hexSelected.EntityHolding != null && hexSelected.EntityHolding.GetComponent<EnemyCharacter>())
            {
                if (characterSelected != null) { hexVisualizer.UnHighlightHex(characterSelected.HexOn); }
                hexVisualizer.HighlightSelectionHex(hexSelected);
                characterSelected = hexSelected.EntityHolding.GetComponent<Character>();
                EnemyCharacter character = hexSelected.EntityHolding.GetComponent<EnemyCharacter>();
                FindObjectOfType<EnemyController>().ShowActionCard(character);
            }
            else
            {
                if (characterSelected != null) {
                    FindObjectOfType<CharacterViewer>().HideCharacterStats();
                    FindObjectOfType<CharacterViewer>().HideActionCard();
                    hexVisualizer.UnHighlightHex(characterSelected.HexOn);
                }
            }
        }
    }

    public void CheckToShowCharacterStats()
    {
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hexSelected = HexHit.GetComponent<Hex>();
            if (hexSelected.EntityHolding != null && hexSelected.EntityHolding.GetComponent<EnemyCharacter>())
            {
                if (characterSelected != null) { hexVisualizer.UnHighlightHex(characterSelected.HexOn); }
                hexVisualizer.HighlightSelectionHex(hexSelected);
                characterSelected = hexSelected.EntityHolding.GetComponent<Character>();
                EnemyCharacter character = hexSelected.EntityHolding.GetComponent<EnemyCharacter>();
                FindObjectOfType<CharacterViewer>().ShowCharacterStats(character.CharacterName, character.enemySprite, character);
            }
            else if  (hexSelected.EntityHolding != null && hexSelected.EntityHolding.GetComponent<PlayerCharacter>())
            {
                if (characterSelected != null) { hexVisualizer.UnHighlightHex(characterSelected.HexOn); }
                hexVisualizer.HighlightSelectionHex(hexSelected);
                characterSelected = hexSelected.EntityHolding.GetComponent<Character>();
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

    public void SwitchActionOrSelectCharacter()
    {
        if (myCombatState == CombatState.UsingCombatCards)
        {
            SwitchAction();
        }
        else if (myCombatState == CombatState.SelectingCombatCards)
        {
            playerController.selectAnotherCharacter();
        }
    }

    void SwitchAction()
    {
        if (PerformingAction) { return; }
        if (ActionsAllUsed()) { return; }
        int NewActionIndex = findNextActionIndex(ActionIndex);
        SetCurrentActionAs(NewActionIndex);
    }

    int findNextActionIndex(int index)
    {
        index++;
        if (index >= SelectedCardActions.Count) { index = 0; }
        if (!ActionsUsed[index]) { return index; }
        else { return findNextActionIndex(index); }
    }

    bool ActionsHasActionType(List<Action> actions, ActionType actionType)
    {
        for(int i = 0; i < actions.Count; i++)
        {
            if (actions[i].thisActionType == actionType) { return true; }
        }
        return false;
    }

    public void SetCurrentActionAs(int index)
    {
        FindObjectOfType<MyActionBoard>().UnHighlightAction(ActionIndex);
        ActionIndex = index;
        myCurrentAction = SelectedCardActions[ActionIndex];
        UnHighlightHexes();
        ShowAbility(SelectedCardActions[ActionIndex]);
        FindObjectOfType<MyActionBoard>().HighlightAction(ActionIndex);
        return;
    }


    public void UnHighlightHexes() { hexVisualizer.UnhighlightHexes(); }

    public void CheckToUseActions()
    {
        if (myCombatState == CombatState.UsingCombatCards)
        {
            UseCard(SelectedCardActions);
        }
    }

    public void UseCard(List<Action> actions)
    {
        UseAction(myCurrentAction);
    }

    public bool UseAction(Action action)
    {
        if (PerformingAction) { return false; }
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hexSelected = HexHit.GetComponent<Hex>();
            if (myCurrentAction.thisActionType == ActionType.Movement)
            {
                return CheckForMove(action, hexSelected);
            }
            else if (myCurrentAction.thisActionType == ActionType.Attack)
            {
                return CheckForAttack(action, hexSelected);
            }
            else if (myCurrentAction.thisActionType == ActionType.Heal)
            {
                return CheckForHeal(action, hexSelected);
            }
            else if (myCurrentAction.thisActionType == ActionType.Shield)
            {
                return CheckForShield(action, hexSelected);
            }
        }
        return false;
    }

    bool CheckForShield(Action action, Hex hexSelected)
    {
        if (action.Range == 0 && action.thisAOE.thisAOEType == AOEType.SingleTarget)
        {
            PerformingAction = true;
            playerController.SelectPlayerCharacter.Shield(action.thisAOE.Damage);
            return true;
        }
        return false;
    }

    bool CheckForHeal(Action action, Hex hexSelected)
    {
        //Heal self
        if (action.Range == 0 && action.thisAOE.thisAOEType == AOEType.SingleTarget)
        {
            PerformingAction = true;
            playerController.SelectPlayerCharacter.Heal(action.thisAOE.Damage);
            return true;
        }
        return false;
    }

    bool CheckForAttack(Action action, Hex hexSelected)
    {

        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        List<Node> nodes = FindObjectOfType<HexMapController>().GetAOE(action.thisAOE.thisAOEType, myCharacter.HexOn.HexNode, hexSelected.HexNode);
        bool success = false;

        List<Character> charactersAttacking = new List<Character>();
        bool meleeAtack = action.Range == 1;
        int range = meleeAtack ? action.Range : action.Range + myCharacter.GetDexterity();
        if (!myCharacter.HexAttackable(hexSelected, range)) { return false; }
        foreach (Node node in nodes)
        {
            if (node == null) { break; }
            if (myCharacter.HexDamageable(node.NodeHex))
            {
                UnHighlightHexes();
                foreach (Node node_highlight in nodes)
                {
                    hexVisualizer.HighlightAttackAreaHex(node_highlight.NodeHex);
                }
                Attacking = true;
                charactersAttacking.Add(node.NodeHex.EntityHolding.GetComponent<Character>());
                success = true;
            }
        }
        if ( charactersAttacking.Count == 0) { return false; }
        playerController.DisableEndTurn();
        myCharacter.Attack(action.thisAOE.Damage + myCharacter.GetStrength(), charactersAttacking);
        PerformingAction = true;
        return success;
    }

    bool CheckForMove(Action action, Hex hexSelected)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        if (myCharacter.HexInMoveRange(hexSelected, action.Range + myCharacter.GetAgility()))
        {
            UnHighlightHexes();
            myCharacter.ShowPath(hexSelected.HexNode);

            myCharacter.MoveOnPath(hexSelected);
            FindObjectOfType<MyCameraController>().SetTarget(myCharacter.transform);
            playerController.DisableEndTurn();
            PerformingAction = true;
            return true;
        }
        return false;
    }

    public void FinishedMoving()
    {
        if ( myCombatState == CombatState.UsingCombatCards)
        {
            PerformingAction = false;
            playerController.AllowEndTurn();
            FindObjectOfType<MyCameraController>().UnLockCamera();
            MoveToNextAbility();
        }
    }

    public void FinishedAttacking()
    {
        if (Attacking != false)
        {
            PerformingAction = false;
            playerController.AllowEndTurn();
            Attacking = false;
            MoveToNextAbility();
        }
    }

    public void FinishedHealing()
    {
        PerformingAction = false;
        MoveToNextAbility();
    }

    public void FinishedShielding()
    {
        PerformingAction = false;
        MoveToNextAbility();
    }

    void MoveToNextAbility()
    {
        UnHighlightHexes();
        hexVisualizer.HighlightSelectionHex(playerController.SelectPlayerCharacter.HexOn);
        characterSelected = null;
        FindObjectOfType<CharacterViewer>().HideCharacterStats();
        FindObjectOfType<CharacterViewer>().HideActionCard();
        ActionsUsed[ActionIndex] = true;
        FindObjectOfType<MyActionBoard>().DisableAction(ActionIndex);
        if (ActionsAllUsed()) { myCurrentAction = NoAction(); }
        else { SwitchAction(); }
    }

    public void ShowActions(PlayerCharacter character)
    {
        FindObjectOfType<MyActionBoard>().hideActions();
        CombatPlayerCard card = character.GetMyCombatHand().getSelectedCard();
        if (card != null)
        {
            FindObjectOfType<MyActionBoard>().showActions(card.CardAbility.Actions, character);
        }
    }

    bool ActionsAllUsed()
    {
        foreach (bool ActionUsed in ActionsUsed)
        {
            if (!ActionUsed) { return false; }
        }
        return true;
    }

    public void SetAbilities(CardAbility cardAbility)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        CardAbilityUsing = cardAbility;
        SelectedCardActions.Clear();
        ActionsUsed.Clear();
        FindObjectOfType<MyActionBoard>().showActions(CardAbilityUsing.Actions, myCharacter);
        List<Action> Actions = new List<Action>(cardAbility.Actions);
        foreach (Action action in Actions) {
            SelectedCardActions.Add(action);
            ActionsUsed.Add(false);
        }
        ActionIndex = 0;
        myCurrentAction = cardAbility.Actions[ActionIndex];
        ShowAbility(SelectedCardActions[ActionIndex]);
        FindObjectOfType<MyActionBoard>().HighlightAction(ActionIndex);
    }

    void ShowAbility(Action action)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        if (myCurrentAction.thisActionType == ActionType.Movement)
        {
            ShowMoveDistance(action.Range + myCharacter.GetAgility());
        }
        else if (myCurrentAction.thisActionType == ActionType.Attack)
        {
            bool meleeAtack = action.Range == 1;
            int range = meleeAtack ? action.Range : action.Range + myCharacter.GetDexterity();
            ShowAttack(range);
        }
        else if (myCurrentAction.thisActionType == ActionType.Heal)
        {
            ShowHeal(action.Range);
        }
        if (myCurrentAction.thisActionType == ActionType.Shield)
        {
            ShowShield(action.Range);
        }
    }

    void ShowShield(int Range)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        myCharacter.ShowShield(Range);
    }

    void ShowHeal(int Range)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        myCharacter.ShowHeal(Range);
    }

    void ShowAttack(int Range)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        myCharacter.ShowAttack(Range);
        HighlightHexForAttack(Range);
    }

    void HighlightHexForAttack(int Distance)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hexSelected = HexHit.GetComponent<Hex>();
            if (myCharacter.CheckIfinAttackRange(hexSelected, myCharacter.GetCurrentAttackRange()) && !Attacking)
            {
                hexVisualizer.HighlightAttackArea(hexSelected);
            }
            return;   
        }
    }

    public void ShowMoveArea()
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;

        if (myCurrentAction.thisActionType == ActionType.Movement)
        {
            ShowMoveDistance(myCurrentAction.Range + myCharacter.GetAgility());
        }
    }

    void ShowMoveDistance(int Distance)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        myCharacter.ShowMoveDistance(Distance);
        HighlightHexOverForMove(Distance);
    }

    void HighlightHexOverForMove(int Distance)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        Transform HexHit = raycaster.HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            Hex hexSelected = HexHit.GetComponent<Hex>();
            if (myCharacter.HexInMoveRange(hexSelected, Distance)) { hexVisualizer.HighlightMovePath(hexSelected); }
        }
    }

    void ShowDistance(int Distance)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        myCharacter.ShowRangeDistance(Distance);
    }


}

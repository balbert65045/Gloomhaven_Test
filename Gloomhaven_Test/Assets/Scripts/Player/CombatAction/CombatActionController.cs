using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatActionController : MonoBehaviour {

    public LayerMask MapLayer;

    private CombatCardAbility CardAbilityUsing;
    private List<Action> SelectedCardActions = new List<Action>();
    public List<bool> ActionsUsed = new List<bool>();

    public Action myCurrentAction;
    public Action GetMyCurrectAction()
    {
        return myCurrentAction;
    }

    public bool Attacking = false;

    private PlayerController playerController;
    private Character myCharacter;

    private Character characterSelected;

    public int ActionIndex = 0;

    // Use this for initialization
    void Start () {
        playerController = GetComponent<PlayerController>();
        myCharacter = playerController.myCharacter;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public Action NoAction()
    {
        return new Action(ActionType.None, new AOE(AOEType.SingleTarget, 0, 0), 0);
    }

    public void CheckToShowCharacterStatsAndCard()
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
                FindObjectOfType<EnemyController>().ShowActionCard(character);
            }
            else
            {
                if (characterSelected != null) {
                    FindObjectOfType<CharacterViewer>().HideCharacterStats();
                    FindObjectOfType<CharacterViewer>().HideActionCard();
                    characterSelected.HexOn.UnHighlight();
                }
            }
        }
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
            else if  (Hit.transform.GetComponent<Hex>().EntityHolding != null && Hit.transform.GetComponent<Hex>().EntityHolding.GetComponent<Character>())
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

    public void SwitchAction()
    {
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


    public void UnHighlightHexes()
    {
        Hex[] hexes = FindObjectsOfType<Hex>();
        foreach (Hex hex in hexes)
        {
            if (hex.HexNode.Shown)
            {
                hex.UnHighlight();
            }
        }
    }

    public void CheckToUseActions()
    {
        UseCard(SelectedCardActions);
    }

    public void UseCard(List<Action> actions)
    {
        UseAction(myCurrentAction);
    }

    public bool UseAction(Action action)
    {
        RaycastHit Hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out Hit, 100f, MapLayer))
        {
            Hex hexSelected = Hit.transform.GetComponent<Hex>();
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
            myCharacter.Shield(action.thisAOE.Damage);
            return true;
        }
        return false;
    }

    bool CheckForHeal(Action action, Hex hexSelected)
    {
        //Heal self
        if (action.Range == 0 && action.thisAOE.thisAOEType == AOEType.SingleTarget)
        {
            myCharacter.Heal(action.thisAOE.Damage);
            return true;
        }
        return false;
    }

    bool CheckForAttack(Action action, Hex hexSelected)
    {
        List<Node> nodes = FindObjectOfType<HexMapController>().GetAOE(action.thisAOE.thisAOEType, myCharacter.HexOn.HexNode, hexSelected.HexNode);
        bool success = false;

        List<Character> charactersAttacking = new List<Character>();
        bool meleeAtack = action.Range == 1;
        int range = meleeAtack ? action.Range : action.Range + myCharacter.Dexterity;
        if (!myCharacter.HexAttackable(hexSelected, range)) { return false; }
        foreach (Node node in nodes)
        {
            if (node == null) { break; }
            if (myCharacter.HexDamageable(node.NodeHex))
            {
                UnHighlightHexes();
                foreach (Node node_highlight in nodes)
                {
                    node_highlight.NodeHex.HighlightAttackArea();
                }
                Attacking = true;
                charactersAttacking.Add(node.NodeHex.EntityHolding.GetComponent<Character>());
                success = true;
            }
        }
        if ( charactersAttacking.Count == 0) { return false; }
        playerController.disableEndTurn();
        myCharacter.Attack(action.thisAOE.Damage + myCharacter.Strength, charactersAttacking);
        return success;
    }

    bool CheckForMove(Action action, Hex hexSelected)
    {
        if (myCharacter.HexInMoveRange(hexSelected, action.Range + myCharacter.Agility))
        {
            UnHighlightHexes();
            myCharacter.ShowPath(hexSelected.HexNode);

            myCharacter.MoveOnPath(hexSelected);
            if (hexSelected.GetComponent<Door>() != null && !hexSelected.GetComponent<Door>().isOpen)
            {
                hexSelected.GetComponent<Door>().OpenHexes();
            }
            FindObjectOfType<MyCameraController>().SetTarget(myCharacter.transform);
            playerController.disableEndTurn();
            return true;
        }
        return false;
    }

    public void finishedMoving()
    {
        playerController.allowEndTurn();
        FindObjectOfType<MyCameraController>().UnLockCamera();
        MoveToNextAbility();
    }

    public void finishedAttacking()
    {
        if (Attacking != false)
        {
            playerController.allowEndTurn();
            Attacking = false;
            MoveToNextAbility();
        }
    }

    public void finishedHealing()
    {
        MoveToNextAbility();
    }

    public void finishedShielding()
    {
        MoveToNextAbility();
    }

    void MoveToNextAbility()
    {
        UnHighlightHexes();
        characterSelected = null;
        FindObjectOfType<CharacterViewer>().HideCharacterStats();
        FindObjectOfType<CharacterViewer>().HideActionCard();
        ActionsUsed[ActionIndex] = true;
        FindObjectOfType<MyActionBoard>().DisableAction(ActionIndex);
        if (ActionsAllUsed()) { myCurrentAction = NoAction(); }
        else { SwitchAction(); }
    }

    bool ActionsAllUsed()
    {
        foreach (bool ActionUsed in ActionsUsed)
        {
            if (!ActionUsed) { return false; }
        }
        return true;
    }

    public void SetAbilities(CombatCardAbility cardAbility)
    {
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
        if (myCurrentAction.thisActionType == ActionType.Movement)
        {
            ShowMoveDistance(action.Range + myCharacter.Agility);
        }
        else if (myCurrentAction.thisActionType == ActionType.Attack)
        {
            bool meleeAtack = action.Range == 1;
            int range = meleeAtack ? action.Range : action.Range + myCharacter.Dexterity;
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
        myCharacter.ShowShield(Range);
    }

    void ShowHeal(int Range)
    {
        myCharacter.ShowHeal(Range);
    }

    void ShowAttack(int Range)
    {
        myCharacter.ShowAttack(Range);
    }

    void ShowMoveDistance(int Distance)
    {
        myCharacter.ShowMoveDistance(Distance);
    }

    void ShowDistance(int Distance)
    {
        myCharacter.ShowRangeDistance(Distance);
    }


}

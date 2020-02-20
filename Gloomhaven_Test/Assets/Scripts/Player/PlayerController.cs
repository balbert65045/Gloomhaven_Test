using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        myCharacters.Clear();
        myCharacters.AddRange(FindObjectsOfType<PlayerCharacter>());
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

        actionButton.gameObject.SetActive(false);

        StartCoroutine("StartGame");
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(.5f);
        CSBM.SetUpCharacterSelectionButtons();
        GoOutOfCombat();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            switch (myState)
            {
                case PlayerState.InCombat:
                    combatController.CheckToSelectCharacter();
                    break;
                case PlayerState.OutofCombat:
                    outOfCombatController.CheckToShowCharacterStats();
                    break;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (myState == PlayerState.OutofCombat)
            {
                outOfCombatController.UseAction(SelectPlayerCharacter);
            }
            else if (myState == PlayerState.InCombat)
            {
                combatController.CheckToUseActions();
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (myState == PlayerState.InCombat)
            {
                combatController.SwitchActionOrSelectCharacter();
            } 
            else if (myState == PlayerState.OutofCombat)
            {
                selectAnotherCharacter();
            }
        }
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
        switch (myState)
        {
            case PlayerState.InCombat:
                combatController.SelectCard(card);
                break;
            case PlayerState.OutofCombat:
                outOfCombatController.SelectCard(card);
                break;
        }
    }

    public void BeginNewTurn()
    {
        CSBM.ShowCharacterSelection();
        CSBM.ShowCardIndicators();
        foreach (PlayerCharacter character in myCharacters)
        {
            character.DecreaseBuffsDuration();
            character.resetShield(character.GetArmor());
            character.GetMyCombatHand().discardSelectedCard();
            character.SetMyCurrentCombatCard(null);
        }
        combatController.ChangeCombatState(CombatActionController.CombatState.SelectingCombatCards);
        SelectCharacter(myCharacters[0]);
    }

    public void BeginActions(PlayerCharacter character)
    {
        SelectCharacter(character);
        FindObjectOfType<myCharacterCard>().ShowCharacterStats(SelectPlayerCharacter.name, SelectPlayerCharacter.characterIcon, SelectPlayerCharacter);
        SelectPlayerCharacter.GetMyCombatHand().ShowSelectedCardToUse();
        CombatPlayerCard card = SelectPlayerCharacter.GetMyCombatHand().getSelectedCard();
        combatController.SetAbilities(card.CardAbility);
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
            FindObjectOfType<CombatManager>().ShowPeopleInCombat();

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
        if (myState == PlayerState.InCombat)
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
        combatController.UnHighlightHexes();
        FindObjectOfType<CharacterViewer>().HideCharacterStats();
        switch (myState)
        {
            case PlayerState.InCombat:
                combatController.EndPlayerTurn();
                break;
            case PlayerState.OutofCombat:
                outOfCombatController.EndTurn();
                break;
        }
    }


    //Callbacks
    public void FinishedMoving(PlayerCharacter characterFinished)
    {
        if (myState == PlayerState.InCombat) { combatController.FinishedMoving(); }
        else if (myState == PlayerState.OutofCombat)
        {
            outOfCombatController.FinishedMoving(characterFinished);
            CheckToAllowExitFloorOrOpenDoor();
        }
    }

    public void FinishedAttacking(PlayerCharacter characterFinished)
    {
        if (myState == PlayerState.InCombat){ GetComponent<CombatActionController>().FinishedAttacking(); }
        else if (myState == PlayerState.OutofCombat){ GoIntoCombat(); }
    }

    public void FinishedBuffing(PlayerCharacter characterFinished)
    {
        if (myState == PlayerState.InCombat) { GetComponent<CombatActionController>().FinishedBuffing(); }
        else { outOfCombatController.FinishedAction(characterFinished); }
    }

    public void FinishedHealing(PlayerCharacter characterFinished)
    {
        if (myState == PlayerState.InCombat) { GetComponent<CombatActionController>().FinishedHealing(); }
        else { outOfCombatController.FinishedAction(characterFinished); }
    }

    public void FinishedShielding(PlayerCharacter characterFinished)
    {
        if (myState == PlayerState.InCombat) { GetComponent<CombatActionController>().FinishedShielding(); }
        else { outOfCombatController.FinishedAction(characterFinished); }
    }

    //Character selection
    public void selectAnotherCharacter()
    {
        foreach (PlayerCharacter character in myCharacters)
        {
            if (character != SelectPlayerCharacter)
            {
                if (myState == PlayerState.InCombat) { combatController.UnShowSelectedPlayerCards() ; }
                else if (myState == PlayerState.OutofCombat) { SelectPlayerCharacter.GetMyOutOfCombatHand().unShowAnyCards(); }
                SelectCharacter(character);
                return;
            }
        }
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
        if (myState == PlayerState.OutofCombat)
        {
            outOfCombatController.SelectCharacter(playerCharacter);
        }
        else if (myState == PlayerState.InCombat)
        {
            combatController.SelectCharacter(playerCharacter);
        }
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
        if (myState == PlayerState.OutofCombat)
        {
            FindObjectOfType<EnemyController>().ShowCharactersView();
            SelectPlayerCharacter.Selected();
        }
    }

    public bool ShowEnemyAreaAndCheckToFight()
    {
        if (myState == PlayerState.OutofCombat)
        {
            return FindObjectOfType<EnemyController>().ShowEnemyViewAreaAndCheckToFight();
        }
        else if (myState == PlayerState.InCombat)
        {
            FindObjectOfType<EnemyController>().CheckToFight();
        }
        return false;
    }

    void CheckToAllowExitFloorOrOpenDoor()
    {
        bool exitAllowed = true;
        foreach (PlayerCharacter character in myCharacters)
        {
            if (character.HexOn.GetComponent<ExitAreaHex>() == null){ exitAllowed = false; }
        }
        if (exitAllowed) {
            AllowExit();
            return;
        }
        DisableExit();
    }

    void AllowExit()
    {
        actionButton.gameObject.SetActive(true);
        //actionButton.enabled = true;
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
        endTurnButton.AllowEndTurn();
    }

    public void DisableEndTurn()
    {
        endTurnButton.DisableEndTurn();
    }

    public void CharacterDied(PlayerCharacter character)
    {
        myCharacters.Remove(character);
        CSBM.RemoveCharacterButton(character);
        FindObjectOfType<InitiativeBoard>().takeCharacterOffBoard(character.CharacterName);
        if (myCharacters.Count == 0)
        {
            Time.timeScale = 0;
            FindObjectOfType<LostScreen>().TurnOnPanel();
        }
    }

    public void ChestOpenedFor(Card card)
    {
        if (card.GetComponent<OutOfCombatCard>() != null)
        {
            SelectPlayerCharacter.GetMyOutOfCombatHand().DisableViewButton();
        }
        else if (card.GetComponentInChildren<CombatPlayerCard>() != null) {
            ((CombatPlayerCard)card).SetUpCardActions();
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
        SelectPlayerCharacter.GetMyCombatHand().UnSelectCard();
        SelectPlayerCharacter.GetMyOutOfCombatHand().UnSelectCard();

        SelectPlayerCharacter.GetMyCombatHand().EnableViewButton();
        SelectPlayerCharacter.GetMyCombatHand().EnableBasicAttack();
        SelectPlayerCharacter.GetMyOutOfCombatHand().EnableViewButton();
        SelectPlayerCharacter.GetMyCombatHand().HideHand();
        SelectPlayerCharacter.GetMyOutOfCombatHand().ShowHand();
    }


}

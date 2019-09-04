using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public LayerMask MapLayer;

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

    private OutOfCombatActionController outOfCombatController;
    private CombatActionController combatController;
    private EndTurnButton endTurnButton;
    private CharacterSelectionButton[] characterButtons;
    private PlayerActionButton actionButton;
    private InitiativeBoard initBoard;
    private MyCameraController myCamera;

    private void Start()
    {
        myCamera = FindObjectOfType<MyCameraController>();
        combatController = GetComponent<CombatActionController>();
        outOfCombatController = GetComponent<OutOfCombatActionController>();
        endTurnButton = FindObjectOfType<EndTurnButton>();
        characterButtons = FindObjectsOfType<CharacterSelectionButton>();
        actionButton = FindObjectOfType<PlayerActionButton>();
        initBoard = FindObjectOfType<InitiativeBoard>();

        actionButton.gameObject.SetActive(false);

        StartCoroutine("StartGame");
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(.5f);
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
                    switch (myCombatState)
                    {
                        case CombatState.UsingCombatCards:
                            combatController.CheckToShowCharacterStatsAndCard();
                            break;
                        case CombatState.SelectingCombatCards:
                            CheckToSelectCharacter();
                            combatController.CheckToShowCharacterStats();
                            break;
                    }
                    break;
                case PlayerState.OutofCombat:
                    outOfCombatController.ShowOutOfCombatAbility(null);
                    SelectPlayerCharacter.GetMyOutOfCombatHand().UnSelectCard();
                    CheckToSelectCharacter();
                    outOfCombatController.CheckToShowCharacterStats();
                    break;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (myState == PlayerState.OutofCombat)
            {
                outOfCombatController.UseAction(SelectPlayerCharacter);
                //outOfCombatController.CheckToMoveOutOfCombat(SelectPlayerCharacter);
            }
            else if (myState == PlayerState.InCombat && myCombatState == CombatState.UsingCombatCards)
            {
                combatController.CheckToUseActions();
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (myState == PlayerState.InCombat && myCombatState == CombatState.UsingCombatCards)
            {
                combatController.SwitchAction();
            } 
            else if (myState == PlayerState.OutofCombat || (myState == PlayerState.InCombat && myCombatState == CombatState.SelectingCombatCards))
            {
                selectAnotherCharacter();
            }
        }
    }

    // Large Game Actions
    public void SelectCard()
    {
        switch (myState)
        {
            case PlayerState.InCombat:
                SelectPlayerCharacter.SetMyCurrentCombatCard(SelectPlayerCharacter.GetMyCombatHand().getSelectedCard());
                ShowCardSelected(SelectPlayerCharacter);
                if (AllPlayersHaveCardSelected()){ AllowEndTurn(); }
                else { selectNextAvailableCharacter(); }
                break;
            case PlayerState.OutofCombat:
                OutOfCombatCard card = SelectPlayerCharacter.GetMyOutOfCombatHand().GetSelectecdCard();
                GetComponent<OutOfCombatActionController>().ShowOutOfCombatAbility(card);
                break;
        }
    }

    public void BeginNewTurn()
    {
        ShowCharacterSelection();
        ShowCardIndicators();
        foreach (PlayerCharacter character in myCharacters)
        {
            character.DecreaseBuffsDuration();
            character.resetShield(character.Armor);
            character.GetMyCombatHand().discardSelectedCard();
            character.SetMyCurrentCombatCard(null);
        }
        ChangeCombatState(CombatState.SelectingCombatCards);
        SelectCharacter(myCharacters[0]);
    }

    public void BeginActions(PlayerCharacter character)
    {
        SelectCharacter(character);
        FindObjectOfType<myCharacterCard>().ShowCharacterStats(SelectPlayerCharacter.name, SelectPlayerCharacter.characterIcon, SelectPlayerCharacter);
        SelectPlayerCharacter.GetMyCombatHand().showSelectedCard();
        CombatPlayerCard card = SelectPlayerCharacter.GetMyCombatHand().getSelectedCard();
        combatController.SetAbilities(card.CardAbility);
    }

    public void GoIntoCombat()
    {
        if (myState == PlayerState.OutofCombat)
        {
            outOfCombatController.FinishedMovingIntoCombat();

            myState = PlayerState.InCombat;
            myCombatState = CombatState.SelectingCombatCards;
            //Animation
            foreach (PlayerCharacter character in myCharacters)
            {
                character.SwitchCombatState(true);
            }
            //UI
            SelectPlayerCharacter.GetMyOutOfCombatHand().HideHand();
            SelectPlayerCharacter.GetMyCombatHand().ShowHand();
            ShowCardIndicators();
            endTurnButton.gameObject.SetActive(true);
            initBoard.gameObject.SetActive(true);

            //Game
            FindObjectOfType<CombatManager>().ShowPeopleInCombat();

            //Board
            combatController.UnHighlightHexes();
            SelectPlayerCharacter.HexOn.HighlightSelection();
            //Camera
            FindObjectOfType<MyCameraController>().LookAt(SelectPlayerCharacter.transform);
        }
    }

    public void GoOutOfCombat()
    {
        myState = PlayerState.OutofCombat;
        foreach (PlayerCharacter character in myCharacters)
        {
            character.DecreaseBuffsDuration();
            character.resetShield(character.Armor);
            character.SwitchCombatState(false);
            if (character.GetMyCombatHand().getSelectedCard() != null)
            {
                character.GetMyCombatHand().discardSelectedCard();
                character.SetMyCurrentCombatCard(null);
            }
        }
        SelectCharacter(myCharacters[0]);

        //UI
        ShowCharacterSelection();
        SelectPlayerCharacter.GetMyOutOfCombatHand().ShowHand();
        SelectPlayerCharacter.GetMyCombatHand().HideHand();
        endTurnButton.gameObject.SetActive(false);
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
                switch (myCombatState)
                {
                    case CombatState.WaitingInCombat:
                        break;
                    case CombatState.SelectingCombatCards:
                        SelectPlayerCharacter.GetMyCombatHand().HideHand();
                        foreach (PlayerCharacter character in myCharacters)
                        {
                            if (!character.GetMyCombatHand().selectedCardLinkedButton.basicAttack) { LoseCardForCharacter(character); }
                        }
                        HideCharacterSelection();
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
                break;
            case PlayerState.OutofCombat:
                break;
        }
    }


    //Callbacks
    public void FinishedMoving()
    {
        if (myState == PlayerState.InCombat && myCombatState == CombatState.UsingCombatCards) { GetComponent<CombatActionController>().FinishedMoving(); }
        else if (myState == PlayerState.OutofCombat)
        {
            GetComponent<OutOfCombatActionController>().FinishedMoving();
            CheckToAllowExitFloorOrOpenDoor();
        }
    }

    public void FinishedAttacking()
    {
        if (myState == PlayerState.InCombat){ GetComponent<CombatActionController>().FinishedAttacking(); }
    }

    public void FinishedHealing()
    {
        if (myState == PlayerState.InCombat) { GetComponent<CombatActionController>().FinishedHealing(); }
    }

    public void FinishedShielding()
    {
        if (myState == PlayerState.InCombat) { GetComponent<CombatActionController>().FinishedShielding(); }
    }

    //Character Health to Card control
    public void SetHandSize(int size)
    {
        SelectPlayerCharacter.SetNewHandSize(size);
    }

    public void LoseCardForCharacter(PlayerCharacter character)
    {
        character.LoseCard();
    }

    //Character selection
    void selectAnotherCharacter()
    {
        foreach (PlayerCharacter character in myCharacters)
        {
            if (character != SelectPlayerCharacter)
            {
                if (myState == PlayerState.InCombat && myCombatState == CombatState.SelectingCombatCards) { SelectPlayerCharacter.GetMyCombatHand().unShowAnyCards(); }
                else if (myState == PlayerState.OutofCombat) { SelectPlayerCharacter.GetMyOutOfCombatHand().unShowAnyCards(); }
                SelectCharacter(character);
                return;
            }
        }
    }

    void selectNextAvailableCharacter()
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

    bool AllPlayersHaveCardSelected()
    {
        foreach(PlayerCharacter character in myCharacters)
        {
            if (character.GetMyCurrentCombatCard() == null) { return false; }
        }
        return true;
    }

    public void SelectCharacter(PlayerCharacter playerCharacter)
    {
        //if (AnyCharacterMoving()) { return; }
        if (myState == PlayerState.OutofCombat)
        {
            if (outOfCombatController.MovingIntoCombat) { return; }

            UnHighlightHexes();
            if (SelectPlayerCharacter != null) { SelectPlayerCharacter.myDecks.SetActive(false); }
            SelectPlayerCharacter = playerCharacter;

            myCamera.SetTarget(SelectPlayerCharacter.transform);

            SelectPlayerCharacter.HexOn.HighlightSelection();
            SelectPlayerCharacter.myDecks.SetActive(true);
            SelectPlayerCharacter.GetMyCombatHand().HideHand();
            SelectPlayerCharacter.GetMyOutOfCombatHand().ShowHand();
            ShowCharacterButtonSelected(SelectPlayerCharacter);
        }
        else if (myState == PlayerState.InCombat)
        {
            if (myCombatState == CombatState.SelectingCombatCards)
            {
                UnHighlightHexes();
                if (SelectPlayerCharacter != null) { SelectPlayerCharacter.myDecks.SetActive(false); }
                SelectPlayerCharacter = playerCharacter;

                myCamera.SetTarget(SelectPlayerCharacter.transform);
                SelectPlayerCharacter.HexOn.HighlightSelection();
                SelectPlayerCharacter.myDecks.SetActive(true);
                SelectPlayerCharacter.GetMyCombatHand().ShowHand();
                SelectPlayerCharacter.GetMyOutOfCombatHand().HideHand();
                combatController.ShowActions(SelectPlayerCharacter);
                ShowCharacterButtonSelected(SelectPlayerCharacter);
            }
            else if(myCombatState == CombatState.UsingCombatCards)
            {
                UnHighlightHexes();
                if (SelectPlayerCharacter != null) { SelectPlayerCharacter.myDecks.SetActive(false); }
                SelectPlayerCharacter = playerCharacter;

                myCamera.SetTarget(SelectPlayerCharacter.transform);

                SelectPlayerCharacter.HexOn.HighlightSelection();
                ShowCharacterButtonSelected(SelectPlayerCharacter);
            }
        }
        CheckToHideOpenDoor();
    }

    void CheckToSelectCharacter()
    {
        RaycastHit Hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out Hit, 100f, MapLayer))
        {
            if (Hit.transform.GetComponent<Hex>().EntityHolding != null && Hit.transform.GetComponent<Hex>().EntityHolding.GetComponent<PlayerCharacter>())
            {
                SelectCharacter(Hit.transform.GetComponent<Hex>().EntityHolding.GetComponent<PlayerCharacter>());
            }
        }
    }

    //Group character check
    bool AnyCharacterMoving()
    {
        foreach (PlayerCharacter character in myCharacters)
        {
            if (character.GetComponent<CharacterAnimationController>().Moving) { return true; }
        }
        return false;
    }


    //Map
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

    public void ShowCharacterView()
    {
        if (myState == PlayerState.OutofCombat)
        {
            FindObjectOfType<EnemyController>().ShowCharactersView();
        }
    }

    public bool ShowEnemyAreaAndCheckToFight()
    {
        if (myState == PlayerState.OutofCombat)
        {
            return FindObjectOfType<EnemyController>().ShowEnemyViewAreaAndCheckToFight();
        }
        return false;
    }


    //Character Selection Buttons
    void ShowCharacterButtonSelected(PlayerCharacter character)
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            if (button.characterLinkedTo == character)
            {
                button.CharacterSelected();
                return;
            }
        }
    }

    void HideCharacterSelection()
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            button.hideCardIndicators();
            button.gameObject.SetActive(false);
        }
    }

    void ShowCardSelected(PlayerCharacter character)
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            if (button.characterLinkedTo == character) { button.CardForCharacterSelected(); }
        }
    }

    void ShowCardIndicators()
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            button.showCardIndicators();
        }
    }

    void ShowCharacterSelection()
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            button.gameObject.SetActive(true);
            button.ReturnToColor();
        }
    }

    void CheckToAllowExitFloorOrOpenDoor()
    {
        if (SelectPlayerCharacter.HexOn.GetComponent<Door>() != null && !SelectPlayerCharacter.HexOn.GetComponent<Door>().isOpen)
        {
            AllowOpenDoor();
            return;
        }

        bool exitAllowed = true;
        foreach (PlayerCharacter character in myCharacters)
        {
            if (character.HexOn.GetComponent<ExitHex>() == null){ exitAllowed = false; }
        }
        if (exitAllowed) {
            AllowExit();
            return;
        }
        DisableExit();
        CheckToHideOpenDoor();
    }

    void AllowExit()
    {
        actionButton.gameObject.SetActive(true);
        actionButton.AllowExit();
    }

    void DisableExit()
    {
        actionButton.DisableExit();
        actionButton.gameObject.SetActive(false);
    }

    public void CheckToHideOpenDoor()
    {
        if (SelectPlayerCharacter.HexOn.GetComponent<Door>() != null && !SelectPlayerCharacter.HexOn.GetComponent<Door>().isOpen)
        {
            AllowOpenDoor();
        }
        else
        {
            HideOpenDoor();
        }
    }

    public void ExitLevel()
    {
        actionButton.gameObject.SetActive(false);
        Time.timeScale = 0;
        FindObjectOfType<LevelClearedPanel>().TurnOnPanel();
    }

    //Door Button
    public void AllowOpenDoor()
    {
        actionButton.gameObject.SetActive(true);
        actionButton.allowOpenDoorAction();
    }

    public void HideOpenDoor()
    {
        actionButton.gameObject.SetActive(false);
    }

    public void OpenDoor()
    {
        actionButton.gameObject.SetActive(false);
        SelectPlayerCharacter.OpenDoor();
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
        CharacterSelectionButton[] characterButtons = FindObjectsOfType<CharacterSelectionButton>();
        foreach(CharacterSelectionButton button in characterButtons)
        {
            if (button.characterLinkedTo == character) {
                button.gameObject.SetActive(false);
                break;
            }
        }
        FindObjectOfType<InitiativeBoard>().takeCharacterOffBoard(character.CharacterName);
        if (myCharacters.Count == 0)
        {
            Time.timeScale = 0;
            FindObjectOfType<LostScreen>().TurnOnPanel();
        }
    }

}

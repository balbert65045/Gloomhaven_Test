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

    private void Start()
    {
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

    public void SetHandSize(int size)
    {
        SelectPlayerCharacter.SetNewHandSize(size);
    }

    public void LoseCardForCharacter()
    {
        SelectPlayerCharacter.LoseCard();
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

    public void FinishedMoving()
    {
        if (myState == PlayerState.InCombat && myCombatState == CombatState.UsingCombatCards) { GetComponent<CombatActionController>().FinishedMoving(); }
        else if (myState == PlayerState.OutofCombat){ GetComponent<OutOfCombatActionController>().FinishedMoving(); }
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

    public void SelectCard()
    {
        switch (myState)
        {
            case PlayerState.InCombat:
                SelectPlayerCharacter.SetMyCurrentCombatCard(SelectPlayerCharacter.GetMyCombatHand().getSelectedCard());
                ShowCardSelected(SelectPlayerCharacter);
                if (AllPlayersHaveCardSelected())
                {
                    AllowEndTurn();
                }
                break;
            case PlayerState.OutofCombat:
                OutOfCombatCard card = SelectPlayerCharacter.GetMyOutOfCombatHand().GetSelectecdCard();
                GetComponent<OutOfCombatActionController>().UseOutOfCombatAbility(card);
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
            character.resetShield(SelectPlayerCharacter.Armor);
            character.GetMyCombatHand().discardSelectedCard();
            character.SetMyCurrentCombatCard (null);
        }
        ChangeCombatState(CombatState.SelectingCombatCards);
        SelectCharacter(myCharacters[0]);
    }

    public void AllowOpenDoor()
    {
        actionButton.gameObject.SetActive(true);
        actionButton.allowOpenDoorAction();
    }

    public void OpenDoor()
    {
        actionButton.gameObject.SetActive(false);
        SelectPlayerCharacter.OpenDoor();
    }

    public void AllowEndTurn()
    {
        endTurnButton.AllowEndTurn();
    }

    public void DisableEndTurn()
    {
        endTurnButton.DisableEndTurn();
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
            foreach (PlayerCharacter character in myCharacters)
            {
                character.SwitchCombatState(true);
            }
            SelectPlayerCharacter.GetMyOutOfCombatHand().HideHand();
            SelectPlayerCharacter.GetMyCombatHand().ShowHand();
            endTurnButton.gameObject.SetActive(true);
            initBoard.gameObject.SetActive(true);
            myState = PlayerState.InCombat;
            FindObjectOfType<CombatManager>().ShowPeopleInCombat();
            combatController.UnHighlightHexes();
            ShowCardIndicators();
        }
    }

    public void GoOutOfCombat()
    {
        myState = PlayerState.OutofCombat;
        ShowCharacterSelection();
        SelectCharacter(myCharacters[0]);
        foreach (PlayerCharacter character in myCharacters)
        {
            character.DecreaseBuffsDuration();
            character.SwitchCombatState(false);
        }
        SelectPlayerCharacter.GetMyOutOfCombatHand().ShowHand();
        SelectPlayerCharacter.GetMyCombatHand().HideHand();
        endTurnButton.gameObject.SetActive(false);
        initBoard.gameObject.SetActive(false);
    }

    public bool LoseCardInHand()
    {
        return SelectPlayerCharacter.GetMyCombatHand().LoseRandomCard();
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
                        foreach(PlayerCharacter character in myCharacters)
                        {
                            if (!character.GetMyCombatHand().selectedCardLinkedButton.basicAttack) { LoseCardForCharacter(); }
                        }
                        //SelectPlayerCharacter.SetMyCurrentCombatCard(SelectPlayerCharacter.GetMyCombatHand().getSelectedCard());
                        HideCharacterSelection();
                        FindObjectOfType<EndTurnButton>().DisableEndTurn();
                        FindObjectOfType<CombatManager>().PlayerDonePickingCombatCards();
                        //if (!AllPlayersHaveCardSelected())
                        //{
                        //    foreach (PlayerCharacter character in myCharacters)
                        //    {
                        //        if (character.GetMyCurrentCombatCard() == null)
                        //        {
                        //            SelectCharacter(character);
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    HideCharacterSelection();
                        //    FindObjectOfType<EndTurnButton>().DisableEndTurn();
                        //    FindObjectOfType<CombatManager>().PlayerDonePickingCombatCards();
                        //}
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

    public void UnHighlightHexes()
    {
        Hex[] hexes = FindObjectsOfType<Hex>();
        foreach (Hex hex in hexes)
        {
            if (hex.HexNode.Shown)
            {
                hex.returnToPreviousColor();
            }
        }
    }

    public void SelectCharacter(PlayerCharacter playerCharacter)
    {
        if (AnyCharacterMoving()) { return; }
        UnHighlightHexes();
        if (SelectPlayerCharacter != null) { SelectPlayerCharacter.myDecks.SetActive(false); }
        SelectPlayerCharacter = playerCharacter;
        if (myState == PlayerState.OutofCombat)
        {
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
                SelectPlayerCharacter.HexOn.HighlightSelection();
                SelectPlayerCharacter.myDecks.SetActive(true);
                SelectPlayerCharacter.GetMyCombatHand().ShowHand();
                SelectPlayerCharacter.GetMyOutOfCombatHand().HideHand();
                combatController.ShowActions(SelectPlayerCharacter);
                ShowCharacterButtonSelected(SelectPlayerCharacter);
            }
            else if(myCombatState == CombatState.UsingCombatCards)
            {
                SelectPlayerCharacter.HexOn.HighlightSelection();
                ShowCharacterButtonSelected(SelectPlayerCharacter);
            }
        }
    }

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

    bool AnyCharacterMoving()
    {
        foreach (PlayerCharacter character in myCharacters)
        {
            if (character.GetComponent<CharacterAnimationController>().Moving) { return true; }
        }
        return false;
    }

    // Update is called once per frame
    void Update () {

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
                    if (!AnyCharacterMoving())
                    {
                        CheckToSelectCharacter();
                        outOfCombatController.CheckToShowCharacterStats();
                    }
                    break;
            }
        }

       if (Input.GetMouseButtonDown(1))
        {
            if (myState == PlayerState.OutofCombat)
            {
                outOfCombatController.CheckToMoveOutOfCombat(SelectPlayerCharacter);
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
        }
	}

}

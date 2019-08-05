using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public LayerMask MapLayer;
    public PlayerCharacter myCharacter;
    public PlayerCharacter getMyCharacter()
    {
        return myCharacter;
    }

    public Sprite characterIcon;
    public string CharacterName;

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
    private CombatPlayerHand hand;
    private OutOfCombatHand outOfCombatHand;
    private EndTurnButton endTurnButton;
    private PlayerActionButton actionButton;
    private InitiativeBoard initBoard;

    private void Start()
    {
        combatController = GetComponent<CombatActionController>();
        outOfCombatController = GetComponent<OutOfCombatActionController>();
        hand = FindObjectOfType<CombatPlayerHand>();
        outOfCombatHand = FindObjectOfType<OutOfCombatHand>();
        endTurnButton = FindObjectOfType<EndTurnButton>();
        actionButton = FindObjectOfType<PlayerActionButton>();
        initBoard = FindObjectOfType<InitiativeBoard>();

        actionButton.gameObject.SetActive(false);

        myCharacter.ShowHexes();
        StartCoroutine("StartGame");
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(.5f);
        GoOutOfCombat();
    }

    public void SetUpCharacterHand(int size)
    {
        myCharacter.SetHandSize(size);
    }

    public void setHandSize(int size)
    {
        myCharacter.setNewHandSize(size);
    }

    public void LoseCardForCharacter()
    {
        myCharacter.LoseCard();
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
        if (myState == PlayerState.InCombat && myCombatState == CombatState.UsingCombatCards) { GetComponent<CombatActionController>().finishedMoving(); }
        else if (myState == PlayerState.OutofCombat){}
    }

    public void FinishedAttacking()
    {
        if (myState == PlayerState.InCombat){ GetComponent<CombatActionController>().finishedAttacking(); }
    }

    public void FinishedHealing()
    {
        if (myState == PlayerState.InCombat) { GetComponent<CombatActionController>().finishedHealing(); }
    }

    public void FinishedShielding()
    {
        if (myState == PlayerState.InCombat) { GetComponent<CombatActionController>().finishedShielding(); }
    }

    public void SelectCard()
    {
        switch (myState)
        {
            case PlayerState.InCombat:
                allowEndTurn();
                break;
            case PlayerState.OutofCombat:
                OutOfCombatCard card = outOfCombatHand.GetSelectecdCard();
                GetComponent<OutOfCombatActionController>().UseOutOfCombatAbility(card);
                break;
        }
    }

    public void BeginNewTurn()
    {
        myCharacter.DecreaseBuffsDuration();
        myCharacter.resetShield(myCharacter.Armor);
        hand.discardSelectedCard();
        ChangeCombatState(CombatState.SelectingCombatCards);
        hand.ShowHand();
    }

    public void allowOpenDoor()
    {
        actionButton.gameObject.SetActive(true);
        actionButton.allowOpenDoorAction();
    }

    public void OpenDoor()
    {
        actionButton.gameObject.SetActive(false);
        myCharacter.OpenDoor();
    }

    public void allowEndTurn()
    {
        endTurnButton.allowEndTurn();
    }

    public void disableEndTurn()
    {
        endTurnButton.disableEndTurn();
    }

    public void BeginActions()
    {
        FindObjectOfType<myCharacterCard>().ShowCharacterStats(myCharacter.name, characterIcon, myCharacter);
        hand.showSelectedCard();
        CombatPlayerCard card = hand.getSelectedCard();
        combatController.SetAbilities(card.TopAbility);
    }

    public void GoIntoCombat()
    {
        if (myState == PlayerState.OutofCombat)
        {
            myCharacter.SwitchCombatState(true);
            outOfCombatHand.HideHand();
            hand.ShowHand();
            endTurnButton.gameObject.SetActive(true);
            initBoard.gameObject.SetActive(true);
            myState = PlayerState.InCombat;
            FindObjectOfType<CombatManager>().ShowPeopleInCombat();
            combatController.UnHighlightHexes();
        }
    }

    public void GoOutOfCombat()
    {
        myCharacter.DecreaseBuffsDuration();
        myCharacter.SwitchCombatState(false);
        outOfCombatHand.ShowHand();
        hand.HideHand();
        endTurnButton.gameObject.SetActive(false);
        initBoard.gameObject.SetActive(false);
        myState = PlayerState.OutofCombat;
    }

    public bool LoseCardInHand()
    {
        return hand.LoseRandomCard();
    }

    public void EndPlayerTurn()
    {
        combatController.UnHighlightHexes();
        FindObjectOfType<CharacterViewer>().HideCharacterStats();
        FindObjectOfType<EndTurnButton>().disableEndTurn();
        switch (myState)
        {
            case PlayerState.InCombat:
                switch (myCombatState)
                {
                  case CombatState.WaitingInCombat:
                    break;
                  case CombatState.SelectingCombatCards:
                    hand.HideHand();
                    if (!hand.selectedCardLinkedButton.basicAttack) { LoseCardForCharacter(); }
                    FindObjectOfType<CombatManager>().PlayerDonePickingCombatCards();
                    break;
                  case CombatState.UsingCombatCards:
                    hand.HideSelectedCard();
                    FindObjectOfType<myCharacterCard>().HideCharacterStats();
                    FindObjectOfType<CombatManager>().PerformNextInInitiative();
                    break;
                }
                break;
            case PlayerState.OutofCombat:
                break;
        }
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
                            combatController.CheckToShowCharacterStats();
                            break;
                    }
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
                outOfCombatController.CheckToMoveOutOfCombat(myCharacter);
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

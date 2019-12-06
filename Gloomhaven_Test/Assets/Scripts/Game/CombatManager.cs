using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour {

    private EnemyController enemyController;
    private PlayerController playerController;
    private InitiativeBoard initBoard;

    public void PlayerDonePickingCombatCards()
    {
        EnemyActionCard[] enemyActions = enemyController.selectEnemyActions();
        PlayerCharacter[] playerCharacters = FindObjectsOfType<PlayerCharacter>();

        foreach(PlayerCharacter playerCharacter in playerCharacters)
        {
            CombatPlayerCard playerCard = playerCharacter.GetMyCurrentCombatCard();
            initBoard.AddInitiative(playerCharacter.CharacterName, playerCard.Initiative, playerCard.gameObject);
        }

        foreach (EnemyActionCard enemyAction in enemyActions)
        {
            initBoard.AddInitiative(enemyAction.characterName, enemyAction.Initiative, enemyAction.gameObject);
        }
        initBoard.OrganizeInits();

        playerController.ChangeCombatState(CombatActionController.CombatState.WaitingInCombat);
        PerformNextInInitiative();
    }

    public void AddGroupToCombat(EnemyGroup EG)
    {
        initBoard.AddCharacterToBoard(EG);
        EnemyActionCard action = EG.getNewActionCard();
        initBoard.AddInitiative(action.characterName, action.Initiative, action.gameObject);
        initBoard.OrganizeInits();
    }

    public void ShowPeopleInCombat()
    {
        StartCoroutine("ShowingCharactersInCombet");
    }

    IEnumerator ShowingCharactersInCombet()
    {
        yield return new WaitForSeconds(.2f);
        List<EnemyGroup> GroupsInCombat = new List<EnemyGroup>();
        foreach (EnemyGroup EG in enemyController.enemyGroups)
        {
            if (EG.hasCharactersOut() && EG.hasCharacterInCombat())
            {
                GroupsInCombat.Add(EG);
            }
        }

        initBoard.placeCharacterIcons(playerController, GroupsInCombat.ToArray());
    }

    public void PerformNextInInitiative()
    {
    
        GameObject card = initBoard.GetNextInitiativeCard();
        if (card != null)
        {
            if (card.GetComponent<CombatPlayerCard>() != null)
            {
                // Not the way to do this
                PlayerCharacter currentPlayerCharacter = null;
                PlayerCharacter[] playerCharacters = FindObjectsOfType<PlayerCharacter>();
                foreach(PlayerCharacter character in playerCharacters)
                {
                    if (character.GetMyCurrentCombatCard() == card.GetComponent<CombatPlayerCard>())
                    {
                        currentPlayerCharacter = character;
                        break;
                    }
                }

                if (currentPlayerCharacter == null) { Debug.LogWarning("No character with that card"); }
                FindObjectOfType<MyCameraController>().LookAt(currentPlayerCharacter.transform);
                FindObjectOfType<MyCameraController>().UnLockCamera();
                playerController.ChangeCombatState(CombatActionController.CombatState.UsingCombatCards);
                playerController.AllowEndTurn();
                playerController.BeginActions(currentPlayerCharacter);
            }
            else if (card.GetComponent<EnemyActionCard>() != null)
            {
                playerController.ChangeCombatState(CombatActionController.CombatState.WaitingInCombat);
                enemyController.BeginActionForGroup(card.GetComponent<EnemyActionCard>());
            }
        }
        else
        {
            if (enemyController.hasNoMoreCharacters()) { playerController.GoOutOfCombat(); }
            else { BeginNewTurn(); }
        }
    }

    void BeginNewTurn()
    {
        FindObjectOfType<MyCameraController>().LookAt(playerController.SelectPlayerCharacter.transform);
        FindObjectOfType<MyCameraController>().UnLockCamera();
        //TakeAwaySummoningSickness();
        //FindObjectOfType<EnemyCardButton>().SetInteractable(false);
        //announcmentBoard.ShowText("Pick Cards");
        enemyController.takeAwayBuffs();
        initBoard.ClearInitiativeBoard();
        ShowPeopleInCombat();
        playerController.BeginNewTurn();
    }


    // Use this for initialization
    void Start()
    {
        enemyController = FindObjectOfType<EnemyController>();
        playerController = FindObjectOfType<PlayerController>();
        initBoard = FindObjectOfType<InitiativeBoard>();
      //  StartCoroutine("DummyStartCombatCoroutine");
    }

    IEnumerator DummyStartCombatCoroutine()
    {
        yield return new WaitForSeconds(.2f);
        ShowPeopleInCombat();
    }

    // Update is called once per frame
    void Update () {
		
	}
}

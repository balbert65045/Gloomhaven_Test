using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour {

    private EnemyController enemyController;
    private PlayerController playerController;
    private InitiativeBoard initBoard;

    public void PlayerDonePickingCombatCards()
    {
        //announcmentBoard.ShowText("Inniative");
        EnemyActionCard[] enemyActions = enemyController.selectEnemyActions();
        CombatPlayerCard playerCard = FindObjectOfType<CombatPlayerHand>().getSelectedCard();

        initBoard.AddInitiative(playerController.CharacterName, playerCard.Initiative, playerCard.gameObject);
        foreach (EnemyActionCard enemyAction in enemyActions)
        {
            initBoard.AddInitiative(enemyAction.characterName, enemyAction.Initiative, enemyAction.gameObject);
        }
        initBoard.OrganizeInits();

        playerController.ChangeCombatState(PlayerController.CombatState.WaitingInCombat);
        PerformNextInInitiative();
    }

    public void ShowPeopleInCombat()
    {
        List<EnemyGroup> GroupsInCombat = new List<EnemyGroup>();
        foreach (EnemyGroup EG in enemyController.enemyGroups)
        {
            if (EG.hasCharactersOut())
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
                FindObjectOfType<MyCameraController>().LookAt(playerController.myCharacter.transform);
                FindObjectOfType<MyCameraController>().UnLockCamera();
                playerController.ChangeCombatState(PlayerController.CombatState.UsingCombatCards);
                playerController.allowEndTurn();
                playerController.BeginActions();
            }
            else if (card.GetComponent<EnemyActionCard>() != null)
            {
                playerController.ChangeCombatState(PlayerController.CombatState.WaitingInCombat);
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
        FindObjectOfType<MyCameraController>().LookAt(playerController.myCharacter.transform);
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

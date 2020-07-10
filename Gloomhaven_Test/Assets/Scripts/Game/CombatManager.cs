using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour {

    private EnemyController enemyController;
    private PlayerController playerController;
    private InitiativeBoard initBoard;

    int InitIndex = 0;
    List<InitiativePosition> InitiativesInPlay = new List<InitiativePosition>();
    static int SortByInitiative(InitiativePosition IP1, InitiativePosition IP2)
    {
        return IP1.InitValue.CompareTo(IP2.InitValue);
    }
    public void OrganizeAllInitiatives()
    {
        InitiativesInPlay.Sort(SortByInitiative);
    }
    public GameObject GetNextInitiativeCard()
    {
        if (InitIndex < InitiativesInPlay.Count)
        {
            GameObject card = InitiativesInPlay[InitIndex].GetCard();
            return card;
        }
        return null;
    }

    public GameObject CombatZonePrefab;

    public CombatZone CreateCombatZone(Character Character)
    {
        GameObject NewCombatZone = Instantiate(CombatZonePrefab);
        NewCombatZone.transform.position = new Vector3(0, 0, 0);
        NewCombatZone.GetComponent<CombatZone>().AddCharacterToCombat(Character);
        return NewCombatZone.GetComponent<CombatZone>();
    }

    public void SelectCharacter(string characterName)
    {
        if (!enemyController.SelectCharacter(characterName)) { playerController.SelectCharacterByName(characterName); }
    }

    public void PlayerDonePickingCombatCards()
    {
        //EnemyActionCard[] enemyActions = enemyController.selectEnemyActions();
        //CombatZone[] combatZones = FindObjectsOfType<CombatZone>();
        //foreach(CombatZone combatZone in combatZones)
        //{
        //    List<InitiativePosition> InitiativesInZone = combatZone.PopulateInitiatives(enemyActions);
        //    foreach(InitiativePosition initiative in InitiativesInZone)
        //    {
        //        bool alreadThere = false;
        //        foreach(InitiativePosition ExistingIP in InitiativesInPlay)
        //        {
        //            if (ExistingIP.CharacterNameLinkedTo == initiative.CharacterNameLinkedTo) { alreadThere = true; }
        //        }
        //        if (!alreadThere) { InitiativesInPlay.Add(initiative); }
        //    }
        //}
        //OrganizeAllInitiatives();
        //InitIndex = 0;

        //PerformNextInInitiative();
    }

    public void AddGroupToCombat(EnemyGroup EG)
    {
        //initBoard.AddCharacterToBoard(EG);
        //EnemyActionCard action = EG.getNewActionCard();
        //initBoard.AddInitiative(action.characterName, action.Initiative, action.gameObject);
        //initBoard.OrganizeInits();
    }

    public void ShowPeopleInCombat()
    {
        StartCoroutine("ShowingCharactersInCombet");
    }

    IEnumerator ShowingCharactersInCombet()
    {
        yield return new WaitForSeconds(.2f);
        //List<EnemyGroup> GroupsInCombat = new List<EnemyGroup>();
        //foreach (EnemyGroup EG in enemyController.enemyGroups)
        //{
        //    if (EG.hasCharactersOut() && EG.hasCharacterInCombat())
        //    {
        //        GroupsInCombat.Add(EG);
        //    }
        //}

        //initBoard.placeCharacterIcons(playerController, GroupsInCombat.ToArray());
    }

    public void PerformNextInInitiative()
    {
    
        //GameObject card = GetNextInitiativeCard();
        //InitIndex++;
        //if (card != null)
        //{
        //    CombatZone[] combatZones = FindObjectsOfType<CombatZone>();
        //    foreach(CombatZone combatZone in combatZones) { combatZone.ShowInitiativeBoard(); }
        //    if (card.GetComponent<CombatPlayerCard>() != null)
        //    {
        //        // Not the way to do this
        //        PlayerCharacter currentPlayerCharacter = null;
        //        PlayerCharacter[] playerCharacters = FindObjectsOfType<PlayerCharacter>();
        //        foreach(PlayerCharacter character in playerCharacters)
        //        {
        //            //if (character.GetMyCurrentCombatCard() == card.GetComponent<CombatPlayerCard>())
        //            //{
        //            //    currentPlayerCharacter = character;
        //            //    break;
        //            //}
        //        }

        //        if (currentPlayerCharacter == null || currentPlayerCharacter.myCombatZone == null || currentPlayerCharacter.GetGoingToDie()) {
        //            PerformNextInInitiative();
        //            return;
        //        }
        //        currentPlayerCharacter.myCombatZone.ShowInitiativeBoard();
        //        currentPlayerCharacter.myCombatZone.ShowMyCharacterAsCurrentAction(currentPlayerCharacter.CharacterName);
        //        FindObjectOfType<MyCameraController>().LookAt(currentPlayerCharacter.transform);
        //        FindObjectOfType<MyCameraController>().UnLockCamera();
        //        playerController.AllowEndTurn();
        //    }
        //    else if (card.GetComponent<EnemyActionCard>() != null)
        //    {
        //        enemyController.BeginActionForGroup(card.GetComponent<EnemyActionCard>());
        //    }
        //}
        //else
        //{
        //    CombatZone[] combatZones = FindObjectsOfType<CombatZone>();
        //    foreach (CombatZone combatZone in combatZones)
        //    {
        //        combatZone.CheckToEndZone();
        //    }
        //    //if (enemyController.hasNoMoreCharacters()) { playerController.GoOutOfCombat(); }
        //    //CheckCombatZones
        //    BeginNewTurn();
        //}
    }

    void BeginNewTurn()
    {
        if (playerController.SelectPlayerCharacter != null) { FindObjectOfType<MyCameraController>().LookAt(playerController.SelectPlayerCharacter.transform); }
        FindObjectOfType<MyCameraController>().UnLockCamera();
        //TakeAwaySummoningSickness();
        //FindObjectOfType<EnemyCardButton>().SetInteractable(false);
        //announcmentBoard.ShowText("Pick Cards");
        enemyController.takeAwayBuffs();
        InitiativesInPlay.Clear();
        CombatZone[] combatZones = FindObjectsOfType<CombatZone>();
        foreach(CombatZone combatZone in combatZones)
        {
            if (!combatZone.Dissolving) { combatZone.ShowPeopleInCombat(); }
        }
        //ShowPeopleInCombat();
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

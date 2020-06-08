using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatZone : MonoBehaviour {

    public List<Node> CombatNodes;
    public void AddNodesToCombatNodes(List<Node> nodes)
    {
        foreach (Node node in nodes)
        {
            if (node.edge) { continue; }
            if (!CombatNodes.Contains(node)) {
                CombatNodes.Add(node);
                node.NodeHex.AddCombatZone(this);
            }
        }
    }
    public void UpdateCombatNodes()
    {
        UnLinkAllHexes();
        CombatNodes.Clear();
        foreach(Character character in CharactersInCombat)
        {
            AddNodesToCombatNodes(character.CombatNodes);
        }
        CreateZone();
    }

    public List<Character> CharactersInCombat;
    public List<PlayerCharacter> GetPlyaerCharacters()
    {
        List<PlayerCharacter> PCs = new List<PlayerCharacter>();
        foreach(Character character in CharactersInCombat) {
            if (character == null) { Debug.LogError("Null Character in combat"); }
            if (character.GetComponent<PlayerCharacter>() != null) { PCs.Add(character.GetComponent<PlayerCharacter>()); }
        }
        return PCs;
    }
    public void AddCharacterToCombat(Character character) {
        if (!CharactersInCombat.Contains(character))
        {
            CharactersInCombat.Add(character);
            character.SetCombatZone(this);
        }
    }

    public void AddPlayerCharacterToAlreadyStartedCombat(PlayerCharacter character)
    {
        CharactersInCombat.Add(character);
        character.SetCombatZone(this);
        InitiativeBoard.AddCharacterIcon(character);
    }

    public GameObject InitiativeBoardPrefab;
    InitiativeBoard InitiativeBoard;

    MeshGenerator CombatZoneMeshGen;
    EdgeLine CombatZoneEdgeLine;
    public bool Dissolving = false;

    public void CheckToEndZone()
    {
        if (CharactersInCombat.Count == 0) {
            DissolveCombatZone();
        }
        else if (CharactersInCombat[0].IsPlayer())
        {
            foreach(Character character in CharactersInCombat)
            {
                if (character.IsEnemy()) { return;}
            }
            DissolveCombatZone();
        }
        else if (CharactersInCombat[0].IsEnemy())
        {
            foreach (Character character in CharactersInCombat)
            {
                if (character.IsPlayer()) { return; }
            }
            DissolveCombatZone();
        }
    }

    void DissolveCombatZone()
    {
        Dissolving = true;
        UnlinkAllCharacters();
        UnLinkAllHexes();
        Destroy(InitiativeBoard.gameObject);
        Destroy(this.gameObject);
    }

    void UnLinkAllHexes()
    {
        foreach (Node node in CombatNodes)
        {
            node.NodeHex.RemoveCombatZone(this);
        }
    }

    void UnlinkAllCharacters()
    {
        foreach (Character character in CharactersInCombat)
        {
            character.myCombatZone = null;
            if (character.IsPlayer())
            {
                PlayerCharacter PC = character.GetComponent<PlayerCharacter>();
                PC.resetShield(character.GetArmor());
                PC.SwitchCombatState(false);
                if (PC.GetMyCombatHand().getSelectedCard() != null)
                {
                    PC.GetMyCombatHand().discardSelectedCard();
                    PC.SetMyCurrentCombatCard(null);
                }
                PC.myCharacterSelectionButton.hideCardIndicators();
                PC.myCharacterSelectionButton.ShowActions();
            }
            else if (character.IsEnemy())
            {
                EnemyCharacter EC = character.GetComponent<EnemyCharacter>();
                character.SwitchCombatState(false);
                EC.RecreateThreatArea();
            }
        }
    }

    public void removeCharacter(Character character)
    {
        CharactersInCombat.Remove(character);
        if (character.GetComponent<EnemyCharacter>() != null)
        {
            EnemyGroup group = character.GetComponent<EnemyCharacter>().GetGroup();
            foreach (Character characterInCombat in CharactersInCombat)
            {
                if (characterInCombat == character) { continue; }
                EnemyCharacter enemyChar = characterInCombat.GetComponent<EnemyCharacter>();
                if (enemyChar == null) { continue; }
                if (enemyChar.GetGroup() == group) { return; }
            }
            InitiativeBoard.takeCharacterOffBoard(group.CharacterNameLinkedTo);
        }
        else { InitiativeBoard.takeCharacterOffBoard(((PlayerCharacter)character).CharacterName); }
    }

    public void HideInitiativeBoard()
    {
        InitiativeBoard.gameObject.SetActive(false);
    }

    public void ShowInitiativeBoard()
    {
        CombatZone[] combatZones = FindObjectsOfType<CombatZone>();
        foreach(CombatZone combatZone in combatZones)
        {
            combatZone.HideInitiativeBoard();
        }
        InitiativeBoard.gameObject.SetActive(true);
    }

    public void ClearInitiativeBoard()
    {
        InitiativeBoard.ClearInitiativeBoard();
    }

    public List<InitiativePosition> PopulateInitiatives(EnemyActionCard[] enemyActions)
    {
        List<InitiativePosition> Initiatives = new List<InitiativePosition>();
        foreach(EnemyActionCard enemyAction in enemyActions)
        {
            InitiativePosition IP = InitiativeBoard.AddInitiative(enemyAction.characterName, enemyAction.Initiative, enemyAction.gameObject);
            if (IP != null){ Initiatives.Add(IP); }
        }
        foreach(PlayerCharacter playerCharacter in GetPlyaerCharacters())
        {
            CombatPlayerCard playerCard = playerCharacter.GetMyCurrentCombatCard();
            InitiativePosition IP = InitiativeBoard.AddInitiative(playerCharacter.CharacterName, playerCard.Initiative, playerCard.gameObject);
            Initiatives.Add(IP);
        }
        InitiativeBoard.OrganizeInits();
        return Initiatives;
    }

    public void ShowMyCharacterAsCurrentAction(string characterName)
    {
        InitiativeBoard.ShowMyCharacterAsCurrentAction(characterName);
    }

    public void ShowPeopleInCombat()
    {
        if (InitiativeBoard != null) { Destroy(InitiativeBoard.gameObject); }
        CreateInitiativeBoard();
        ShowCharactersOnInitiative();
        CreateZone();
        HideZone();
    }

    void ShowCharactersOnInitiative()
    {
        List<EnemyGroup> GroupsInCombat = new List<EnemyGroup>();
        List<PlayerCharacter> playerCharacters = new List<PlayerCharacter>();
        foreach (Character character in CharactersInCombat)
        {
            if (character == null) { continue; }
            if (character.GetComponent<PlayerCharacter>() != null)
            {
                playerCharacters.Add((PlayerCharacter)character);
            }
            else if (character.GetComponent<EnemyCharacter>() != null)
            {
                EnemyGroup group = FindObjectOfType<EnemyController>().GetGroupFromCharacter((EnemyCharacter)character);
                if (!GroupsInCombat.Contains(group)) { GroupsInCombat.Add(group); }
            }
        }

        InitiativeBoard.placeCharacterIconsUsingCharacters(playerCharacters, GroupsInCombat);
    }

    void CreateInitiativeBoard()
    {
        Transform canvas = FindObjectOfType<Canvas>().transform;
        GameObject InitBoard = Instantiate(InitiativeBoardPrefab, canvas);
        InitiativeBoard = InitBoard.GetComponent<InitiativeBoard>();
        InitiativeBoard.transform.SetAsFirstSibling();
        InitiativeBoard.InitializeBoard();
    }

    public void CreateZone()
    {
        if (CombatZoneMeshGen == null && CombatZoneEdgeLine == null)
        {
            CombatZoneMeshGen = GetComponentInChildren<MeshGenerator>();
            CombatZoneEdgeLine = GetComponentInChildren<EdgeLine>();
        }
        List<Vector3> points = FindObjectOfType<HexMapController>().GetHexesSurrounding(CharactersInCombat[0].HexOn.HexNode, CombatNodes);
        CombatZoneMeshGen.CreateMesh(points);
        CombatZoneEdgeLine.CreateLine(points.ToArray());
    }

    public void HideZone()
    {
        CombatZoneMeshGen.gameObject.SetActive(false);
        CombatZoneEdgeLine.gameObject.SetActive(false);
    }

    public void ShowZone()
    {
        CombatZoneMeshGen.gameObject.SetActive(true);
        CombatZoneEdgeLine.gameObject.SetActive(true);
    }

    public void MergeCombatZone(CombatZone combatZoneMerging, string characterCausingMerge)
    {
        foreach(Character character in combatZoneMerging.CharactersInCombat)
        {
            if (!CharactersInCombat.Contains(character)) {
                AddCharacterToCombat(character);
            }
        }
        foreach(Node node in combatZoneMerging.CombatNodes)
        {
            node.NodeHex.RemoveCombatZone(combatZoneMerging);
        }
        MergeInitiativeBoards(combatZoneMerging.InitiativeBoard, characterCausingMerge);
        AddNodesToCombatNodes(combatZoneMerging.CombatNodes);
        Destroy(combatZoneMerging.gameObject);
    }

    public void MergeInitiativeBoards(InitiativeBoard board, string characterCausingMerge)
    {
        foreach (InitiativePosition initPos in board.ActiveInitPositions)
        {
            if (!InitiativeBoard.AlreadyHasThisCharacter(initPos.CharacterNameLinkedTo))
            {
                InitiativeBoard.AddCharacterFromInitPos(initPos);
            }
        }
        InitiativeBoard.OrganizeInits();
        InitiativeBoard.PutIndicatorOnCharacter(characterCausingMerge);
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

        //InitiativeBoard.placeCharacterIcons(playerController, GroupsInCombat.ToArray());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {
    // Use this for initialization
    public EnemyGroup[] enemyGroups;

    public GameObject EnemyMeshGeneratorPrefab;
    public GameObject EnemyLinePrefab;

    public GameObject ThreatAreaPrefab;

    public void CreateThreatAreaShown(EnemyCharacter character, List<Node> nodes)
    {
        GameObject newThreatAreaObj = Instantiate(ThreatAreaPrefab);
        ThreatArea newThreatArea = newThreatAreaObj.GetComponent<ThreatArea>();
        newThreatArea.AddEnemyCharacter(character);
        newThreatArea.AddEnemyNodes(nodes);
        newThreatArea.UpdateVisualArea();
    }

    public void CreateThreatAreaHidden(List<Node> nodes)
    {
        GameObject newThreatAreaObj = Instantiate(ThreatAreaPrefab);
        ThreatArea newThreatArea = newThreatAreaObj.GetComponent<ThreatArea>();
        newThreatArea.AddEnemyNodes(nodes);
    }

    void Start()
    {
        enemyGroups = GetComponentsInChildren<EnemyGroup>();
    }

    public void RemoveAllThreatAreas()
    {
        foreach (MeshGenerator meshgen in GetComponentsInChildren<MeshGenerator>())
        {
            Destroy(meshgen.gameObject);
        }

        foreach (EdgeLine line in GetComponentsInChildren<EdgeLine>())
        {
            Destroy(line.gameObject);
        }
    }

    public void RemoveThreatArea(Character character)
    {
        foreach(MeshGenerator meshgen in GetComponentsInChildren<MeshGenerator>())
        {
            if (meshgen.characterLinkedTo == character)
            {
                Destroy(meshgen.gameObject);
            }
        }

        foreach (EdgeLine line in GetComponentsInChildren<EdgeLine>())
        {
            if (line.characterLinkedTo == character)
            {
                Destroy(line.gameObject);
            }
        }
    }

    public void CreateThreatArea(Character character, List<Vector3> nodesInArea)
    {
        GameObject meshGen = Instantiate(EnemyMeshGeneratorPrefab, this.transform);
        GameObject lineGen = Instantiate(EnemyLinePrefab, this.transform);
        meshGen.GetComponent<MeshGenerator>().CreateMesh(nodesInArea);
        meshGen.GetComponent<MeshGenerator>().characterLinkedTo = character;
        lineGen.GetComponent<EdgeLine>().CreateLine(nodesInArea.ToArray());
        lineGen.GetComponent<EdgeLine>().characterLinkedTo = character;
    }

    public bool SelectCharacter(string characterName)
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (group.CharacterNameLinkedTo == characterName)
            {
                group.selectRandomCharacter();
                return true;
            }
        }
        return false;
    }

    public bool hasNoMoreCharacters()
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (group.hasCharactersOut() && group.hasCharacterInCombat()) { return false; }
        }
        return true;
    }

    public EnemyActionCard[] selectEnemyActions()
    {
        List<EnemyActionCard> enemiesCurrentActions = new List<EnemyActionCard>();
        foreach (EnemyGroup EG in enemyGroups)
        {
            if (EG.hasCharactersOut() && EG.hasCharacterInCombat()) { enemiesCurrentActions.Add(EG.getNewActionCard()); }
        }
        return enemiesCurrentActions.ToArray();
    }

    public void BeginActionForGroup(EnemyActionCard card)
    {
        StartCoroutine("StartActions", card);
    }

    IEnumerator StartActions(EnemyActionCard card)
    {
        yield return new WaitForSeconds(1f);
        EnemyGroup group = GetGroupFromCard(card);
        group.beginActions();
    }

    public EnemyGroup GetGroupFromCharacter(EnemyCharacter character)
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (group.linkedCharacters.Contains(character)) { return group; }
        }
        return null;
    }

    EnemyGroup GetGroupFromCard(EnemyActionCard card)
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (card == group.currentAction) { return group; }
        }
        return null;
    }

    public void LinkSpawnedCharacter(EnemyCharacter character)
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (group.CharacterNameLinkedTo == character.CharacterName)
            {
                group.LinkCharacterToGroup(character);
            }
        }
    }

    public void CharacterDied(EnemyCharacter character)
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (group.CharacterNameLinkedTo == character.CharacterName)
            {
                group.UnLinkCharacterToGroup(character);
            }
        }
    }

    public void ShowActionCard(EnemyCharacter character)
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (group.CharacterNameLinkedTo == character.CharacterName)
            {
                group.ShowCharacter(character);
                return;
            }
        }
    }

    public bool ShowEnemyViewAreaAndCheckToFight(PlayerCharacter playerCharacter)
    {
        if (playerCharacter.HexOn.InThreatArea())
        {
            CreateCombatZone(playerCharacter);
            playerCharacter.HexOn.ThreatAreaIn.TurnIntoCombatZone(playerCharacter.myCombatZone);
            return true;
        }
        //foreach (EnemyCharacter character in enemiesOut())
        //{
        //    if (character.InCombat()) { continue; }
        //    PlayerCharacter characterFound = character.PlayerInView();
        //    if (characterFound != null)
        //    {
        //        CreateCombatZone(playerCharacter);
        //        playerCharacter.myCombatZone.AddCharacterToCombat(character);
        //        RemoveAllThreatAreas();
        //        character.ShowHexesViewingAndAlertOthersToCombat();
        //        return true;
        //    }
        //}
        return false;
    }

    public void CreateCombatZone(Character PlayerCharacter)
    {
        FindObjectOfType<CombatManager>().CreateCombatZone(PlayerCharacter);
    }

    public void takeAwayBuffs()
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            group.takeAwayBuffs();
        }
    }

    public EnemyCharacter[] enemiesOut()
    {
        List<EnemyCharacter> charactersOut = new List<EnemyCharacter>();
        foreach (EnemyGroup group in enemyGroups)
        {
            charactersOut.AddRange(group.linkedCharacters);
        }
        return charactersOut.ToArray();
    }

}

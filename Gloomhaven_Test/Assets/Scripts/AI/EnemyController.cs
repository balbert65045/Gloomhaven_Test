using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {
    // Use this for initialization
    public EnemyGroup[] enemyGroups;

    public GameObject EnemyMeshGeneratorPrefab;
    public GameObject EnemyLinePrefab;

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

    public void ShowCharactersView()
    {
        foreach (EnemyCharacter character in enemiesOut())
        {
            character.ShowViewAreaInShownHexes();
        }
    }

    public bool ShowEnemyViewAreaAndCheckToFight()
    {
        foreach (EnemyCharacter character in enemiesOut())
        {
            character.ShowViewAreaInShownHexes();
            if (character.PlayerInView())
            {
                RemoveAllThreatAreas();
                character.InCombat = true;
                character.ShowHexesViewingAndAlertOthersToCombat();
                return true;
            }
        }
        return false;
    }

    public void CheckToFight()
    {
        foreach (EnemyGroup EG in enemyGroups)
        {
            EG.CheckToPutCharacterInCombat();
        }
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

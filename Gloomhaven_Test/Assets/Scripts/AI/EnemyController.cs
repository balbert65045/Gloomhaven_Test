﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    public EnemyGroup[] enemyGroups;
    MyCameraController myCamera;
    TurnOrder turnOrder;

    int enemyGroupIndex = 0;
    public void DoEnemyActions()
    {
        StartCoroutine("DoNextEnemyCharacterAction", turnOrder.GetCurrentCharacter().GetComponent<EnemyCharacter>());
    }

    IEnumerator DoNextEnemyCharacterAction(EnemyCharacter character)
    {
        myCamera.SetTarget(character.transform);
        yield return new WaitForSeconds(.5f);
        character.PerformActionSet(character.GetGroup().CurrentActionSet);
    }

    public void CharacterEndedTurn()
    {
        turnOrder.EndTurn();
        if (turnOrder.GetCurrentCharacter().IsPlayer())
        {
            foreach (EnemyGroup eg in enemyGroups)
            {
                eg.SetNewAction();
            }
            FindObjectOfType<PlayerController>().AllowNewTurns();
        }
        else
        {
            StartCoroutine("DoNextEnemyCharacterAction", turnOrder.GetCurrentCharacter().GetComponent<EnemyCharacter>());
        } 
    }

    void Awake()
    {
        enemyGroups = GetComponentsInChildren<EnemyGroup>();
        myCamera = FindObjectOfType<MyCameraController>();
        turnOrder = FindObjectOfType<TurnOrder>();
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

    public EnemyGroup GetGroupFromCharacter(EnemyCharacter character)
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (group.linkedCharacters.Contains(character)) { return group; }
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

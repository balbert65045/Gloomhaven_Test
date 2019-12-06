using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {
    // Use this for initialization
    public EnemyGroup[] enemyGroups;

    void Start()
    {
        enemyGroups = GetComponentsInChildren<EnemyGroup>();
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
                character.InCombat = true;
                character.ShowHexesViewingAndAlertOthersToCombat();
                FindObjectOfType<PlayerController>().GoIntoCombat();
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

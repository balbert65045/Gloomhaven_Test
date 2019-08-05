using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroup : MonoBehaviour {

    public Sprite CharacterIcon;
    public string CharacterNameLinkedTo;

    public EnemyActionDeck mydeck;
    public EnemyActionCard currentAction;
    public List<EnemyCharacter> linkedCharacters = new List<EnemyCharacter>();

    public int currentCharacterIndex = 0;

    // Use this for initialization
    void Start()
    {
        EnemyActionDeck[] decks = FindObjectsOfType<EnemyActionDeck>();
        foreach (EnemyActionDeck deck in decks)
        {
            if (deck.CharacterNameLinkedTo == CharacterNameLinkedTo)
            {
                mydeck = deck;
                mydeck.SetUpDeck();
            }
        }

        EnemyCharacter[] enemyCharacters = FindObjectsOfType<EnemyCharacter>();
        foreach (EnemyCharacter character in enemyCharacters)
        {
            if (character.CharacterName == CharacterNameLinkedTo) { LinkCharacterToGroup(character); }
        }
    }

    public bool hasCharactersOut()
    {
        EnemyCharacter[] characters = FindObjectsOfType<EnemyCharacter>();
        foreach (EnemyCharacter character in characters)
        {
            if (character.CharacterName == CharacterNameLinkedTo) { return true; }
        }
        return false;
    }

    public void LinkCharacterToGroup(EnemyCharacter character)
    {
        linkedCharacters.Add(character);
    }

    public void UnLinkCharacterToGroup(EnemyCharacter character)
    {
        linkedCharacters.Remove(character);
        if (linkedCharacters.Count == 0) {
            FindObjectOfType<InitiativeBoard>().takeCharacterOffBoard(CharacterNameLinkedTo);
            mydeck.DiscardCard(currentAction);
            if (currentAction.Shuffle) { mydeck.Shuffle(); }
        }
    }

    public EnemyActionCard getNewActionCard()
    {
        currentAction = mydeck.GetRandomActionCard();
        return currentAction;
    }

    public void beginActions()
    {
        //currentAction.showCard();
        performNextCharacterAction();
    }

    public void performNextCharacterAction()
    {
        if (linkedCharacters.Count == 0) {
            endGroupTurn();
        }
        else if (currentCharacterIndex < linkedCharacters.Count)
        {
            EnemyCharacter character = linkedCharacters[currentCharacterIndex];
            if (character.hasSummonSickness())
            {
                currentCharacterIndex++;
                performNextCharacterAction();
            }
            else
            {
                StartCoroutine("WaitAndPerformCharacterAction");
            }
        }
        else
        {
            endGroupTurn();
        }
    }

    void endGroupTurn()
    {
        currentCharacterIndex = 0;
        FindObjectOfType<CharacterViewer>().HideActionCard();
        FindObjectOfType<CharacterViewer>().HideCharacterStats();
        mydeck.DiscardCard(currentAction);
        if (currentAction.Shuffle) { mydeck.Shuffle(); }
        FindObjectOfType<CombatManager>().PerformNextInInitiative();
    }

    public void takeAwayBuffs()
    {
        foreach (Character character in linkedCharacters)
        {
            character.DecreaseBuffsDuration();
            character.resetShield(character.Armor);
            character.SetSummonSickness(false);
        }
    }

    IEnumerator WaitAndPerformCharacterAction()
    {
        EnemyCharacter character = linkedCharacters[currentCharacterIndex];
        FindObjectOfType<MyCameraController>().SetTarget(character.transform);
        FindObjectOfType<CharacterViewer>().ShowCharacterStats(character.CharacterName, character.enemySprite, character);
        FindObjectOfType<CharacterViewer>().ShowActionCard(currentAction.gameObject);
        currentAction.setUpCard(character.Strength, character.Agility, character.Dexterity);
        yield return new WaitForSeconds(.5f);
        currentCharacterIndex++;
        character.PerformAction(currentAction.MovementIncrease, currentAction.DamageIncrease, currentAction.RangeIncrease, currentAction.MovementAvailable, currentAction.AttackAvailable, currentAction.HealAmount, currentAction.ShieldAmount);
    }
}

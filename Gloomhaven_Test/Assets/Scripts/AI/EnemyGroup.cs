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

    public int RandomCharacterIndex = 0;

    MyCameraController myCamera;
    CharacterViewer characterViewer;

    //public List<EnemyCharacter> charactersOut = new List<EnemyCharacter>();

    // Use this for initialization
    void Start()
    {
        myCamera = FindObjectOfType<MyCameraController>();
        characterViewer = FindObjectOfType<CharacterViewer>();
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

    public void selectRandomCharacter()
    {
        if (hasCharactersOut())
        {
            FindObjectOfType<HexVisualizer>().UnhighlightHexes();
            if (RandomCharacterIndex >= linkedCharacters.Count) { RandomCharacterIndex = 0; }
            EnemyCharacter character = linkedCharacters[RandomCharacterIndex];
            RandomCharacterIndex++;
            FindObjectOfType<HexVisualizer>().HighlightSelectionHex(character.HexOn);
            FindObjectOfType<MyCameraController>().UnLockCamera();
            FindObjectOfType<MyCameraController>().LookAt(character.transform);
            character.ShowStats();
            if (FindObjectOfType<CombatActionController>().myCombatState == CombatActionController.CombatState.UsingCombatCards)
            {
                FindObjectOfType<EnemyController>().ShowActionCard(character);
            }
        }
    }

    public bool hasCharactersOut()
    {
        return linkedCharacters.Count != 0;
    }

    public bool hasCharacterInCombat()
    {
        foreach (EnemyCharacter character in linkedCharacters)
        {
            if (character.InCombat()) { return true; }
        }
        return false;
    }

    public void LinkCharacterToGroup(EnemyCharacter character)
    {
        linkedCharacters.Add(character);
    }

    public void UnLinkCharacterToGroup(EnemyCharacter character)
    {
        if (character.myCombatZone == null) {}
        else
        {
            character.myCombatZone.removeCharacter(character);
        }
        linkedCharacters.Remove(character);

        if (linkedCharacters.Count == 0) {
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
            if (character.GetSummonSickness() || !character.InCombat())
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
        FindObjectOfType<CharacterViewer>().HideCharacterCards();
        mydeck.DiscardCard(currentAction);
        if (currentAction.Shuffle) { mydeck.Shuffle(); }
        FindObjectOfType<CombatManager>().PerformNextInInitiative();
    }

    public void takeAwayBuffs()
    {
        foreach (Character character in linkedCharacters)
        {
            character.DecreaseBuffsDuration();
            character.resetShield(character.GetArmor());
            character.SetSummonSickness(false);
        }
    }

    IEnumerator WaitAndPerformCharacterAction()
    {
        EnemyCharacter character = linkedCharacters[currentCharacterIndex];
        character.myCombatZone.ShowInitiativeBoard();
        character.myCombatZone.ShowMyCharacterAsCurrentAction(this.CharacterNameLinkedTo);
        CombatZone[] combatZones = FindObjectsOfType<CombatZone>();
        foreach (CombatZone combatZone in combatZones)
        {
            if (character.myCombatZone == combatZone) { combatZone.HideZone(); }
            else { combatZone.ShowZone(); }
        }
        myCamera.SetTarget(character.transform);
        ShowCharacter(character);
        yield return new WaitForSeconds(.5f);
        currentCharacterIndex++;
        character.PerformAction(currentAction.Movement, currentAction.Damage, currentAction.Range, currentAction.MovementAvailable, currentAction.AttackAvailable, currentAction.HealAmount, currentAction.ShieldAmount);
    }

    public void ShowCharacter(EnemyCharacter character)
    {
        character.ShowStats();
        characterViewer.ShowActionCard(currentAction.gameObject);
        currentAction.setUpCard(character.GetStrength(), character.GetAgility(), character.GetDexterity());
    }
}

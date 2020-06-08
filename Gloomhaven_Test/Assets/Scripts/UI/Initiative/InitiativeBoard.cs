using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitiativeBoard : MonoBehaviour {

    public InitiativePosition[] InitiativePositions;
    public List<InitiativePosition> ActiveInitPositions;
    int startingPosition = 0;
    int turnIndex = 0;

    public GameObject ActiveIndicator;

    public List<Vector3> PositionList = new List<Vector3>();
    public List<GameObject> InitiativeCards;

    public void PlaceIndicatorOnName(string CharacterName)
    {

    }

    static int SortByInitiative(InitiativePosition IP1, InitiativePosition IP2)
    {
        return IP1.InitValue.CompareTo(IP2.InitValue);
    }

    public void ClearInitiativeBoard()
    {
        foreach (InitiativePosition IP in ActiveInitPositions)
        {
            IP.ResetNewTurn();
        }
        ActiveInitPositions.Clear();
        ActiveIndicator.SetActive(false);
        turnIndex = 0;
    }

    public void ShowMyCharacterAsCurrentAction(string characterName)
    {
        InitiativePosition IP = FindInitPositionWithName(characterName);
        if (IP != null)
        {
            ActiveIndicator.SetActive(true);
            ActiveIndicator.transform.position = new Vector3(IP.transform.position.x, ActiveIndicator.transform.position.y, ActiveIndicator.transform.position.z);
        }
    }

    InitiativePosition FindInitPositionWithName(string name)
    {
        foreach (InitiativePosition IP in ActiveInitPositions)
        {
            if (IP.CharacterNameLinkedTo == name) { return IP; }
        }
        return null;
    }

    public void placeCharacterIcons(PlayerController playerController, EnemyGroup[] enemyGroups)
    {
        int totalCharacterTypes = playerController.myCharacters.Count + enemyGroups.Length;
        startingPosition = (InitiativePositions.Length - totalCharacterTypes) / 2;
        int nextPosition = startingPosition;
        for (int i = 0; i < totalCharacterTypes; i++)
        {
            if (i < playerController.myCharacters.Count) { PlaceCharacterOnBoard(nextPosition, playerController.myCharacters[i].characterIcon, playerController.myCharacters[i].CharacterName, true); }
            else { PlaceCharacterOnBoard(nextPosition, enemyGroups[i - playerController.myCharacters.Count].CharacterIcon, enemyGroups[i - playerController.myCharacters.Count].CharacterNameLinkedTo, false); }
            nextPosition++;
        }
    }

    public void placeCharacterIconsUsingCharacters(List<PlayerCharacter> playerCharacters, List<EnemyGroup> enemyGroups)
    {
        int totalCharacterTypes = playerCharacters.Count + enemyGroups.Count;
        startingPosition = (InitiativePositions.Length - totalCharacterTypes) / 2;
        int nextPosition = startingPosition;
        for (int i = 0; i < totalCharacterTypes; i++)
        {
            if (i < playerCharacters.Count) { PlaceCharacterOnBoard(nextPosition, playerCharacters[i].characterIcon, playerCharacters[i].CharacterName, true); }
            else { PlaceCharacterOnBoard(nextPosition, enemyGroups[i - playerCharacters.Count].CharacterIcon, enemyGroups[i - playerCharacters.Count].CharacterNameLinkedTo, false); }
            nextPosition++;
        }
    }

    public void AddCharacterIcon(PlayerCharacter playerCharacter)
    {
        startingPosition = ((InitiativePositions.Length - ActiveInitPositions.Count) / 2) - 1;
        PlaceCharacterOnBoard(startingPosition, playerCharacter.characterIcon, playerCharacter.CharacterName, true);
    }

    public void AddCharacterToBoard(EnemyGroup enemy)
    {
        PlaceCharacterOnBoard(0, enemy.CharacterIcon, enemy.CharacterNameLinkedTo, false);
    }

    public void AddCharacterFromInitPos(InitiativePosition InitPos)
    {
        PlaceCharacterOnBoard(0, InitPos.InitiativeCharacterImage.sprite, InitPos.CharacterNameLinkedTo, InitPos.player);
        AddInitiative(InitPos.CharacterNameLinkedTo, (int)InitPos.InitValue, InitPos.myCard);
    }

    void PlaceCharacterOnBoard(int position, Sprite charIcon, string characterName, bool player)
    {
        InitiativePositions[position].LinkCharacter(charIcon, characterName, player);
        InitiativePositions[position].transform.localPosition = PositionList[position];
        ActiveInitPositions.Add(InitiativePositions[position]);
    }

    public void takeCharacterOffBoard(string characterName)
    {
        for (int i = 0; i < ActiveInitPositions.Count; i++)
        {
            if (ActiveInitPositions[i].CharacterNameLinkedTo == characterName)
            {
                ActiveInitPositions[i].unLinkCharacter();
                ActiveInitPositions.Remove(ActiveInitPositions[i]);
                if (turnIndex > i) {turnIndex--;}
                break;
            }
        }
    }

    public bool AlreadyHasThisCharacter(string nameLinkedTo)
    {
        foreach (InitiativePosition initPos in ActiveInitPositions)
        {
            if (initPos.CharacterNameLinkedTo == nameLinkedTo) { return true; }
        }
        return false;
    }

    public InitiativePosition AddInitiative(string CharacterName, int Initiative, GameObject ActionCard)
    {
        foreach(InitiativePosition IP in ActiveInitPositions)
        {
            if (IP.CharacterNameLinkedTo == CharacterName) {
                IP.SetInitiative(Initiative);
                IP.AddCard(ActionCard);
                return IP;
            }
        }
        return null;
    }

    public void OrganizeInits()
    {
        ActiveInitPositions.Sort(SortByInitiative);
        for (int i = 0; i < ActiveInitPositions.Count; i++)
        {
            ActiveInitPositions[i].transform.localPosition = PositionList[startingPosition + i];
        }
    }

    public GameObject GetNextInitiativeCard()
    {
        if (turnIndex < ActiveInitPositions.Count)
        {
            GameObject card = ActiveInitPositions[turnIndex].GetCard();
            ActiveIndicator.gameObject.SetActive(true);
            ActiveIndicator.transform.position = new Vector3(ActiveInitPositions[turnIndex].transform.position.x, ActiveIndicator.transform.position.y, ActiveIndicator.transform.position.z);
            turnIndex++;
            return card;
        }
        return null;
    }

    public string GetCurrentCharacter()
    {
        return ActiveInitPositions[turnIndex].CharacterNameLinkedTo;
    }

    public void PutIndicatorOnCharacter(string name)
    {
        for (int i = 0; i < ActiveInitPositions.Count; i++)
        {
            if (ActiveInitPositions[i].CharacterNameLinkedTo == name)
            {
                ActiveIndicator.gameObject.SetActive(true);
                ActiveIndicator.transform.position = new Vector3(ActiveInitPositions[i].transform.position.x, ActiveIndicator.transform.position.y, ActiveIndicator.transform.position.z);
                turnIndex = i;
            }
        }
    }

    public void InitializeBoard()
    {
        for (int i = 0; i < InitiativePositions.Length; i++)
        {
            PositionList.Add(InitiativePositions[i].transform.localPosition);
        }
    } 

	// Use this for initialization
	void Start () {
		//for (int i = 0; i < InitiativePositions.Length; i++)
  //      {
  //          PositionList.Add(InitiativePositions[i].transform.localPosition);
  //      }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

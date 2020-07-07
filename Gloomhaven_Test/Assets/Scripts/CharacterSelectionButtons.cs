using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionButtons : MonoBehaviour {

    public CharacterSelectionButton CSBDragging;
    public void SetDraggingCharacterSelectionButton(CharacterSelectionButton CSB)
    {
        CSBDragging = CSB;
    }

    public FollowRow[] FollowRows;

    public void AddCharacterWithNoFollow(GameObject Character)
    {
        for (int i = 0; i < 4; i++)
        {
            if (!FollowRows[i].HasACharacter())
            {
                FollowRows[i].AddPlayerToRow(Character);
            }
        }
    }

    public void SetFollowing(CharacterSelectionButton CSBFollowing, CharacterSelectionButton CSBFollowed)
    {
        FollowRow rowJoining = CSBFollowed.GetComponentInParent<FollowRow>();
        FollowRow rowMovingFrom = CSBFollowing.GetComponentInParent<FollowRow>();
        CSBFollowed.SetLinked(true);
        rowJoining.AddPlayerToRow(CSBFollowing.gameObject);
        FindObjectOfType<PlayerController>().SelectCharacter(rowJoining.GetLeader().characterLinkedTo);
    }

    public void MoveCharacterOutOfFollow(CharacterSelectionButton CSB)
    {

        AddCharacterWithNoFollow(CSB.gameObject);
    }

    public void BreakAllLinks()
    {
        CharacterSelectionButton[] CSBS = GetComponentsInChildren<CharacterSelectionButton>(); 
        foreach (CharacterSelectionButton CSB in CSBS)
        {
            CSB.SetLinked(false);
            AddCharacterWithNoFollow(CSB.gameObject);
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

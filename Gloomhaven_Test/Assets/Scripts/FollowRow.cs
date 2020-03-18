using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRow : MonoBehaviour {

    public GameObject[] Positions;

    public bool HasACharacter()
    {
        return Positions[0].transform.childCount != 0;
    }

    public void AddPlayerToRow(GameObject player)
    {
        int pos = AvailablePosition();
        player.transform.SetParent(Positions[pos].transform);
        player.transform.localPosition = Vector3.zero;
    }

    void MovePlayerToPosition(GameObject player, int pos)
    {
        player.transform.SetParent(Positions[pos].transform);
        player.transform.localPosition = Vector3.zero;
    }

    public CharacterSelectionButton GetLastFollower()
    {
        CharacterSelectionButton LastFollower = null;
        for (int i = 0; i < 4; i++)
        {
            if (Positions[i].GetComponentInChildren<CharacterSelectionButton>() != null)
            {
                LastFollower = Positions[i].GetComponentInChildren<CharacterSelectionButton>();
            }
            else { break; }
        }
        return LastFollower;
    }

    int AvailablePosition()
    {
        for(int i = 0; i < 4; i++)
        {
            if (Positions[i].transform.childCount == 0) { return i; }
        }
        return -1;
    }

    public bool IsLeading(CharacterSelectionButton CSB)
    {
        foreach (CharacterSelectionButton CS in Positions[0].GetComponentsInChildren<CharacterSelectionButton>())
        {
            if (CS == CSB)
            {
                return true;
            }
        }
        return false;
    }

    public CharacterSelectionButton GetLeader()
    {
        return Positions[0].GetComponentInChildren<CharacterSelectionButton>();
    }

    public void MakeNewLeader()
    {
        if (Positions[1].transform.childCount > 0)
        {
            GameObject player = Positions[1].GetComponentInChildren<CharacterSelectionButton>().gameObject;
            player.transform.SetParent(Positions[0].transform);
            player.transform.localPosition = Vector3.zero;
            player.transform.SetAsFirstSibling();
        }
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

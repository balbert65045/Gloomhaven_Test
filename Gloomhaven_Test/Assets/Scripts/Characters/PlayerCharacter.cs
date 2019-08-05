using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{

    private PlayerController playerController;
    private bool SavingThrowUsed = false;

    // Use this for initialization
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        myHealthBar = GetComponentInChildren<HealthBar>();
        if (myHealthBar != null)
        {
            myHealthBar.CreateHealthBar(health);
        }
        maxHealth = health;
    }

    //VIEW//

    //placeholder
    public void ShowHexes()
    {
        StartCoroutine("ShowNodes");
    }
    //

    IEnumerator ShowNodes()
    {
        yield return new WaitForSeconds(.5f);
        ShowViewAreaAndCheckToFight(HexOn, ViewDistance);
    }

    public override bool ShowViewAreaAndCheckToFight(Hex hex, int distance)
    {
        List<Node> nodesAlmostSeen = HexMap.GetNodesAtDistanceFromNode(hex.HexNode, distance + 1);
        List<Node> nodesSeen = HexMap.GetNodesAtDistanceFromNode(hex.HexNode, distance);
        foreach (Node node in nodesAlmostSeen)
        {
            if (!node.Shown && nodesSeen.Contains(node))
            {
                node.GetComponent<Hex>().UnHighlight();
                node.Shown = true;
                if (node.NodeHex.EntityToSpawn != null)
                {
                    node.NodeHex.CreateCharacter();
                }
            }
            else if (!node.Shown)
            {
                node.GetComponent<Hex>().slightlyShowHex();
            }
        }

        return playerController.ShowEnemyAreaAndCheckToFight();
    }

    public override void FinishedMoving(Hex hex)
    {
        FindObjectOfType<PlayerController>().FinishedMoving();
        if (HexOn.GetComponent<Door>() != null && !HexOn.GetComponent<Door>().isOpen)
        {
            FindObjectOfType<PlayerController>().allowOpenDoor();
        }
    }

    public override void FinishedAttacking()
    {
        FindObjectOfType<PlayerController>().FinishedAttacking();
    }

    public override void FinishedHealing()
    {
        FindObjectOfType<PlayerController>().FinishedHealing();
    }

    public override void FinishedShielding()
    {
        FindObjectOfType<PlayerController>().FinishedHealing();
    }

    //DOOR
    public void OpenDoor()
    {
        if (HexOn.GetComponent<Door>() != null)
        {
            HexOn.GetComponent<Door>().OpenHexes();
            ShowViewAreaAndCheckToFight(HexOn, ViewDistance);
        }
    }

    //CARDS
    public override bool SavingThrow()
    {
        SavingThrowUsed = playerController.LoseCardInHand();
        return SavingThrowUsed;
    }

    public void SetHandSize(int size)
    {
        myHealthBar.CreateHandSize(size);
    }

    public void setNewHandSize(int size)
    {
        myHealthBar.ResetHandSize(size);
    }

    public void LoseCard()
    {
        myHealthBar.LoseCardInHand();
    }

    //Damage
    public override void GetHit()
    {
        base.GetHit();
        if (SavingThrowUsed)
        {
            SavingThrowUsed = false;
            LoseCard();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
    public Sprite characterIcon;
    public string CharacterName;

    public GameObject myDecks;
    public CombatPlayerHand GetMyCombatHand(){ return myDecks.GetComponentInChildren<CombatPlayerHand>(); }
    public OutOfCombatHand GetMyOutOfCombatHand() { return myDecks.GetComponentInChildren<OutOfCombatHand>(); }

    public CombatPlayerCard myCurrentCombatCard;
    public CombatPlayerCard GetMyCurrentCombatCard() { return myCurrentCombatCard; }
    public void SetMyCurrentCombatCard(CombatPlayerCard card) { myCurrentCombatCard = card; }

    private PlayerController playerController;
    private bool SavingThrowUsed = false;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        myHealthBar = GetComponentInChildren<HealthBar>();
        if (myHealthBar != null)
        {
            myHealthBar.CreateHealthBar(health);
        }
        maxHealth = health;

        //int cardsInHand = myDecks.GetComponentInChildren<CombatPlayerHand>().CheckCardsInHand();
        //SetHandSize(cardsInHand);
    }

    //VIEW//
    public void ShowRangeDistance(int Range)
    {
        List<Node> nodesInDistance = HexMap.GetDistanceRange(HexOn.HexNode, Range, myCT);
        foreach (Node node in nodesInDistance)
        {
            node.GetComponent<Hex>().HighlightMoveRange();
        }
    }

    public void ShowPath(Node NodeToMoveTo)
    {
        Node StartNode = HexOn.HexNode;
        Node EndNode = NodeToMoveTo;
        List<Node> NodePath = FindObjectOfType<AStar>().FindPath(StartNode, EndNode, HexMap.Map, myCT);
        foreach (Node node in NodePath)
        {
            node.GetComponent<Hex>().HighlightMoveRange();
        }
    }

    public void Selected(){ if (!GetMoving()) { HexOn.HighlightSelection(); }}

    //placeholder
    public void ShowHexes(){ StartCoroutine("ShowNodes"); }
    //

    IEnumerator ShowNodes()
    {
        yield return new WaitForSeconds(.5f);
        ShowViewArea(HexOn, ViewDistance);
    }

    public override void ShowViewArea(Hex hex, int distance)
    {
        List<Node> nodesAlmostSeen = HexMap.GetNodesAtDistanceFromNode(hex.HexNode, distance + 1, true);
        List<Node> nodesSeen = HexMap.GetNodesAtDistanceFromNode(hex.HexNode, distance, true);
        foreach (Node node in nodesAlmostSeen)
        {
            if (!node.Shown && nodesSeen.Contains(node))
            {
                node.GetComponent<Hex>().ShowHexEnd();
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

        playerController.ShowCharacterView();
    }

    public override bool CheckToFight()
    {
        return playerController.ShowEnemyAreaAndCheckToFight();
    }

    public override void FinishedMoving(Hex hex)
    {
        HexMovingTo.CharacterArrivedAtHex();
        FindObjectOfType<PlayerController>().FinishedMoving();
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
        if (HexOn.GetComponent<doorConnectionHex>() != null)
        {
            HexOn.GetComponent<doorConnectionHex>().door.OpenHexes();
            ShowViewArea(HexOn, ViewDistance);
            CheckToFight();
        }
    }

    //CARDS
    //public override bool SavingThrow()
    //{
    //    SavingThrowUsed = GetMyCombatHand().LoseRandomCard();
    //    return SavingThrowUsed;
    //}

    //public void SetHandSize(int size)
    //{
    //    myHealthBar.CreateHandSize(size);
    //}

    //public void SetNewHandSize(int size)
    //{
    //    myHealthBar.ResetHandSize(size);
    //}

    //public void LoseCard()
    //{
    //    myHealthBar.LoseCardInHand();
    //}

    //Damage
    public override void GetHit()
    {
        base.GetHit();
        //if (SavingThrowUsed)
        //{
        //    SavingThrowUsed = false;
        //    LoseCard();
        //}
    }

    public override void ShowNewMoveArea()
    {
        FindObjectOfType<CombatActionController>().ShowMoveArea();
    }

    public override void Die()
    {
        FindObjectOfType<PlayerController>().CharacterDied(this);
        base.Die();
    }

}

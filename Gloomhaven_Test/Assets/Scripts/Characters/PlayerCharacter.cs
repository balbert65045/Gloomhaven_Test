using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
    public Sprite characterIcon;
    public string CharacterName;

    public int CombatHandSize = 5;
    public GameObject[] InitialCombatCards;
    public int OutOfCombatHandSize = 5;
    public GameObject[] InitialOutOfCombatCards;

    public GameObject DeckPrefab;

    public GameObject myDecks;
    public CombatPlayerHand GetMyCombatHand(){ return myDecks.GetComponentInChildren<CombatPlayerHand>(); }
    public OutOfCombatHand GetMyOutOfCombatHand() { return myDecks.GetComponentInChildren<OutOfCombatHand>(); }

    public CombatPlayerCard myCurrentCombatCard;
    public CombatPlayerCard GetMyCurrentCombatCard() { return myCurrentCombatCard; }
    public void SetMyCurrentCombatCard(CombatPlayerCard card) { myCurrentCombatCard = card; }

    private PlayerController playerController;
    private bool SavingThrowUsed = false;

    private Door doorToOpen;
    public void SetDoorToOpen(Door door) { doorToOpen = door; }

    private CardChest ChestToOpen;
    public void SetChestToOpen(CardChest chest) { ChestToOpen = chest; }

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        myHealthBar = GetComponentInChildren<HealthBar>();
        if (myHealthBar != null)
        {
            myHealthBar.CreateHealthBar(health);
        }
        maxHealth = health;
        BuildDecks();
    }

    public void BuildDecks()
    {
        Transform deckParent = FindObjectOfType<PlayersDecks>().transform;
        GameObject Deck = Instantiate(DeckPrefab, deckParent);
        Deck.name = name + " Deck";
        Deck.SetActive(false);
        myDecks = Deck;
        GetMyCombatHand().SetHandSize(CombatHandSize);
        GetMyOutOfCombatHand().SetHandSize(OutOfCombatHandSize);

        foreach (GameObject card in InitialCombatCards)
        {
            GetMyCombatHand().AddCard(card);
        }
        foreach (GameObject card in InitialOutOfCombatCards)
        {
            GetMyOutOfCombatHand().AddCard(card);
        }
    }

    //VIEW//
    public void ShowRangeDistance(int Range)
    {
        List<Node> nodesInDistance = HexMap.GetDistanceRange(HexOn.HexNode, Range, myCT);
        foreach (Node node in nodesInDistance)
        {
           hexVisualizer.HighlightMoveRangeHex(node.NodeHex);
        }
    }

    public void ShowPath(Node NodeToMoveTo)
    {
        Node StartNode = HexOn.HexNode;
        Node EndNode = NodeToMoveTo;
        List<Node> NodePath = FindObjectOfType<AStar>().FindPath(StartNode, EndNode, myCT);
        foreach (Node node in NodePath)
        {
            hexVisualizer.HighlightMoveRangeHex(node.NodeHex);
        }
    }

    public void Selected(){ if (!GetMoving()) { hexVisualizer.HighlightSelectionHex(HexOn); }}

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
        List<Node> nodesAlmostSeen = HexMap.GetNodesAtDistanceFromNode(hex.HexNode, distance + 1);
        List<Node> nodesSeen = HexMap.GetNodesAtDistanceFromNode(hex.HexNode, distance);
        foreach (Node node in nodesAlmostSeen)
        {
            if (!node.Shown && nodesSeen.Contains(node))
            {
                node.GetComponent<Hex>().ShowHexEnd();
                hexVisualizer.UnHighlightHex(node.NodeHex);
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
        if (doorToOpen != null)
        {
            OpenDoor();
            doorToOpen = null;
        }
        else if (ChestToOpen != null)
        {
            ChestToOpen.OpenChest();
            ChestToOpen = null;
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
        if (HexOn.GetComponent<doorConnectionHex>() != null)
        {
            HexOn.GetComponent<doorConnectionHex>().door.OpenHexes();
            ShowViewArea(HexOn, ViewDistance);
            CheckToFight();
        }
    }

    //Damage
    public override void GetHit()
    {
        base.GetHit();
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

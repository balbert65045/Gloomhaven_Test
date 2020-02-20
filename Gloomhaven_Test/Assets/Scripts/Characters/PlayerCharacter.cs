using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCharacterType
{
    All = 1,
    Knight = 2,
    Barbarian = 3,
}

public class PlayerCharacter : Character
{
    public int AvailableActions = 2;
    public bool OutOfActions() { return AvailableActions <= 0; }

    public GameObject SelectionPrefab;
    public CharacterSelectionButton myCharacterSelectionButton { get; set; }

    public PlayerCharacterType myType;

    public Sprite characterIcon;
    public string CharacterName;

    public int CombatHandSize = 5;
    public GameObject[] InitialCombatCards;
    public int OutOfCombatHandSize = 5;
    public GameObject[] InitialOutOfCombatCards;

    //TODO change this to be an ID to later be able to lookup
    public List<string> CardsStored;
    public void AddCardToBeStored(string cardToBeStored) { CardsStored.Add(cardToBeStored); }

    public GameObject DeckPrefab;

    public GameObject myDecks { get; set; }
    public CombatPlayerHand GetMyCombatHand(){ return myDecks.GetComponentInChildren<CombatPlayerHand>(); }
    public OutOfCombatHand GetMyOutOfCombatHand() { return myDecks.GetComponentInChildren<OutOfCombatHand>(); }

    public CombatPlayerCard myCurrentCombatCard { get; set; }
    public CombatPlayerCard GetMyCurrentCombatCard() { return myCurrentCombatCard; }
    public void SetMyCurrentCombatCard(CombatPlayerCard card) { myCurrentCombatCard = card; }

    private PlayerController playerController;
    private bool SavingThrowUsed = false;

    public Door doorToOpen;
    public void SetDoorToOpen(Door door) { doorToOpen = door; }

    private CardChest ChestToOpen;
    public void SetChestToOpen(CardChest chest) { ChestToOpen = chest; }

    public PlayerCharacter CharacterLeading = null;
    public PlayerCharacter CharacterFollowing = null;
    public void SetFollow(PlayerCharacter character)
    {
        if (CharacterFollowing != null) { CharacterFollowing.CharacterLeading = null; }
        CharacterFollowing = character;
        if (character != null) { CharacterFollowing.CharacterLeading = this; }
    }

    public void LeaderMoved(Hex hexMovedFrom, Hex MovingTo)
    {
        if (AvailableActions > 0)
        {
            List<Node> FullPath = FindObjectOfType<AStar>().FindPath(HexOn.HexNode, hexMovedFrom.HexNode, myCT);
            int InitialDistance = FullPath.Count;
            List<Node> PathToNextArea = FindObjectOfType<AStar>().FindPath(hexMovedFrom.HexNode, MovingTo.HexNode, myCT);
            FullPath.AddRange(PathToNextArea);
            int TotalDistance = FullPath.Count;
            for (int i = 0; i < InitialDistance; i++){ FullPath.RemoveAt(TotalDistance - (i + 1)); }

            NodesInWalkingDistance.Clear();
            HexMovingTo = FullPath[0].NodeHex;
            HexMovingTo.CharacterMovingToHex();
            RemoveLinkFromHex();
            Node HexToMoveTo = FullPath[0];
            FullPath.Remove(HexToMoveTo);
            GetComponent<CharacterAnimationController>().MoveTowards(HexToMoveTo.NodeHex, FullPath);
        }
    }

    public override void MoveOnPath(Hex hex)
    {
        Hex HexMovingFrom = HexOn;
        base.MoveOnPath(hex);
        ActionUsed();
        if (CharacterFollowing != null) {
            CharacterFollowing.LeaderMoved(HexMovingFrom, hex);
            CharacterFollowing.ActionUsed();
        }
    }

    public override void ShowMoveDistance(int moveRange)
    {
        CurrentMoveRange = moveRange;
        List<Node> nodesInDistance = aStar.Diskatas(HexOn.HexNode, moveRange, myCT);
        NodesInWalkingDistance.Clear();
        if (AvailableActions <= 0 && playerController.GetPlayerState() == PlayerController.PlayerState.OutofCombat) {
            playerController.RemoveArea();
            return;
        }
        List<Node> nodesInArea = new List<Node>();
        foreach (Node node in nodesInDistance)
        {
            if (!node.Shown || node.edge) { continue; }
            nodesInArea.Add(node);
            if (node.NodeHex.EntityHolding != null) { continue; }
            NodesInWalkingDistance.Add(node);
        }
        ShowHexesOnEdgeOfRange(nodesInArea);
    }

    public void ShowHexesOnEdgeOfRange(List<Node> nodes)
    {
        List<Vector3> points = HexMap.GetHexesSurrounding(HexOn.HexNode, nodes);
        playerController.CreateArea(points, ActionType.Movement);
    }

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
        BuildCharacterSelectionIcon();
    }

    public void BuildCharacterSelectionIcon()
    {
        CharacterSelectionButtons CSBS = FindObjectOfType<CharacterSelectionButtons>();
        GameObject CSB = Instantiate(SelectionPrefab, CSBS.transform);
        CSBS.AddCharacterWithNoFollow(CSB);
        myCharacterSelectionButton = CSB.GetComponent<CharacterSelectionButton>();
        myCharacterSelectionButton.characterLinkedTo = this;
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

    public void Selected(){
        if (!GetMoving()) {
            hexVisualizer.ResetLastHex();
            hexVisualizer.HighlightSelectionHex(HexOn);
            ShowMoveDistance(CurrentMoveDistance);
        }
    }

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
        ExitHex exit = null;
        foreach (Node node in nodesAlmostSeen)
        {
            if (nodesSeen.Contains(node))
            {
                node.GetComponent<HexWallAdjuster>().ShowWall();
                if (!node.GetComponent<HexAdjuster>().FullyShown()) { node.GetComponent<HexAdjuster>().RevealRoomEdge(); }
                if (!node.Shown)
                {
                    node.GetComponent<Hex>().ShowHex();
                    node.GetComponent<HexAdjuster>().RevealRoomEdge();
                    node.GetComponent<HexWallAdjuster>().ShowWall();
                    node.GetComponent<Hex>().ShowHexEnd();
                    if (node.GetComponent<Door>() != null && node.GetComponent<Door>().door != null)
                    {
                        node.GetComponent<Door>().door.transform.parent.gameObject.SetActive(true);
                    }
                    if (node.GetComponent<ExitHex>() != null) {
                        exit = node.GetComponent<ExitHex>();
                        node.GetComponent<ExitHex>().ShowExit();
                    }
                    node.Shown = true;
                    if (node.NodeHex.EntityToSpawn != null)
                    {
                        node.NodeHex.CreateCharacter();
                    }
                }
            }
            else
            {
                if (!node.Shown) {
                    node.GetComponent<HexAdjuster>().RevealRoomEdge();
                    node.GetComponent<Hex>().slightlyShowHex();
                }
            }
        }
        if (exit != null) { exit.ShowWinArea(); }
        //playerController.ShowCharacterView();
    }

    public void Stop()
    {
        StartCoroutine("Stopping");
    }

    IEnumerator Stopping()
    {
        yield return new WaitForSeconds(.3f);
        GetComponent<CharacterAnimationController>().Stop();
    }

    public override void LastHexMovingTo()
    {
        if (CharacterFollowing != null)
        {
            CharacterFollowing.Stop();
        }
    }

    public override bool CheckToFight()
    {
        return playerController.ShowEnemyAreaAndCheckToFight();
    }

    public override void FinishedMoving(Hex hex, bool fight)
    {
        HexMovingTo.CharacterArrivedAtHex();
        FindObjectOfType<PlayerController>().FinishedMoving(this);
        if (doorToOpen != null)
        {
            if (CharacterFollowing != null)
            {
                CharacterFollowing.GetComponent<CharacterAnimationController>().Stop();
            }
            StartCoroutine("OpenDoor");
            doorToOpen = null;
        }
        else if (ChestToOpen != null && !fight)
        {
            ChestToOpen.OpenChest(this);
            ChestToOpen = null;
        }
        if (fight && playerController.myState == PlayerController.PlayerState.OutofCombat) { playerController.GoIntoCombat(); }
    }

    public override void FinishedPerformingBuff()
    {
        FindObjectOfType<PlayerController>().FinishedBuffing(this);
    }

    public override void FinishedAttacking()
    {
        base.FinishedAttacking();
        if (CharactersFinishedTakingDamage >= charactersAttackingAt.Count) { FindObjectOfType<PlayerController>().FinishedAttacking(this); }
    }

    public override void FinishedPerformingHealing()
    {
        FindObjectOfType<PlayerController>().FinishedHealing(this);
    }

    public override void FinishedPerformingShielding()
    {
        FindObjectOfType<PlayerController>().FinishedHealing(this);
    }

    //DOOR
    public IEnumerator OpenDoor()
    {
        if (HexOn.GetComponent<doorConnectionHex>() != null)
        {
            HexOn.GetComponent<doorConnectionHex>().door.OpenHexes(HexOn.HexNode.RoomName[0]);
            ShowViewArea(HexOn, ViewDistance);
            if (CheckToFight()) { playerController.GoIntoCombat(); }
            else{ playerController.ShowCharacterView(); }
        }
        yield return null;
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

    public void ActionUsed()
    {
        AvailableActions--;
        myCharacterSelectionButton.ActionUsed();
        if (AvailableActions <= 0) { GetMyOutOfCombatHand().ActionsUsedForHand(); }
    }

    public void RefreshActions()
    {
        GetMyOutOfCombatHand().RefeshActions();
        myCharacterSelectionButton.ActionsAvailable();
        AvailableActions = 2;
    }

}

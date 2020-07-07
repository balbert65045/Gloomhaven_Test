using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCharacterType
{
    All = 1,
    Knight = 2,
    Barbarian = 3,
    Mage = 4,
    Crossbow = 5,
}

public class PlayerCharacter : Character
{
    public CharacterCard CardActingWith;

    public int CharacterLevel = 1;
    public float CurrentXP = 0;
    public float XpUntilNextLevel { get { return CharacterLevel * 100 + (CharacterLevel - 1) * 50; } }

    public int AvailableActions = 2;
    public bool OutOfActions() { return AvailableActions <= 0; }

    public GameObject SelectionPrefab;
    public CharacterSelectionButton myCharacterSelectionButton { get; set; }

    public PlayerCharacterType myType;

    public Sprite characterSymbol;
    public Sprite characterIcon;
    public string CharacterName;

    public int CombatHandSize = 5;
    public GameObject[] InitialCombatCards;
    public int OutOfCombatHandSize = 5;
    public GameObject[] InitialOutOfCombatCards;

    public void AddCardToBeStored(GameObject cardToBeStored) { FindObjectOfType<GroupCardStorage>().AddCardStored(cardToBeStored, CharacterName); }
    public void AddCardToHand(GameObject cardToBeStored) { FindObjectOfType<GroupCardStorage>().AddCardHolding(cardToBeStored, CharacterName); }
    public void ReplacingCardInHand(GameObject cardToBeInHand, GameObject cardToBeOutOfHand) { FindObjectOfType<GroupCardStorage>().ReplaceCard(cardToBeInHand, cardToBeOutOfHand, CharacterName); }

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

    List<Node> NodesSeen = new List<Node>();

    bool EntityIsOnPositionAndMoving(Hex hex)
    {
        if (hex.EntityHolding.GetComponent<PlayerCharacter>() != null)
        {
            PlayerCharacter character = hex.EntityHolding.GetComponent<PlayerCharacter>();
            return character.GetMoving();
        }
        return false;
    }

    public override void MoveOnPath(Hex hex)
    {
        Hex HexMovingFrom = HexOn;
        List<Node> nodes = GetPath(hex.HexNode);
        List<Node> totalPath = new List<Node>();
        foreach(Node node in nodes) { totalPath.Add(node); }
        if (totalPath.Count == 0) {
            FinishedMoving(HexOn, true);
            return;
        }

        NodesInWalkingDistance.Clear();
        HexMovingTo = hex;
        HexMovingTo.CharacterMovingToHex();
        RemoveLinkFromHex();
        if (hex == HexOn) { FinishedMoving(hex); }
        if (nodes[0] == null) { return; }
        Node HexToMoveTo = nodes[0];
        playerController.AddHexMovingTo(nodes[nodes.Count - 1].NodeHex);
        nodes.Remove(HexToMoveTo);

        GetComponent<CharacterAnimationController>().MoveTowards(HexToMoveTo.NodeHex, nodes, HexMovingFrom);
    }

    public override void ShowMoveDistance(int moveRange)
    {
        CurrentMoveRange = moveRange;
        List<Node> nodesInDistance = aStar.Diskatas(HexOn.HexNode, moveRange, myCT);
        NodesInWalkingDistance.Clear();
        if (AvailableActions <= 0 && !InCombat()) {
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
            myHealthBar.CreateXpBar(CurrentXP / XpUntilNextLevel);
        }
        maxHealth = health;
        //BuildDecks();
        //BuildCharacterSelectionIcon();
        //BuildCharacterCards();
        ShowViewArea(HexOn, ViewDistance);
    }

    void BuildCharacterCards()
    {
        GameObject myCard = Instantiate(CharacterCardPrefab, FindObjectOfType<CharacterViewer>().transform);
        MyCharacterCard = myCard.GetComponent<CharacterCard>();
        MyCharacterCard.ShowCharacterStats(CharacterName, characterIcon, this);
        MyCharacterCard.HideCharacterStats();

        GameObject myCardActing = Instantiate(CharacterCardPrefab, FindObjectOfType<myCharacterCard>().transform);
        CardActingWith = myCardActing.GetComponent<CharacterCard>();
        CardActingWith.ShowCharacterStats(CharacterName, characterIcon, this);
        CardActingWith.HideCharacterStats();
    }

    public override void ShowStats()
    {
        FindObjectOfType<CharacterViewer>().HideCharacterCards();
        MyCharacterCard.ShowCharacterStats(CharacterName, characterIcon, this);
    }

    public void ShowStatsActingWith()
    {
        FindObjectOfType<myCharacterCard>().HideCharacterCards();
        CardActingWith.ShowCharacterStats(CharacterName, characterIcon, this);
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

    //TODo completely show edges
    public override void ShowViewArea(Hex hex, int distance)
    {
        List<Node> nodesAlmostSeen = HexMap.GetNodesInLOS(hex.HexNode, distance + 1);
        NodesSeen = HexMap.GetNodesInLOS(hex.HexNode, distance);
        ExitHex exit = null;
        List<EnemyCharacter> charactersViewUpdate = new List<EnemyCharacter>();
        List<ThreatArea> ThreatAreasToUpdate = new List<ThreatArea>();
        foreach (Node node in nodesAlmostSeen)
        {
            if (node.edge) { continue; }
            if (NodesSeen.Contains(node))
            {
                node.GetComponent<HexWallAdjuster>().ShowWall();
                if (!node.Shown)
                {
                    node.GetComponent<Hex>().ShowHex();
                    if (node.GetComponent<Door>() == null) { node.GetComponent<HexAdjuster>().RevealEdgeHexes(); }
                    if (node.NodeHex.ThreatAreaIn != null)
                    {
                        if (!ThreatAreasToUpdate.Contains(node.NodeHex.ThreatAreaIn)) { ThreatAreasToUpdate.Add(node.NodeHex.ThreatAreaIn); }
                    }
                    if (node.GetComponent<Door>() != null && node.GetComponent<Door>().door != null)
                    {
                        node.GetComponent<Door>().door.transform.parent.gameObject.SetActive(true);
                    }
                    if (node.GetComponent<ExitHex>() != null) {
                        exit = node.GetComponent<ExitHex>();
                        node.GetComponent<ExitHex>().ShowExit();
                    }
                    node.Shown = true;
                    node.NodeHex.ShowMoney();
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
        foreach(ThreatArea threatArea in ThreatAreasToUpdate) {
            threatArea.UpdateVisualArea();
        }
    }

    public override bool CheckToFight()
    {
        if (playerController.ShowEnemyAreaAndCheckToFight(this))
        {
            Debug.Log("Going into combat");
            GoIntoCombat();
            AddOtherCharactersToFightInView();
            myCombatZone.ShowPeopleInCombat();
            playerController.PlayerMovedIntoCombat();
            return true;
        }
        return false;
    }

    public void AddToFight(CombatZone CZ)
    {
        GoIntoCombat();
        CZ.AddCharacterToCombat(this);
    }

    public void AddOtherCharactersToFightInView()
    {
        foreach (Node node in myCombatZone.CombatNodes)
        {
            if (node.NodeHex.HasPlayer() && !myCombatZone.CharactersInCombat.Contains(node.NodeHex.EntityHolding.GetComponent<Character>()))
            {
                node.NodeHex.EntityHolding.GetComponent<PlayerCharacter>().AddToFight(myCombatZone);
            }
        }
    }

    void GoIntoCombat()
    {
        SwitchCombatState(true);
        myCharacterSelectionButton.BreakLink();
        myCharacterSelectionButton.showCardIndicators();
        myCharacterSelectionButton.HideActions();
    }

    public override void FinishedMoving(Hex hex, bool fight = false, Hex hexMovingFrom = null)
    {
        playerController.ClearHexesMovingTo();
        if (HexMovingTo != null) { HexMovingTo.CharacterArrivedAtHex(); }
        int goldPickedUp = HexMovingTo.PickUpMoney();
        if (goldPickedUp > 0)
        {
            GetComponent<CharacterAnimationController>().DoPickup(goldPickedUp);
        }
        if (doorToOpen != null)
        {
            StartCoroutine("OpenDoor");
            doorToOpen = null;
            return;
        }
        else if (ChestToOpen != null && !fight)
        {
            ChestToOpen.OpenChest();
            ChestToOpen = null;
        }
        FindObjectOfType<PlayerController>().FinishedMoving(this);
    }

    public void CollectGold(int amount)
    {
        playerController.AddGold(amount);
        myHealthBar.AddGold(amount);
    }

    public override void FinishedPerformingBuff()
    {
        FindObjectOfType<PlayerController>().FinishedBuffing(this);
    }

    public override void FinishedAttacking()
    {
        base.FinishedAttacking();
        if (CharactersFinishedTakingDamage >= charactersAttackingAt.Count) {
            FindObjectOfType<PlayerController>().FinishedAttacking(this);
            if (!InCombat())
            {
                foreach(Character character in charactersAttackingAt)
                {
                    if (character.myCombatZone == null) { continue; }
                    character.myCombatZone.AddCharacterToCombat(this);
                    AddOtherCharactersToFightInView();
                    myCombatZone.ShowPeopleInCombat();
                }
            }
        }
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
        yield return new WaitForSeconds(.1f);
        if (HexOn.GetComponent<doorConnectionHex>() != null)
        {
            HexOn.GetComponent<doorConnectionHex>().door.OpenHexes(HexOn.HexNode.RoomName[0]);
            if (!CheckToFight()) {
                ShowViewArea(HexOn, ViewDistance);
                playerController.ShowCharacterView();
            }
        }
        FindObjectOfType<PlayerController>().FinishedMoving(this);
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
        if (InCombat())
        {
            this.myCombatZone.removeCharacter(this);
        }
        base.Die();
    }

    float XpGaining = 0;
    public void GainXP(float XP)
    {
        if (XpGaining == 0)
        {
            StartCoroutine("GainXp");
        }
        XpGaining += XP;
    }

    IEnumerator GainXp()
    {
        yield return new WaitForSeconds(.1f);
        CurrentXP += XpGaining;
        if (CurrentXP > XpUntilNextLevel)
        {
            CurrentXP = CurrentXP - XpUntilNextLevel;
            CharacterLevel++;
            myHealthBar.LevelUpAndGainXP(CurrentXP / XpUntilNextLevel);
            if (playerController.SelectPlayerCharacter == this)
            {
                CardActingWith.LevelUpAndGainXP(this);
            }
        }
        else
        {
            myHealthBar.GainXP(CurrentXP / XpUntilNextLevel);
            if (playerController.SelectPlayerCharacter == this)
            {
                CardActingWith.GainXP(this);
            }
        }
        XpGaining = 0;
    }

    public override void SlayedEnemy(float XP)
    {
        FindObjectOfType<PlayerController>().EnemyVanquished(XP);
        base.SlayedEnemy(XP);
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

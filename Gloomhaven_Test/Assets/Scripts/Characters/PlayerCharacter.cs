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

    public PlayerCharacter CharacterLeading = null;
    public PlayerCharacter CharacterFollowing = null;

    List<Node> NodesSeen = new List<Node>();

    public void StopFollowing()
    {
        if (CharacterLeading != null) { CharacterLeading.CharacterFollowing = null; }
        if (CharacterFollowing != null) { CharacterFollowing.BreakOutOfFollow(); }
        CharacterLeading = null;
        CharacterFollowing = null;
    }

    public void SetFollow(PlayerCharacter character)
    {
        if (CharacterFollowing != null) { CharacterFollowing.CharacterLeading = null; }
        CharacterFollowing = character;
        if (character != null) { CharacterFollowing.CharacterLeading = this; }
    }

    bool EntityIsOnPositionAndMoving(Hex hex)
    {
        if (hex.EntityHolding.GetComponent<PlayerCharacter>() != null)
        {
            PlayerCharacter character = hex.EntityHolding.GetComponent<PlayerCharacter>();
            return character.GetMoving();
        }
        return false;
    }

    public void SetFollowersMoving()
    {
        SetMoving(true);
        if (CharacterFollowing != null) { CharacterFollowing.SetFollowersMoving(); }
    }


    bool CantFollow = false;
    public void LeaderMoved(Hex hexMovedFrom, List<Node> path)
    {
        if (AvailableActions > 0)
        {
            SetFollowersMoving();
            List<Node> FullPath = FindObjectOfType<AStar>().FindPath(HexOn.HexNode, hexMovedFrom.HexNode, myCT);
            FullPath.AddRange(path);
            FullPath.RemoveAt(FullPath.Count - 1);
            for (int i = FullPath.Count - 1; i >= 0; i--)
            {
                if (playerController.HexesMovingTo.Contains(FullPath[i].NodeHex))
                {
                    FullPath.RemoveAt(i);
                    CantFollow = true;
                }
                else if (FullPath[i].NodeHex.EntityHolding != null && FullPath[i].NodeHex.EntityHolding != this)
                {
                    if (!EntityIsOnPositionAndMoving(FullPath[i].NodeHex))
                    {
                        FullPath.RemoveAt(i);
                        CantFollow = true;
                    }else { break; }
                }
                else { break; }
            }
            if (FullPath.Count == 0) { return; }


            NodesInWalkingDistance.Clear();
            List<Node> PathOn = new List<Node>();
            for (int i = 0; i < FullPath.Count; i++) { PathOn.Add(FullPath[i]); }
            playerController.AddHexMovingTo(FullPath[FullPath.Count - 1].NodeHex);
            Follow(FullPath);
            ActionUsed();
            if (CharacterFollowing != null)
            {
                CharacterFollowing.LeaderMoved(HexOn, PathOn);
            }
        }
        else
        {
            BreakOutOfFollow();
        }
    }

    void BreakOutOfFollow()
    {
        if (CharacterFollowing != null) { CharacterFollowing.BreakOutOfFollow(); }
        CharacterSelectionButtons CSBS = FindObjectOfType<CharacterSelectionButtons>();
        CSBS.BreakLink(myCharacterSelectionButton);
        CSBS.AddCharacterWithNoFollow(myCharacterSelectionButton.gameObject);
        myCharacterSelectionButton.SetPosition();
    }

    void Follow(List<Node> path)
    {
        HexMovingTo = path[0].NodeHex;
        HexMovingTo.CharacterMovingToHex();
        Hex HexCurrentlyOn = HexOn;
        RemoveLinkFromHex();
        Node HexToMoveTo = path[0];
        path.Remove(HexToMoveTo);
        GetComponent<CharacterAnimationController>().MoveTowards(HexToMoveTo.NodeHex, path, HexCurrentlyOn);
    }

    public override void MoveOnPath(Hex hex)
    {
        if (CharacterLeading != null)
        {
            CharacterLeading.BreakOutOfFollow();
        }
        Hex HexMovingFrom = HexOn;
        List<Node> nodes = GetPath(hex.HexNode);
        bool movingToFight = false;
        if (!InCombat())
        {
            if (nodes[nodes.Count - 1].NodeHex.InThreatArea() || nodes[nodes.Count - 1].NodeHex.InCombatZone())
            {
                movingToFight = true;
                int start = nodes.Count - 2;
                for (int i = start; i >= 0; i--)
                {
                    if (nodes[i].NodeHex.InThreatArea() || nodes[i].NodeHex.InCombatZone()) { nodes.RemoveAt(i + 1); }
                }
            }
        }

        List<Node> totalPath = new List<Node>();
        foreach(Node node in nodes) { totalPath.Add(node); }

        if (nodes[0] == null) { return; }
        GetComponent<CharacterAnimationController>().SetMovingToFight(movingToFight);

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

        ActionUsed();
        if (CharacterFollowing != null) {
            CharacterFollowing.LeaderMoved(HexMovingFrom, totalPath);
        }
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
        BuildDecks();
        BuildCharacterSelectionIcon();
        BuildCharacterCards();
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
        if (CharacterLeading != null) { return; }
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
        if (CharacterLeading == null)
        {
            if (playerController.ShowEnemyAreaAndCheckToFight(this))
            {
                GoIntoCombat();
                AddOtherCharactersToFightInView();
                myCombatZone.ShowPeopleInCombat();
                playerController.PlayerMovedIntoCombat();
                return true;
            }
        }
        return false;
    }

    public void AddOtherCharactersToFightInView()
    {
        //Reveal others
        NodesSeen = HexMap.GetNodesInLOS(HexOn.HexNode, ViewDistance - 3);
        CombatNodes = NodesSeen;
        myCombatZone.AddNodesToCombatNodes(NodesSeen);
        foreach (Node node in NodesSeen)
        {
            if (node.NodeHex.EntityHolding != null && node.NodeHex.EntityHolding.GetComponent<PlayerCharacter>() != null)
            {
                PlayerCharacter character = node.NodeHex.EntityHolding.GetComponent<PlayerCharacter>();
                if (!character.InCombat())
                {
                    character.GoIntoCombat();
                    myCombatZone.AddCharacterToCombat(character);
                    character.AddOtherCharactersToFightInView();
                }
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
        // Used to stop follow in wierd spots
        if (CantFollow)
        {
            BreakOutOfFollow();
            CantFollow = false;
        }
        playerController.ClearHexesMovingTo();
        HexMovingTo.CharacterArrivedAtHex();
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
        if (fight && !InCombat()) {
            if (HexOn.InCombatZone())
            {
                HexOn.CombatZonesIn[0].AddCharacterToCombat(this);
                if (HexOn.CombatZonesIn.Count > 1)
                {
                    MergeCombatZones();
                }
                GoIntoCombat();
                AddOtherCharactersToFightInView();
                myCombatZone.ShowPeopleInCombat();
                playerController.PlayerMovedIntoCombat();
            }
            else
            {
                FindObjectOfType<CombatManager>().CreateCombatZone(this);
                HexOn.ThreatAreaIn.TurnIntoCombatZone(myCombatZone);
                GoIntoCombat();
                AddOtherCharactersToFightInView();
                myCombatZone.ShowPeopleInCombat();
                playerController.PlayerMovedIntoCombat();
            }
        }
        else if (InCombat())
        {
            AddOtherCharactersToFightInView();
            myCombatZone.UpdateCombatNodes();
            if (HexOn.InCombatZone() && HexOn.CombatZonesIn.Count > 1)
            {
                MergeCombatZones();
            }
        }
    }

    public void MergeCombatZones()
    {
        CombatZone[] combatZones = HexOn.CombatZonesIn.ToArray();
        foreach (CombatZone combatZone in combatZones)
        {
            if (combatZone != myCombatZone)
            {
                myCombatZone.MergeCombatZone(combatZone, this.CharacterName);
            }
        }
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

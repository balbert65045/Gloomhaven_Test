using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : Character {

    public float XpOnDeath = 10;

    public string CharacterName;
    public Sprite enemySprite;

    public bool Alerted = false;

    public List<Node> nodesInView = new List<Node>();

    private bool MoveAvailable = false;
    private bool AttackAvailable = false;

    private int HealAmount = 0;
    private int ShieldAmount = 0;

    private Hex TargetHex;
    bool attackEnabled;

    PlayerCharacter ClosestCharacter;

    public EnemyGroup GetGroup()
    {
        return FindObjectOfType<EnemyController>().GetGroupFromCharacter(this);
    }

    void Start()
    {
        myHealthBar = GetComponentInChildren<HealthBar>();
        if (myHealthBar != null)
        {
            myHealthBar.CreateHealthBar(health);
        }
        maxHealth = health;
        Shield(baseArmor, this);
        HexMap = FindObjectOfType<HexMapController>();

        BuildCharacterCard();
        //Make threat Area
        if (HexOn.CombatZonesIn.Count == 0)
        {
            AddThreatAreaOnSpawn();
        }
        //Join Combat Zone
        else
        {
            DelayedSwitchCombatState();
            HexOn.CombatZonesIn[0].AddCharacterToCombat(this);
            myCombatZone.ShowPeopleInCombat();
        }
    }

    void DestroyThreatArea()
    {
        ThreatArea[] areas = FindObjectsOfType<ThreatArea>();
        foreach(ThreatArea area in areas)
        {
            if (area.ThreatNodes.Contains(HexOn.HexNode)) { Destroy(area.gameObject); }
        }
    }

    void AddThreatAreaOnSpawn()
    {
        HexOn.ThreatAreaIn.AddEnemyCharacter(this);
    }

    void AddThreatArea()
    {
        nodesInView = HexMap.GetNodesInLOS(HexOn.HexNode, ViewDistance);
        foreach(Node node in nodesInView)
        {
            if (node.NodeHex.ThreatAreaIn != null)
            {
                //Probably need to merge threat area here
                node.NodeHex.ThreatAreaIn.AddEnemyCharacter(this);
                node.NodeHex.ThreatAreaIn.AddEnemyNodes(nodesInView);
                node.NodeHex.ThreatAreaIn.UpdateVisualArea();
                return;
            }
        }
        FindObjectOfType<EnemyController>().CreateThreatAreaShown(this, nodesInView);
    }

    void BuildCharacterCard()
    {
        GameObject myCard = Instantiate(CharacterCardPrefab, FindObjectOfType<CharacterViewer>().transform);
        MyCharacterCard = myCard.GetComponent<CharacterCard>();
        MyCharacterCard.ShowCharacterStats(CharacterName, enemySprite, this);
        MyCharacterCard.HideCharacterStats();
    }

    public override void ShowStats()
    {
        FindObjectOfType<CharacterViewer>().HideCharacterCards();
        MyCharacterCard.ShowCharacterStats(CharacterName, enemySprite, this);
    }

    //OTHER//

    void UnShowPath()
    {
        hexVisualizer.UnhighlightHexes();
    }

    public void ResetBuffs() { resetShield(GetArmor()); }

    void SetAttack(bool value) { attackEnabled = value; }

    //VIEW//

    public void RecreateThreatArea()
    {
        AddThreatArea();
    }

    // When first created show what character can see
    public void ShowViewAreaInShownHexes()
    {
        nodesInView.Clear();
        nodesInView = HexMap.GetNodesInLOS(HexOn.HexNode, ViewDistance);
        if (!InCombat())
        {
            FindObjectOfType<EnemyController>().RemoveThreatArea(this);
            List<Node> DangerZone = new List<Node>();
            foreach (Node node in nodesInView)
            {
                if (node.Shown && !node.edge) {
                    DangerZone.Add(node);
                }
            }
            List<Vector3> points = HexMap.GetHexesSurrounding(HexOn.HexNode, DangerZone);
            FindObjectOfType<EnemyController>().CreateThreatArea(this, points);
        }
    }

    //Checks if the player is in view
    public PlayerCharacter PlayerInView()
    {
        nodesInView = HexMap.GetNodesInLOS(HexOn.HexNode, ViewDistance);
        foreach (Node node in nodesInView)
        {
            if (node.NodeHex.EntityHolding != null && node.NodeHex.EntityHolding.GetComponent<PlayerCharacter>() != null) { return node.NodeHex.EntityHolding.GetComponent<PlayerCharacter>(); }
        }
        return null;
    }

    public override void finishedTakingDamage()
    {
        if (!InCombat())
        {
            if (HexOn.CombatZonesIn.Count == 0)
            {
                HexOn.CombatZonesIn[0].AddCharacterToCombat(this);
                DelayedSwitchCombatState();
            }
            else
            {
                FindObjectOfType<CombatManager>().CreateCombatZone(this);
                HexOn.ThreatAreaIn.TurnIntoCombatZone(myCombatZone);
            }
        }
        base.finishedTakingDamage();
    }

    public void DelayedSwitchCombatState()
    {
        StartCoroutine("SwitchToCombatStateInTime");
    }

    IEnumerator SwitchToCombatStateInTime()
    {
        yield return new WaitForSeconds(.1f);
        SwitchCombatState(true);
    }

    //Callbacks
    public override void Die()
    {
        FindObjectOfType<EnemyController>().CharacterDied(this);
        base.Die();
        characterThatAttackedMe.SlayedEnemy(XpOnDeath);
    }

    public override void FinishedPerformingShielding()
    {
        UnShowPath();
        CheckForHealAndBasicActions();
    }

    public override void FinishedPerformingHealing()
    {
        UnShowPath();
        CheckForBasicActions();
    }

    public override void FinishedAttacking()
    {
        base.FinishedAttacking();
        if (charactersAttackingAt == null || CharactersFinishedTakingDamage >= charactersAttackingAt.Count) { finishedActions(); }
    }

    public override void FinishedMoving(Hex hex, bool fight = false, Hex HexMovingFrom = null)
    {
        if (HexMovingTo != null) { HexMovingTo.CharacterArrivedAtHex(); }
        UnShowPath();
        LinktoHex(hex);
        CheckToAttack(ClosestCharacter);
        if (InCombat())
        {
            myCombatZone.UpdateCombatNodes();
            if (HexOn.InCombatZone() && HexOn.CombatZonesIn.Count > 1)
            {
                CombatZone[] combatZones = HexOn.CombatZonesIn.ToArray();
                foreach (CombatZone combatZone in combatZones)
                {
                    if (combatZone != myCombatZone)
                    {
                        myCombatZone.MergeCombatZone(combatZone, GetGroup().CharacterNameLinkedTo);
                    }
                }
            }
        }
    }


    //Actions//
    // action flow is as follows 
    // 1. Shield
    // 2. Heal
    // 3. Attack if in range
    // 4. Move
    // 5. Attack if in range and have not yet

    void finishedActions()
    {
        UnShowPath();
        EnemyGroup[] groups = FindObjectsOfType<EnemyGroup>();
        foreach (EnemyGroup group in groups)
        {
            if (group.CharacterNameLinkedTo == CharacterName) { group.performNextCharacterAction(); }
        }
    }

    public void PerformAction(int ActionMove, int ActionAttack, int ActionRange, bool moveAvailable, bool attackAvailable, int healAmount, int shieldAmount)
    {
        bool meleeAtack = ActionRange == 1;
        CurrentAttackRange = meleeAtack ? ActionRange : ActionRange + GetDexterity();

        CurrentAttack = ActionAttack;
        CurrentMoveRange = GetAgility() + ActionMove;

        MoveAvailable = moveAvailable;
        AttackAvailable = attackAvailable;
        HealAmount = healAmount;
        ShieldAmount = shieldAmount;

        if (shieldAmount > 0)
        {
            IEnumerator DoShieldThenOtherActionsCoroutine = DoShieldThenOtherActions();
            StartCoroutine(DoShieldThenOtherActionsCoroutine);
        }
        else
        {
            CheckForHealAndBasicActions();
        }
    }

    //Shielding 1.
    IEnumerator DoShieldThenOtherActions()
    {
        hexVisualizer.HighlightArmorPointHex(HexOn);
        yield return new WaitForSeconds(.5f);
        List<Character> characterActingUpon = new List<Character>();
        characterActingUpon.Add(GetComponent<Character>());
        GetComponent<CharacterAnimationController>().DoBuff(ActionType.Shield, ShieldAmount, 0, characterActingUpon);
        yield return null;
    }

    //Healing 2.
    void CheckForHealAndBasicActions()
    {
        if (HealAmount > 0)
        {
            IEnumerator DoHealAndOther = DoHealThenOtherActions();
            StartCoroutine(DoHealAndOther);
        }
        else
        {
            CheckForBasicActions();
        }
    }

    IEnumerator DoHealThenOtherActions()
    {
        hexVisualizer.HighlightHealRangeHex(HexOn);
        if (health != maxHealth)
        {
            yield return new WaitForSeconds(.5f);
            //TODO Change this to be able to allow someone else to heal
            Heal(HealAmount, this);
        }
        else
        {
            yield return new WaitForSeconds(.1f);
            FinishedHealing();
        }
        yield return null;
    }

    //Attack if In range 3.
    void CheckForBasicActions()
    {
        CheckToAttackFirst();
    }

    PlayerCharacter BreadthFirst()
    {
        if (myCombatZone.GetPlyaerCharacters().Count == 0) { return null; }
        List<Hex> frontier = new List<Hex>();
        frontier.Add(HexOn);
        List<Hex> visited = new List<Hex>();
        return BreadthFirstSearch(frontier, visited);
    }


    PlayerCharacter BreadthFirstSearch(List<Hex> Frontier, List<Hex> Visited)
    {
        if (Frontier.Count == 0) { return null; }
        List<Hex> newFrontier = new List<Hex>();
        foreach (Hex current in Frontier)
        {
            foreach (Node next in HexMap.GetRealNeighbors(current.HexNode))
            {
                if (!Visited.Contains(next.NodeHex))
                {
                    if (next.NodeHex.EntityHolding != null && next.NodeHex.EntityHolding.GetComponent<PlayerCharacter>() != null)
                    {
                        if (myCombatZone.GetPlyaerCharacters().Contains(next.NodeHex.EntityHolding.GetComponent<PlayerCharacter>()))
                        {
                            return next.NodeHex.EntityHolding.GetComponent<PlayerCharacter>();
                        }
                    }
                    newFrontier.Add(next.NodeHex);
                    Visited.Add(next.NodeHex);
                }
            }
        }
        return BreadthFirstSearch(newFrontier, Visited);
    }

    public void CheckToAttackFirst()
    {
        ClosestCharacter = BreadthFirst();
        if (ClosestCharacter == null) {
            Debug.Log("No character to attack");
            finishedActions();
            return;
        }
        TargetHex = ClosestCharacter.HexOn;
        GetAttackHexes(CurrentAttackRange);
        if (HexInActionRange(TargetHex))
        {
            CheckToAttack(ClosestCharacter);
        }
        else
        {
            CheckToMoveThenAttack(ClosestCharacter);
        }
    }

    //Movement 4.
    public void CheckToMoveThenAttack(PlayerCharacter CharacterToAttack)
    {
        if (CharacterToAttack == null)
        {
            finishedActions();
            return;
        }
        TargetHex = CharacterToAttack.HexOn;
        if (!HexInActionRange(TargetHex) && MoveAvailable)
        {
            IEnumerator movethenAttack = MoveThenAttack();
            StartCoroutine(movethenAttack);
        }
        else
        {
            finishedActions();
        }
    }

    IEnumerator MoveThenAttack()
    {
        List<Node> nodePath = getPathToTargettoAttack(TargetHex, CurrentAttackRange);
        if (nodePath.Count == 0) { FinishedMoving(HexOn); }
        else
        {
            hexVisualizer.HighlightSelectionHex(HexOn);
            yield return new WaitForSeconds(.5f);
            int distanceToTravel = nodePath.Count;
            if (nodePath.Count > CurrentMoveRange) { distanceToTravel = CurrentMoveRange; }
            Hex hexToMoveTo = null;
            //Loop and eliminate last node if it has something on it
            for (int i = distanceToTravel - 1; i > 0; i--)
            {
                if (nodePath[i].NodeHex.EntityHolding != null)
                {
                    distanceToTravel--;
                }
                else
                {
                    break;
                }
            }

            for (int i = 0; i < distanceToTravel; i++)
            {
                if (i == distanceToTravel - 1)
                {
                //    hexVisualizer.HighlightMovePointHex(nodePath[i].NodeHex);
                    hexToMoveTo = nodePath[i].NodeHex;
                }
                //else
                //{
                //    hexVisualizer.HighlightMoveRangeHex(nodePath[i].NodeHex);
                //}
                //yield return new WaitForSeconds(.5f);
            }

            if (nodePath.Count > distanceToTravel) { nodePath = nodePath.GetRange(0, distanceToTravel); }
            if (hexToMoveTo != null) { MoveOnPathFound(hexToMoveTo, nodePath); }
            else { Debug.LogWarning("No hex to move to"); }
        }
        yield return null;
    }

    void MoveOnPathFound(Hex hexMovingTo, List<Node> nodePath)
    {
        hexMovingTo.CharacterMovingToHex();
        Hex HexCurrentlyOn = HexOn;
        RemoveLinkFromHex();
        if (hexMovingTo == HexOn) { FinishedMoving(hexMovingTo); }
        Node NodeToMoveTo = nodePath[0];
        nodePath.Remove(NodeToMoveTo);
        GetComponent<CharacterAnimationController>().MoveTowards(NodeToMoveTo.NodeHex, nodePath, HexCurrentlyOn);
    }

    //ATTACKING 5.
    public void CheckToAttack(PlayerCharacter characterToAttack)
    {
        if (!AttackAvailable)
        {
            finishedActions();
            return;
        }
        if (characterToAttack == null)
        {
            finishedActions();
            return;
        }
        TargetHex = characterToAttack.HexOn;
        GetAttackHexes(CurrentAttackRange);
        if (HexInActionRange(TargetHex)) { StartCoroutine("ShowAttack"); }
        else { FinishedAttacking(); }
    }

    IEnumerator ShowAttack()
    {
        hexVisualizer.HighlightAttackRangeHex(HexOn);
        yield return new WaitForSeconds(.5f);
        List<Character> charactersAttacking = new List<Character>();
        charactersAttacking.Add(TargetHex.EntityHolding.GetComponent<Character>());
        foreach(Character character in charactersAttacking)
        {
            hexVisualizer.HighlightAttackAreaHex(character.HexOn);
        }
        Attack(CurrentAttack, charactersAttacking);
    }

    public void GetAttackHexes(int Range)
    {
        SetCurrentAttackRange(Range);
        List<Node> nodes = HexMap.GetNodesInLOS(HexOn.HexNode, Range);
        NodesInActionRange.Clear();
        foreach (Node node in nodes)
        {
            if (!node.Shown) { continue; }
            NodesInActionRange.Add(node);
        }
    }


    //PATHING
    public List<Node> getPathToTargettoAttack(Hex target, int range)
    {
        List<Node> possibleNodes = HexMap.GetNodesInLOS(target.HexNode, range);
        if (possibleNodes.Contains(HexOn.HexNode)){ return new List<Node> { HexOn.HexNode }; }
        Node ClosestNode = FindObjectOfType<AStar>().DiskatasWithArea(HexOn.HexNode, possibleNodes, myCT);
        if (ClosestNode != null) { return FindObjectOfType<AStar>().FindPathWithMoveLimit(HexOn.HexNode, ClosestNode, myCT, CurrentMoveRange); }
        else { return new List<Node>(); }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : Character {

    // Use this for initialization
    public string CharacterName;
    public Sprite enemySprite;

    public bool InCombat = false;
    public bool Alerted = false;

    public List<Node> nodesInView = new List<Node>();

    private bool MoveAvailable = false;
    private bool AttackAvailable = false;

    private int HealAmount = 0;
    private int ShieldAmount = 0;

    private Hex TargetHex;
    bool attackEnabled;

    PlayerCharacter ClosestCharacter;

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

        if (InCombat)
        {
            ShowHexesViewingAndAlertOthersToCombat();
        }
    }

    //OTHER//

    void UnShowPath()
    {
        hexVisualizer.UnhighlightHexes();
    }

    public void ResetBuffs() { resetShield(GetArmor()); }

    void SetAttack(bool value) { attackEnabled = value; }

    //VIEW//

    // When first created show what character can see
    public void ShowViewAreaInShownHexes()
    {
        nodesInView.Clear();
        nodesInView = HexMap.GetNodesAtDistanceFromNode(HexOn.HexNode, ViewDistance);
        if (!InCombat)
        {
            foreach (Node node in nodesInView)
            {
                if (node.Shown && !node.edge) { hexVisualizer.HighlightAttackViewArea(node.NodeHex); }
            }
        }
    }

    //Checks if the player is in view
    public bool PlayerInView()
    {
        foreach (Node node in nodesInView)
        {
            if (node.NodeHex.EntityHolding != null && node.NodeHex.EntityHolding.GetComponent<PlayerCharacter>() != null) { return true; }
        }
        return false;
    }

    public override void finishedTakingDamage()
    {
        base.finishedTakingDamage();
        if (!InCombat)
        {
            ShowHexesViewingAndAlertOthersToCombat();
        }
    }

    // Character has seen player and will show area around him and alert other enemies
    public void ShowHexesViewingAndAlertOthersToCombat()
    {
        Alerted = true;
        InCombat = true;
        StartCoroutine("ShowHexesViewingAndAlertOthersToCombatCo");
    }

    IEnumerator ShowHexesViewingAndAlertOthersToCombatCo()
    {
        yield return new WaitForSeconds(.1f);

        SwitchCombatState(true);
        List<Node> nodesAlmostSeen = HexMap.GetNodesAtDistanceFromNode(HexOn.HexNode, ViewDistance + 1);
        nodesInView.Clear();
        nodesInView = HexMap.GetNodesAtDistanceFromNode(HexOn.HexNode, ViewDistance);

        EnemyCharacter[] EnemiesOut = FindObjectOfType<EnemyController>().enemiesOut();
        foreach (EnemyCharacter character in EnemiesOut)
        {
            if (character != this && nodesInView.Contains(character.HexOn.HexNode) && !character.Alerted)
            {
                character.ShowHexesViewingAndAlertOthersToCombat();
            }
        }

        foreach (Node node in nodesAlmostSeen)
        {
            node.NodeHex.TakeAwayThreatArea();
            if (nodesInView.Contains(node))
            {
                node.GetComponent<HexWallAdjuster>().ShowWall();
                if (!node.GetComponent<HexAdjuster>().FullyShown()) { node.GetComponent<HexAdjuster>().RevealRoomEdge(); }
                if (!node.Shown)
                {
                    node.NodeHex.ShowHex();
                    node.GetComponent<Hex>().ShowHexEnd();
                    if (node.GetComponent<Door>() != null)
                    {
                        node.GetComponent<Door>().door.transform.parent.gameObject.SetActive(true);
                    }
                    node.Shown = true;
                    if (node.NodeHex.EntityToSpawn != null)
                    {
                        GameObject obj = node.NodeHex.CreateCharacter();
                        if (obj.GetComponent<EnemyCharacter>() != null)
                        {
                            obj.GetComponent<EnemyCharacter>().InCombat = true;
                        }
                    }
                }
            }
            else
            {
                if (!node.Shown)
                {
                    node.GetComponent<HexAdjuster>().RevealRoomEdge();
                    node.GetComponent<Hex>().slightlyShowHex();
                }
            }
        }
        //yield return null;
    }

    //Callbacks
    public override void Die()
    {
        FindObjectOfType<EnemyController>().CharacterDied(this);
        base.Die();
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
        finishedActions();
    }

    public override void FinishedMoving(Hex hex)
    {
        if (HexMovingTo != null) { HexMovingTo.CharacterArrivedAtHex(); }
        UnShowPath();
        LinktoHex(hex);
        CheckToAttack(ClosestCharacter);
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
        Shield(ShieldAmount, this);
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
                        return next.NodeHex.EntityHolding.GetComponent<PlayerCharacter>();
                    }
                    newFrontier.Add(next.NodeHex);
                    Visited.Add(next.NodeHex);
                }
            }
        }
        return BreadthFirstSearch(newFrontier, Visited);
    }

    //Change this to a breadth first search
    PlayerCharacter FindClosestEnemy(int AdditionalRange)
    {
        PlayerCharacter[] charactersOut = FindObjectsOfType<PlayerCharacter>();
        int PathToClosestPlayerLength = 100;
        PlayerCharacter ClosestCharacter = null;
        foreach (PlayerCharacter character in charactersOut)
        {
            if (character.GetGoingToDie()) { continue; }
            List<Node> pathToCharacter = getClosestPathToTarget(character.HexOn, CurrentAttackRange + AdditionalRange);
            if (pathToCharacter.Count == 0) {continue;}
            if (pathToCharacter.Count < PathToClosestPlayerLength)
            {
                PathToClosestPlayerLength = pathToCharacter.Count;
                ClosestCharacter = character;
            }
        }
        if (ClosestCharacter == null)
        {
            return FindClosestEnemy(AdditionalRange + 1);
        }
  
        //TODO may need to handle when No closest character
        return ClosestCharacter;
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
                    hexVisualizer.HighlightMovePointHex(nodePath[i].NodeHex);
                    hexToMoveTo = nodePath[i].NodeHex;
                }
                else
                {
                    hexVisualizer.HighlightMoveRangeHex(nodePath[i].NodeHex);
                }
                yield return new WaitForSeconds(.5f);
            }

            if (nodePath.Count > CurrentMoveRange) { nodePath = nodePath.GetRange(0, CurrentMoveRange); }
            if (hexToMoveTo != null) { MoveOnPathFound(hexToMoveTo, nodePath); }
            else { Debug.LogWarning("No hex to move to"); }
        }
        yield return null;
    }

    void MoveOnPathFound(Hex hexMovingTo, List<Node> nodePath)
    {
        hexMovingTo.CharacterMovingToHex();
        RemoveLinkFromHex();
        if (hexMovingTo == HexOn) { FinishedMoving(hexMovingTo); }
        Node NodeToMoveTo = nodePath[0];
        nodePath.Remove(NodeToMoveTo);
        GetComponent<CharacterAnimationController>().MoveTowards(NodeToMoveTo.NodeHex, nodePath);
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
        List<Node> nodes = HexMap.GetNodesAtDistanceFromNode(HexOn.HexNode, Range);
        NodesInActionRange.Clear();
        foreach (Node node in nodes)
        {
            if (!node.Shown) { continue; }
            NodesInActionRange.Add(node);
        }
    }

    public List<Node> getClosestPathToTarget(Hex target, int range)
    {
        List<Node> possibleNodes = HexMap.GetNodesInLOS(target.HexNode, range);
        if (possibleNodes.Contains(HexOn.HexNode)) { return new List<Node> { HexOn.HexNode }; }
        List<Node> OpenNodes = GetOpenNodes(possibleNodes);
        if (OpenNodes.Count > 0)
        {
            Node closestNode = FindClosestNode(OpenNodes);
            if (closestNode != null)
            {
                List<Node> nodePath = GetPath(closestNode);
                nodePath.Add(HexOn.HexNode);
                return nodePath;
            }
        }
        return new List<Node>();
    }

    //PATHING
    public List<Node> getPathToTargettoAttack(Hex target, int range)
    {
        List<Node> possibleNodes = HexMap.GetNodesInLOS(target.HexNode, range);
        if (possibleNodes.Contains(HexOn.HexNode)){ return new List<Node> { HexOn.HexNode }; }
        List<Node> OpenNodes = GetOpenNodes(possibleNodes);
        if (OpenNodes.Count > 0)
        {
            Node closestNode = FindClosestNode(OpenNodes);
            if (closestNode != null)
            {
                List<Node> nodePath = GetPath(closestNode);
                return nodePath;
            }
        }
        Debug.Log("Attempting to Increase range");
        // if that doesnt work increase range and try again until the raw distance is larger then the range
        if (FindObjectOfType<AStar>().GetRawPath(HexOn.HexNode, target.HexNode).Count - 1 > range)
        {
            return getPathToTargettoAttack(target, range + 1);
        }
        else
        {
            return new List<Node>();
        }
    }
}

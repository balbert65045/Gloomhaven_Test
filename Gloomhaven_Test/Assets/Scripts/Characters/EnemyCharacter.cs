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
        Shield(baseArmor);
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
                if (node.Shown && !node.edge) { hexVisualizer.HighlightAttackAreaHex(node.NodeHex); }
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

    // Character has seen player and will show area around him and alert other enemies
    public void ShowHexesViewingAndAlertOthersToCombat()
    {
        Alerted = true;
        StartCoroutine("ShowHexesViewingAndAlertOthersToCombatCo");
    }

    IEnumerator ShowHexesViewingAndAlertOthersToCombatCo()
    {
        yield return new WaitForSeconds(.1f);
        SwitchCombatState(true);
        List<Node> nodesAlmostSeen = HexMap.GetNodesAtDistanceFromNode(HexOn.HexNode, ViewDistance + 1);
        nodesInView.Clear();
        nodesInView = HexMap.GetNodesAtDistanceFromNode(HexOn.HexNode, ViewDistance);

        EnemyCharacter[] EnemiesOut = FindObjectsOfType<EnemyCharacter>();
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
            if (!node.Shown && nodesInView.Contains(node))
            {
                hexVisualizer.UnHighlightHex(node.NodeHex);
                node.GetComponent<Hex>().ShowHexEnd();
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
            else if (!node.Shown)
            {
                node.GetComponent<Hex>().slightlyShowHex();
            }
        }
    }

    //Callbacks
    public override void Die()
    {
        FindObjectOfType<EnemyController>().CharacterDied(this);
        base.Die();
    }

    public override void FinishedShielding()
    {
        UnShowPath();
        CheckForHealAndBasicActions();
    }

    public override void FinishedHealing()
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

        CurrentAttack = GetStrength() + ActionAttack;
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
        Shield(ShieldAmount);
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
            Heal(HealAmount);
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
        ClosestCharacter = FindClosestEnemy(0);
        if (ClosestCharacter == null) {
            Debug.Log("No character to attack");
            finishedActions();
            return;
        }
        TargetHex = ClosestCharacter.HexOn;
        GetAttackHexes(CurrentAttackRange);
        if (HexAttackable(TargetHex, CurrentAttackRange))
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
        if (!HexAttackable(TargetHex, CurrentAttackRange) && MoveAvailable)
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
            if (hexToMoveTo != null) { MoveOnPath(hexToMoveTo); }
            else { Debug.LogWarning("No hex to move to"); }
        }
        yield return null;
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
        if (HexAttackable(TargetHex, CurrentAttackRange)) { StartCoroutine("ShowAttack"); }
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
        NodesInAttackRange.Clear();
        foreach (Node node in nodes)
        {
            if (!node.Shown) { continue; }
            NodesInAttackRange.Add(node);
        }
    }

    public List<Node> getClosestPathToTarget(Hex target, int range)
    {
        List<Node> possibleNodes = HexMap.GetNodesAtDistanceFromNode(target.HexNode, range);
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
        List<Node> possibleNodes = HexMap.GetNodesAtDistanceFromNode(target.HexNode, range);
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

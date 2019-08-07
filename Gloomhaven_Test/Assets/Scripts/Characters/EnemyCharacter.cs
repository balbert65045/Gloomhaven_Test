using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : Character {

    // Use this for initialization
    public string CharacterName;
    public Sprite enemySprite;

    public int modifiedAttack;
    public int modifiedMovement;
    public int modifiedAttackRange;

    public List<Node> nodesInView = new List<Node>();


    private bool MoveAvailable = false;
    private bool AttackAvailable = false;
    private int HealAmount = 0;
    private int ShieldAmount = 0;

    private Hex TargetHex;
    bool attackEnabled;

    public bool InCombat = false;

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
        Hex[] hexes = FindObjectsOfType<Hex>();
        foreach (Hex hex in hexes)
        {
            if (hex.HexNode.Shown)
            {
                hex.UnHighlight();
            }
        }
    }

    public void ResetBuffs()
    {
        resetShield(Armor);
    }

    void SetAttack(bool value)
    {
        attackEnabled = value;
    }

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
                if (node.Shown) { node.NodeHex.HighlightAttackArea(); }
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
            if (character != this && nodesInView.Contains(character.HexOn.HexNode))
            {
                character.ShowHexesViewingAndAlertOthersToCombat();
            }
        }

        foreach (Node node in nodesAlmostSeen)
        {
            if (!node.Shown && nodesInView.Contains(node))
            {
                node.GetComponent<Hex>().UnHighlight();
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
        UnShowPath();
        LinktoHex(hex);
        CheckToAttack();
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
        modifiedAttackRange = meleeAtack ? ActionRange : ActionRange + Dexterity;

        modifiedAttack = Strength + ActionAttack;
        modifiedMovement = Agility + ActionMove;

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
        HexOn.HighlightShieldlRange();
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
        HexOn.HighlightHealRange();
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

    PlayerCharacter FindClosestEnemy()
    {
        PlayerCharacter[] charactersOut = FindObjectsOfType<PlayerCharacter>();
        int PathToClosestPlayerLength = 100;
        PlayerCharacter ClosestCharacter = null;
        foreach (PlayerCharacter character in charactersOut)
        {
            //TODO change this to current attack range
            int pathToCharacter = getPathToTarget(character.HexOn, modifiedAttackRange).Count;
            if (pathToCharacter < PathToClosestPlayerLength)
            {
                PathToClosestPlayerLength = pathToCharacter;
                ClosestCharacter = character;
            }
        }
        return ClosestCharacter;
    }

    public void CheckToAttackFirst()
    {
        TargetHex = FindClosestEnemy().HexOn;
        GetAttackHexes(modifiedAttackRange);
        if (HexAttackable(TargetHex, modifiedAttackRange))
        {
            CheckToAttack();
        }
        else
        {
            CheckToMoveThenAttack();
        }
    }

    //Movement 4.
    public void CheckToMoveThenAttack()
    {
        TargetHex = FindClosestEnemy().HexOn;
        if (!HexAttackable(TargetHex, modifiedAttackRange) && MoveAvailable)
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
        TargetHex = FindClosestEnemy().HexOn;
        List<Node> nodePath = getPathToTarget(TargetHex, modifiedAttackRange);
        HexOn.HighlightSelection();
        yield return new WaitForSeconds(.5f);
        int distanceToTravel = nodePath.Count;
        if (nodePath.Count > modifiedMovement) { distanceToTravel = modifiedMovement; }
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
                nodePath[i].NodeHex.HighlightMovePoint();
                hexToMoveTo = nodePath[i].NodeHex;
            }
            else
            {
                nodePath[i].NodeHex.HighlightMoveRange();
            }
            yield return new WaitForSeconds(.5f);
        }
        if (hexToMoveTo != null) { MoveOnPath(hexToMoveTo); }
        else { Debug.LogWarning("No hex to move to"); }
        yield return null;
    }

    //ATTACKING 5.
    public void CheckToAttack()
    {
        if (!AttackAvailable)
        {
            finishedActions();
            return;
        }
        TargetHex = FindClosestEnemy().HexOn;
        GetAttackHexes(modifiedAttackRange);
        if (HexAttackable(TargetHex, modifiedAttackRange)) { StartCoroutine("ShowAttack"); }
        else { FinishedAttacking(); }
    }

    IEnumerator ShowAttack()
    {
        HexOn.HighlightAttackRange();
        yield return new WaitForSeconds(.5f);
        List<Character> charactersAttacking = new List<Character>();
        charactersAttacking.Add(TargetHex.EntityHolding.GetComponent<Character>());
        Attack(modifiedAttack, charactersAttacking);
    }

    //PATHING
    public List<Node> getPathToTarget(Hex target, int range)
    {
        List<Node> possibleNodes = HexMap.LineOfSight(range, target);
        List<Node> OpenNodes = GetOpenNodes(possibleNodes);
        if (OpenNodes.Count > 0)
        {
            Node closestNode = FindClosestNode(OpenNodes);
            List<Node> nodePath = GetPath(closestNode);
            return nodePath;
        }
        // if that doesnt work increase range and try again until the raw distance is larger then the range
        if (FindObjectOfType<AStar>().GetRawPath(HexOn.HexNode, target.HexNode, HexMap.Map).Count - 1 > range)
        {
            return getPathToTarget(target, range + 1);
        }
        else
        {
            return new List<Node> { HexOn.HexNode };
        }
    }
}

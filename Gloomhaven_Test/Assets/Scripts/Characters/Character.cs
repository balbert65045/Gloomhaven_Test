using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    Strength = 1,
    Agility = 2,
    Dexterity = 3,
    Armor = 4,
}

[System.Serializable]
public class Buff
{
    public int Amount;
    public int Duration;
    public BuffType myBuffType;

    public Buff(int amount, int duration, BuffType buffType)
    {
        Amount = amount;
        Duration = duration;
        myBuffType = buffType;
    }
}

public class Character : Entity {

    public enum CharacterType { Good, Bad }
    public CharacterType myCT;

    public int health = 10;
    public int ViewDistance = 4;

    public int baseStrength = 0;
    public int baseAgility = 0;
    public int baseDexterity = 0;
    public int baseArmor = 0;

    protected HexVisualizer hexVisualizer;

    protected HealthBar myHealthBar;
    protected int maxHealth;

    protected List<Node> NodesInWalkingDistance = new List<Node>();
    protected List<Node> NodesInAttackRange = new List<Node>();

    protected HexMapController HexMap;
    protected AStar aStar;

    protected Hex HexMovingTo;

    protected int CurrentArmor = 0;
    public int GetCurrentArmor() { return CurrentArmor; }
    protected int CurrentAttack = 0;
    public int GetCurrentAttack() { return CurrentAttack; }
    protected int CurrentAttackRange = 0;
    public int GetCurrentAttackRange() { return CurrentAttackRange; }
    public void SetCurrentAttackRange(int range) { CurrentAttackRange = range; }
    protected int CurrentMoveRange = 0;
    public int GetCurrentMoveRange() { return CurrentMoveRange; }

    private int Agility = 0;
    public int GetAgility() { return Agility; }
    private int Strength = 0;
    public int GetStrength() { return Strength; }
    private int Dexterity = 0;
    public int GetDexterity() { return Dexterity; }
    private int Armor = 0;
    public int GetArmor() { return Armor; }
 
    private bool summonSickness = false;
    public bool GetSummonSickness() { return summonSickness; }
    public void SetSummonSickness(bool value) { summonSickness = value; }

    private Character characterThatAttackedMe;
    private List<Character> charactersAttackingAt;

    private int TotalHealthLosing;
    private bool GoingToDie = false;
    public bool GetGoingToDie() { return GoingToDie; }

    private List<Buff> Buffs = new List<Buff>();
    public List<Buff> GetBuffs() { return Buffs; }

    private bool Stealthed = false;
    public bool GetStealthed() { return Stealthed; }
    private int StealthDuration = 0;

    private bool Moving = false;
    public bool GetMoving() { return Moving; }
    public void SetMoving(bool value) { Moving = value; }

    private bool Attacking = false;
    public bool GetAttacking() { return Attacking; }
    public void SetAttacking(bool value) { Attacking = value; }

    public void ApplyBuff(int value, int duration, BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.Strength:
                AddBuff(value, duration, buffType, Strength, baseStrength);
                Strength += value;
                break;
            case BuffType.Armor:
                AddBuff(value, duration, buffType, Armor, baseArmor);
                Armor += value;
                myHealthBar.AddShield(value);
                break;
            case BuffType.Agility:
                AddBuff(value, duration, buffType, Agility, baseAgility);
                Agility += value;
                break;
            case BuffType.Dexterity:
                AddBuff(value, duration, buffType, Dexterity, baseDexterity);
                Dexterity += value;
                break;
        }
    }

    void AddBuff(int value, int duration, BuffType buffType, int Attribute, int baseAttribute)
    {
        Buff buffApplied = new Buff(value, duration, buffType);
        Buffs.Add(buffApplied);
        bool AttributeAlreadyBuffed = Attribute > baseAttribute;
        if (!AttributeAlreadyBuffed) { myHealthBar.AddBuff(buffType); }
    }

    public void DecreaseBuffsDuration()
    {
        List<Buff> buffsToRemove = new List<Buff>();
        foreach (Buff buff in Buffs)
        {
            buff.Duration--;
            if (buff.Duration <= 0) { buffsToRemove.Add(buff); }

        }
        foreach (Buff buff in buffsToRemove)
        {
            switch (buff.myBuffType)
            {
                case BuffType.Strength:
                    Strength = Strength - buff.Amount;
                    if (Strength == baseStrength) { myHealthBar.RemoveBuff(BuffType.Strength); }
                    break;
                case BuffType.Agility:
                    Agility = Agility - buff.Amount;
                    if (Agility == baseAgility) { myHealthBar.RemoveBuff(BuffType.Agility); }
                    break;
                case BuffType.Dexterity:
                    Dexterity = Dexterity - buff.Amount;
                    if (Dexterity == baseDexterity) { myHealthBar.RemoveBuff(BuffType.Dexterity); }
                    break;
                case BuffType.Armor:
                    Armor = Armor - buff.Amount;
                    if (Armor == baseArmor) { myHealthBar.RemoveBuff(BuffType.Armor); }
                    break;
            }
            Buffs.Remove(buff);
        }
    }

    void Awake()
    {
        hexVisualizer = FindObjectOfType<HexVisualizer>();
        HexMap = FindObjectOfType<HexMapController>();
        aStar = FindObjectOfType<AStar>();

        Strength = baseStrength;
        Agility = baseAgility;
        Dexterity = baseDexterity;
        Armor = baseArmor;
       
    }

    private void Start()
    {
        myHealthBar = GetComponentInChildren<HealthBar>();
        if (myHealthBar != null)
        {
            myHealthBar.CreateHealthBar(health);
        }
        Shield(baseArmor);
        maxHealth = health;
    }

    // CALLBACKS
    public virtual void ShowViewArea(Hex hex, int distance) { }

    public virtual bool CheckToFight() { return false; }

    public virtual bool SavingThrow() { return false; }

    public virtual void FinishedMoving(Hex hex) { }

    public virtual void FinishedAttacking() { }

    public virtual void FinishedHealing() { }

    public virtual void FinishedShielding() { }

    public virtual void GetHit()
    {
        transform.LookAt(characterThatAttackedMe.transform);
        GetComponent<CharacterAnimationController>().GetHit();

        int healthBeforeDamage = health;
        health -= Mathf.Clamp((TotalHealthLosing - CurrentArmor), 0, 1000);
        bool UsingSavingThrow = GetComponent<PlayerCharacter>() != null && health <= 0 ;
        if (UsingSavingThrow)
        {
            int HealthDifference = healthBeforeDamage - 1;
            TotalHealthLosing = CurrentArmor + HealthDifference;
        }
        else
        {
            if (health <= 0) { GoingToDie = true; }
        }
        myHealthBar.LoseHealth(TotalHealthLosing, UsingSavingThrow);
    }

    public void SwitchCombatState(bool InCombat)
    {
        GetComponent<CharacterAnimationController>().SwitchCombatState(InCombat);
    }

    public void SavedBySavingThrow() { health = 1; }
    public void DeadBySavingThrow() { GoingToDie = true; }


    //HEALING
    public void Heal(int amount)
    {
        myHealthBar.AddHealth(amount);
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth + 1);
    }

    public void ShowHeal(int range)
    {
        //Change this to incorporate range
        hexVisualizer.HighlightHealRangeHex(this.HexOn);
    }

    //SHIELD
    public void resetShield(int amount)
    {
        int shieldLoss = CurrentArmor - amount;
        CurrentArmor = amount;
        myHealthBar.RemoveShield(shieldLoss);
    }

    public void Shield(int amount)
    {
        CurrentArmor += amount;
        if (amount > 0) { myHealthBar.AddShield(amount); }
    }

    public void ShowShield(int Range)
    {
        hexVisualizer.HighlightArmorPointHex(this.HexOn);
    }


    //ATACKING

    public void finishedTakingDamage()
    {
        if (GoingToDie) {
            GetComponent<CharacterAnimationController>().Die();
        }
        else
        {
            characterThatAttackedMe.FinishedAttacking();
        }
    }

    public virtual void ShowNewMoveArea()
    {

    }

    public virtual void Die()
    {
        characterThatAttackedMe.FinishedAttacking();
        HexOn.RemoveEntityFromHex();
        characterThatAttackedMe.ShowNewMoveArea();
        Destroy(this.gameObject);
    }

    public void TakeDamage(int damage, string modifier, Character characterThatAttacked)
    {

        characterThatAttackedMe = characterThatAttacked;

        int totalDamage = damage;
        if (modifier[0] == "+"[0])
        {
            totalDamage += int.Parse(modifier[1].ToString());
        } else if (modifier[0] == "-"[0])
        {
            totalDamage -= int.Parse(modifier[1].ToString());
        } else if (modifier[0] == "x"[0])
        {
            totalDamage = damage * int.Parse(modifier[1].ToString());
        }

        TotalHealthLosing = totalDamage;
        myHealthBar.LoseCalculateDamage(damage, modifier, totalDamage);
    }

    public void HitOpponent()
    {
        foreach(Character character in charactersAttackingAt)
        {
            character.GetHit();
        }
    }

    public void LetAttackerAttack()
    {
        characterThatAttackedMe.MakeAttack();
    }

    public void MakeAttack()
    {
        GetComponent<CharacterAnimationController>().Attack();
    }

    public void Attack(int damage, List<Character> characters)
    {
        charactersAttackingAt = characters;
        transform.LookAt(characters[0].transform);
        foreach (Character character in characters)
        {
            character.transform.LookAt(transform);
            string modifier = GetComponent<CharacterModifierController>().GetRandomModifier();
            character.TakeDamage(damage, modifier, this);
        }
    }

    public bool HexDamageable(Hex hex)
    {
        return hex.EntityHolding != null && hex.EntityHolding.GetComponent<EnemyCharacter>() != null;
    }

    public bool HexAttackable(Hex hex, int Range)
    {
        if (CheckIfinAttackRange(hex, Range))
        {
            return true;
        }
        return false;
    }


    public void ShowAttack(int Range)
    {
        CurrentAttackRange = Range;
        List<Node> nodes = HexMap.GetNodesAtDistanceFromNode(HexOn.HexNode, Range);
        NodesInAttackRange.Clear();
        foreach (Node node in nodes)
        {
            if (!node.Shown) { continue; }
            NodesInAttackRange.Add(node);
            hexVisualizer.HighlightAttackRangeHex(node.NodeHex);
        }
    }

    public bool CheckIfinAttackRange(Hex hex, int Range)
    {
        if (NodesInAttackRange.Contains(hex.HexNode)) { return true; }
        return false;
    }

    //Stealth
    public void ReduceStealthDuration()
    {
        StealthDuration--;
        if (StealthDuration <= 0) {
            GetComponent<CharacterAnimationController>().SetStealthState(false);
            Stealthed = false;
        }
    }

    public void Stealth(int value)
    {
        Stealthed = true;
        StealthDuration = value;
        GetComponent<CharacterAnimationController>().SetStealthState(true);
    }

    // MOVEMENT//
    public bool HexInMoveRange(Hex hex, int Amount)
    {
        if (NodesInWalkingDistance.Contains(hex.HexNode)) { return true; }
        return false;

    }

    public List<Node> GetOpenNodes(List<Node> currentNodes)
    {
        List<Node> openNodes = new List<Node>();
        foreach (Node node in currentNodes)
        {
            if (node.NodeHex.EntityHolding == null) { openNodes.Add(node); }
        }

        return openNodes;
    }

    public void ShowMoveDistance(int moveRange)
    {
        CurrentMoveRange = moveRange;
        List<Node> nodesInDistance = HexMap.GetNodesAtDistanceFromNode(HexOn.HexNode, CurrentMoveRange);
        NodesInWalkingDistance.Clear();
        foreach (Node node in nodesInDistance)
        {
            if (node.NodeHex.EntityHolding != null || !node.Shown || node.edge) { continue; }
            if (aStar.FindPath(HexOn.HexNode, node, myCT).Count <= CurrentMoveRange)
            {
                NodesInWalkingDistance.Add(node);
                hexVisualizer.HighlightMoveRangeHex(node.NodeHex);
            }
        }
    }

    public Node FindClosestNode(List<Node> nodes)
    {
        int shortestPath = 1000;
        Node nodeToMoveTo = null;
        foreach(Node node in nodes)
        {
            List<Node> path = aStar.FindPath(HexOn.HexNode, node, myCT);
            int distance = path.Count;
            if (distance < shortestPath && distance != 0) {
                nodeToMoveTo = node;
                shortestPath = distance;
            }
        }
        return nodeToMoveTo;
    }

    public List<Node> GetPath(Node NodeToMoveTo)
    {
        Node StartNode = HexOn.HexNode;
        Node EndNode = NodeToMoveTo;
        return FindObjectOfType<AStar>().FindPath(StartNode, EndNode, myCT);
    }

    public void MoveOnPath(Hex hex)
    {
        HexMovingTo = hex;
        HexMovingTo.CharacterMovingToHex();
        RemoveLinkFromHex();
        if (hex == HexOn) { FinishedMoving(hex); }
        List<Node> nodes = GetPath(hex.HexNode);
        Node HexToMoveTo = nodes[0];
        nodes.Remove(HexToMoveTo);
        GetComponent<CharacterAnimationController>().MoveTowards(HexToMoveTo.NodeHex, nodes);
    }

}

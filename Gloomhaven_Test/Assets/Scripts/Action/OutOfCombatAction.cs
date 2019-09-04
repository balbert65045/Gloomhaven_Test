using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OutOfCombatActionType
{
    None = 0, 
    Scout = 1,
    BuffAttack = 2,
    BuffMove = 3,
    BuffRange = 4,
    BuffArmor = 5,
    Stealth = 6,
    Heal = 7,
}

[System.Serializable]
public struct OutOfCombatAction
{
    public OutOfCombatActionType thisActionType;
    public int Value;
    public int Duration;
    public int Range;

    public OutOfCombatAction(OutOfCombatActionType actionType, int amount, int duration, int range)
    {
        thisActionType = actionType;
        Value = amount;
        Duration = duration;
        Range = range;
    }

}

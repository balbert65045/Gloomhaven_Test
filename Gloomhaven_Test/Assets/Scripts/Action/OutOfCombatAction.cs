using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OutOfCombatActionType
{
    Scout = 1,
    BuffAttack = 2,
    BuffMove = 3,
    BuffRange = 4,
    BuffArmor = 5
}

[System.Serializable]
public struct OutOfCombatAction
{
    public OutOfCombatActionType thisActionType;
    public int Value;
    public int Duration;

    public OutOfCombatAction(OutOfCombatActionType actionType, int amount, int duration)
    {
        thisActionType = actionType;
        Value = amount;
        Duration = duration;
    }

}

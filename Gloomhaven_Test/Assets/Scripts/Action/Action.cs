using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    Movement = 1,
    Attack = 2,
    Heal = 3,
    Shield = 4,
    None = 5,
}

[System.Serializable]
public struct Action {

    public ActionType thisActionType;
    public AOE thisAOE;
    public int Range;

    public Action(ActionType actionType, AOE aoe, int amount)
    {
        thisActionType = actionType;
        thisAOE = aoe;
        Range = amount;
    }

}

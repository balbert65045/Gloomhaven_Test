using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CCSCombatCardButton : CCSCardButton
{
    public Text Init;
    public void SetInit(int InitNumber)
    {
        Init.text = InitNumber.ToString();
    }
}

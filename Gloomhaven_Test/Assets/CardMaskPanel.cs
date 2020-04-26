using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardMaskPanel : MonoBehaviour, IPointerClickHandler
{

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        transform.parent.GetComponentInChildren<CCSCard>().TakeOutOfHand();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShortRestButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject ShortRestCard;
    public GameObject Description;
    public HandCardShowArea showArea;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        ShowDescription();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (Description != null)
        {
            HideDescription();
        }
    }

    void ShowDescription()
    {
        Description = Instantiate(ShortRestCard, showArea.transform);
        Description.transform.localPosition = Vector3.zero;
        Description.transform.localScale = new Vector3(0.308f, 0.308f, 0.771f);
    }

    void HideDescription()
    {
        Destroy(Description);
    }

    // Use this for initialization
    void Start()
    {
        showArea = FindObjectOfType<HandCardShowArea>();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CCSCardButton : MonoBehaviour, IPointerEnterHandler
{
    public Text Name;
    public void SetName(string nameText)
    {
        Name.text = nameText;
    }

    public GameObject MyCardPrefab;
    GameObject MyCard;

    public void RemoveButton()
    {
        Showing = false;
        unShowCard();
        FindObjectOfType<CCSCardButtonPanel>().RemoveCardButton(this);
    }

    bool Showing = false;
    public void OnPointerEnter(PointerEventData eventData)
    {
        Showing = true;
        showCard();
    }

    void PointerExited()
    {
        Showing = false;
        unShowCard();
    }

    void showCard()
    {
        MyCard = Instantiate(MyCardPrefab, this.transform);
        MyCard.transform.localScale = new Vector3(.5f, .5f, 1.265f);
        MyCard.transform.localPosition = new Vector3(160, -83, 0);
    }

    void unShowCard()
    {
        if (MyCard != null)
        {
            Destroy(MyCard);
        }
    }

    void Update()
    {
        if (Showing)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            if (!OverThisButton(raysastResults)) { PointerExited(); }
        }
    }

    bool OverThisButton(List<RaycastResult> results)
    {
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<CCSCombatCardButton>() != null && result.gameObject.GetComponent<CCSCombatCardButton>() == this)
            {
                return true;
            }
        }
        return false;
    }
}

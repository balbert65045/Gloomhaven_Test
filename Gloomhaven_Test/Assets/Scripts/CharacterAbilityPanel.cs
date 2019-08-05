using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterAbilityPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public List<Buff> buffsForAbility = new List<Buff>();
    public BuffsPanel buffsPanel;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetUpBuffsPanel();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideBuffPanel();
    }


    public void AddBuff(Buff buff)
    {
        Debug.Log("adding Buff");
        buffsForAbility.Add(buff);
    }

    public void ClearBuffs()
    {
        buffsForAbility.Clear();
    }

    void SetUpBuffsPanel()
    {
        if (buffsForAbility.Count > 0)
        {
            buffsPanel.gameObject.SetActive(true);
            buffsPanel.SetUpBuffs(buffsForAbility);
        }
    }

    void HideBuffPanel()
    {
        buffsPanel.HideBuffs();
        buffsPanel.gameObject.SetActive(false);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

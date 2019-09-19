using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OutOfCombatCardButton : CardButton, IPointerEnterHandler, IPointerExitHandler{

    public void UnHighlight()
    {
        if (!Discarded && !Lost)
        {
            GetComponent<Image>().color = Color.white;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

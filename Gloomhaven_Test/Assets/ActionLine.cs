using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionLine : MonoBehaviour {

    public Color EnhanceColor;
    public Color DehanceColor;
    public BuffType MyActionType;

    public Text AbilityType;
    public Image AbilityImage;
    public Text AbilityAmount;

    public int ActionLineBaseAmount;

	// Use this for initialization
	void Start () {
        ActionLineBaseAmount = int.Parse(AbilityAmount.text);
    }

    public void SetUpAmount(int attribute)
    {
        if (attribute > 0)
        {
            AbilityAmount.color = EnhanceColor;
        }
        else if (attribute < 0)
        {
            AbilityAmount.color = DehanceColor;
        }
        else
        {
            AbilityAmount.color = Color.black;
        }
        AbilityAmount.text = (ActionLineBaseAmount + attribute).ToString();
    }

    public void ResetAmount()
    {
        AbilityAmount.text = ActionLineBaseAmount.ToString();
    }

    // Update is called once per frame
    void Update () {
		
	}
}

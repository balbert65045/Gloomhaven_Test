using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffArea : MonoBehaviour {

    public Text AmountText;
    public Text DurationText;
    public Image IconBuffType;

    public void SetUpBuffArea(int amount, int duration, Sprite BuffIconType)
    {
        AmountText.text = "+ " + amount.ToString();
        DurationText.text = duration.ToString();
        IconBuffType.sprite = BuffIconType;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

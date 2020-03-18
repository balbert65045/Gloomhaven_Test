using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPickerTypeButton : MonoBehaviour {

    public Button OtherButton;

    public void ButtonSelected()
    {
        GetComponent<Button>().interactable = false;
        OtherButton.interactable = true;
    }
}

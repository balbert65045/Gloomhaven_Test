using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSCharacterCardPanel : MonoBehaviour {

    public GameObject Panel;
    public Text Name;
    public Text Description;
    public GameObject BasicAttackArea;

    public GameObject Card;

    public void HidePanel()
    {
        if (Card != null) { Destroy(Card); }
        Panel.SetActive(false);
    }

    public void ShowPanel(CSCharacter character)
    {
        Panel.SetActive(true);
        Name.text = character.Name;
        Description.text = character.Description;
        Card = Instantiate(character.BasicAttackCard, BasicAttackArea.transform);
        Card.transform.localScale = new Vector3(1.17f, 1.17f, 1.17f);
        Card.transform.localPosition = Vector3.zero;
    }
}

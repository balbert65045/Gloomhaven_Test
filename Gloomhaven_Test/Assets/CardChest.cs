using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardChest : MonoBehaviour {


    public void OpenChest()
    {
        FindObjectOfType<ChestPanel>().ActivePanel();
    }

}

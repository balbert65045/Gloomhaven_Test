using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndHex : MonoBehaviour {

    public GameObject[] objectsHiding;

    public void ShowObjectsHiding()
    {
        foreach(GameObject obj in objectsHiding)
        {
            if (obj.GetComponent<Hex>() != null)
            {
                obj.GetComponent<Hex>().ShowHexEditor();
            }
            else
            {
                obj.SetActive(true);
            }
        }
    }

    public void HideObjectsHiding()
    {
        foreach (GameObject obj in objectsHiding)
        {
            if (obj.GetComponent<Hex>() != null)
            {
                obj.GetComponent<Hex>().HideHex();
            } else
            {
                obj.SetActive(false);
            }
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRaycaster : MonoBehaviour {

    public LayerMask HexLayer;
    float maxRaycastDepth = 100f; // Hard coded value

    // Setup delegates for broadcasting layer changes to other classes
    public delegate void OnCursorOverHex(Hex hex); // declare new delegate type
    public event OnCursorOverHex notifyCursorOverHexObservers; // instantiate an observer set

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, maxRaycastDepth, HexLayer);
        if (raycastHits.Length == 0) { notifyCursorOverHexObservers(null); }
        foreach (RaycastHit hit in raycastHits)
        {
            if (hit.transform.GetComponent<Hex>())
            {
                notifyCursorOverHexObservers(hit.transform.GetComponent<Hex>());
                return;
            }
        }
        //notifyCursorOverHexObservers(null);
    }
}

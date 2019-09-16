using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraRaycaster : MonoBehaviour {

    public LayerMask HexLayer;
    float maxRaycastDepth = 100f; // Hard coded value

    // Setup delegates for broadcasting layer changes to other classes
    public delegate void OnCursorOverHex(Hex hex); // declare new delegate type
    public event OnCursorOverHex notifyCursorOverHexObservers; // instantiate an observer set

    public LayerMask InterActionMask;
    public GameObject InteractableObjectOver;

    public Sprite DoorSprite;
    public Sprite ChestSprite;
    public Sprite Pointer;
    Image cursorImage;
    HexVisualizer hexVisualizer;
    PlayerController playerController;

    Vector2 cursorPoint;

    // Use this for initialization
    void Awake()
    {
        Cursor.visible = false;
        cursorImage = GetComponentInChildren<Image>();
        cursorImage.sprite = Pointer;
        hexVisualizer = FindObjectOfType<HexVisualizer>();
        playerController = FindObjectOfType<PlayerController>();
    }

    public Transform HexRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit Hit;
        if (Physics.Raycast(ray, out Hit, 100f, HexLayer)) { return Hit.transform;}
        return null;
    }

    public Transform InteractableRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit Hit;
        if (Physics.Raycast(ray, out Hit, 100f, InterActionMask)) { return Hit.transform; }
        return null;
    }

    // Update is called once per frame
    void Update () {

        cursorPoint = Input.mousePosition;
        transform.position = cursorPoint;

        Transform ActionHit = null;
        if (playerController.GetPlayerState() == PlayerController.PlayerState.OutofCombat){ ActionHit = InteractableRaycast(); }
        if (ActionHit != null)
        {
            if (InteractableObjectOver != ActionHit.gameObject)
            {
                if (ActionHit.GetComponent<DoorObject>() != null && !ActionHit.GetComponent<DoorObject>().door.isOpen)
                {
                    cursorImage.sprite = DoorSprite;
                    hexVisualizer.ShowDoorPath(ActionHit.GetComponent<DoorObject>().door);
                    InteractableObjectOver = ActionHit.gameObject;
                    return;
                }
                else if (ActionHit.GetComponent<CardChest>())
                {
                    cursorImage.sprite = ChestSprite;
                    hexVisualizer.ShowChestPath(ActionHit.GetComponent<Entity>().HexOn);
                    InteractableObjectOver = ActionHit.gameObject;
                }
            }
        }
        else
        {
            InteractableObjectOver = null;
            cursorImage.sprite = Pointer;
            Transform HexHit = HexRaycast();
            if (HexHit != null && HexHit.GetComponent<Hex>())
            {
                notifyCursorOverHexObservers(HexHit.GetComponent<Hex>());
                return;
            }
        }
    }
}

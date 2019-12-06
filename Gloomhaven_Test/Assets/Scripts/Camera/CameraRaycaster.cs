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

    public LayerMask WallMask;

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

    public Transform WallRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit Hit;
        if (Physics.Raycast(ray, out Hit, 100f, WallMask)) { return Hit.transform; }
        return null;
    }

    // Update is called once per frame
    void Update () {

        cursorPoint = Input.mousePosition;
        transform.position = cursorPoint;

        Transform ActionHit = null;
        if (playerController.GetPlayerState() == PlayerController.PlayerState.OutofCombat) { ActionHit = WallRaycast(); }
        if (ActionHit != null)
        {
            if (ActionHit.GetComponent<DoorObject>() != null && !ActionHit.GetComponent<DoorObject>().door.isOpen)
            {
                if (InteractableObjectOver != ActionHit.gameObject)
                {
                    cursorImage.sprite = DoorSprite;
                    hexVisualizer.ShowDoorPath(ActionHit.GetComponent<DoorObject>().door);
                    InteractableObjectOver = ActionHit.gameObject;
                }
                return;
            }
        }

        if (playerController.GetPlayerState() == PlayerController.PlayerState.OutofCombat){ ActionHit = InteractableRaycast(); }
        if (ActionHit != null)
        {
            if (ActionHit.GetComponent<CardChest>() && !ActionHit.GetComponent<CardChest>().isOpen)
            {
                if (InteractableObjectOver != ActionHit.gameObject)
                {
                    cursorImage.sprite = ChestSprite;
                    hexVisualizer.ShowChestPath(ActionHit.GetComponent<Entity>().HexOn);
                    InteractableObjectOver = ActionHit.gameObject;
                }
                return;
            }
        }
        InteractableObjectOver = null;
        Transform HexHit = HexRaycast();
        if (HexHit != null && HexHit.GetComponent<Hex>())
        {
            if (HexHit.GetComponent<Door>() != null && !HexHit.GetComponent<Door>().isOpen) { cursorImage.sprite = DoorSprite; }
            else { cursorImage.sprite = Pointer; }
            notifyCursorOverHexObservers(HexHit.GetComponent<Hex>());
            return;
        }
        cursorImage.sprite = Pointer;
    }
}

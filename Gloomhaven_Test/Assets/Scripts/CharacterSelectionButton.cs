using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterSelectionButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler {

    public PlayerCharacter characterLinkedTo;
    private PlayerController playerController;
    private Color OGcolor;

    public Sprite ActionAvailable;
    public Sprite ActionUnavailable;

    public Image Action1;
    public Image Action2;

    public Sprite NoCardSprite;
    public Sprite HasCardSprite;
    public Image CardIndicatorImage;

    public bool CharacterDead = false;
    public void SetCharacterDeadValue(bool value) { CharacterDead = value;  }

    bool Dragging = false;
    public bool Linked = false;
    Vector3 StartPosition;

    public List<CharacterSelectionButton> AdjacentCharacters = new List<CharacterSelectionButton>();

    CharacterSelectionButtons CSBS;
    GraphicRaycaster m_raycaster;

    // Use this for initialization
    void Start () {
        playerController = FindObjectOfType<PlayerController>();
        OGcolor = GetComponent<Image>().color;
        CardIndicatorImage.gameObject.SetActive(false);
        StartPosition = transform.position;
        CSBS = FindObjectOfType<CharacterSelectionButtons>();
        m_raycaster = FindObjectOfType<Canvas>().GetComponent<GraphicRaycaster>();
    }

    public void BreakLink()
    {
        SetLinked(false);
        CSBS.AddCharacterWithNoFollow(this.gameObject);
        characterLinkedTo.SetFollow(null);
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (!characterLinkedTo.InCombat())
        {
            AdjacentCharacters.Clear();
            Dragging = true;
            transform.parent.SetAsLastSibling();
            transform.GetComponentInParent<FollowRow>().transform.SetAsLastSibling();
            CSBS.SetDraggingCharacterSelectionButton(this);
            RevealAdjacentCharactersToFollow();
        }
    }

    void RevealAdjacentCharactersToFollow()
    {
        HexMapController hexmap = FindObjectOfType<HexMapController>();
        List<Node> adjacentNodes = hexmap.GetNodesAdjacent(characterLinkedTo.HexOn.HexNode);
        CharacterSelectionButton[] AllButtons = FindObjectsOfType<CharacterSelectionButton>();
        foreach (CharacterSelectionButton button in AllButtons)
        {
            if (adjacentNodes.Contains(button.characterLinkedTo.HexOn.HexNode) && button.characterLinkedTo.CharacterFollowing == null) { AdjacentCharacters.Add(button); }
            else { button.GetComponent<Button>().interactable = false; }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Dragging)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            m_raycaster.Raycast(eventData, results);
            
            Dragging = false;
            CSBS.SetDraggingCharacterSelectionButton(null);
            CharacterSelectionButton FollowingButton = GetButtonFromResults(results);
            if (FollowingButton != null && AdjacentCharacters.Contains(FollowingButton))
            {
                CSBS.SetFollowing(this, FollowingButton);
                Linked = true;
                SetPosition();
            }
            else
            {
                if (!Linked) { CSBS.AddCharacterWithNoFollow(gameObject); }
                SetPosition();
            }

            CharacterSelectionButton[] AllButtons = FindObjectsOfType<CharacterSelectionButton>();
            foreach (CharacterSelectionButton button in AllButtons) { button.GetComponent<Button>().interactable = true; }
        }
    }

    public void SetLinked(bool value)
    {
        Linked = value;
    }

    public void SetPosition()
    {
        transform.localPosition = Vector3.zero;
        StartPosition = transform.position;
    }

    CharacterSelectionButton GetButtonFromResults(List<RaycastResult> results)
    {
        foreach(RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<CharacterSelectionButton>() != null)
            {
                if (result.gameObject.GetComponent<CharacterSelectionButton>() != this)
                {
                    return result.gameObject.GetComponent<CharacterSelectionButton>();
                }
            }
        }
        return null;
    }

    private void Update()
    {
        if (Dragging)
        {
            transform.position = Input.mousePosition;
            if (Linked)
            {
                if ((transform.position - StartPosition).magnitude > 30f)
                {
                    Linked = false;
                    CSBS.BreakLink(this);
                }
            }
        }
    }

    public void Disable()
    {
        GetComponent<Button>().interactable = false;
    }

    public void Enable()
    {
        GetComponent<Button>().interactable = true;
    }

    public void SelectCharacter()
    {
        if (GetComponentInParent<FollowRow>().IsLeading(this))
        {
            playerController.SelectCharacter(characterLinkedTo);
        }
        else
        {

            GetComponentInParent<FollowRow>().GetLeader().SelectCharacter();
        }
    }

    public void CharacterSelected()
    {
        GetComponent<Image>().color = Color.green;
        CharacterSelectionButton[] otherButtons = FindObjectsOfType<CharacterSelectionButton>();
        foreach (CharacterSelectionButton button in otherButtons)
        {
            if (button != this) { button.ReturnToColor(); }
        }
    }

    public void ReturnToColor()
    {
        GetComponent<Image>().color = OGcolor;
    }

    public void ShowActions()
    {
        Action1.gameObject.SetActive(true);
        Action2.gameObject.SetActive(true);
    }

    public void HideActions()
    {
        Action1.gameObject.SetActive(false);
        Action2.gameObject.SetActive(false);
    }

    public void ActionsAvailable()
    {
        Action1.sprite = ActionAvailable;
        Action2.sprite = ActionAvailable;
    }

    public void ActionUsed()
    {
        if (Action1.sprite != ActionUnavailable) { Action1.sprite = ActionUnavailable; }
        else { Action2.sprite = ActionUnavailable; }
    }

    public void showCardIndicators()
    {
        CardIndicatorImage.gameObject.SetActive(true);
    }

    public void hideCardIndicators()
    {
        CardIndicatorImage.sprite = NoCardSprite;
        CardIndicatorImage.gameObject.SetActive(false);
    }

    public void CardForCharacterSelected()
    {
        CardIndicatorImage.sprite = HasCardSprite;
    }

    public void CardForCharacterUnselected()
    {
        CardIndicatorImage.sprite = NoCardSprite;
    }
	
}

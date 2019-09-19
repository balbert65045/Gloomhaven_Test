using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexVisualizer : MonoBehaviour {

    private CameraRaycaster cameraRaycaster;
    private PlayerController playerController;
    private CombatActionController combatcontroller;
    private OutOfCombatActionController outOfCombatcontroller;
    private HexMapController hexMap;
    private List<Hex> LastHexesChanged = new List<Hex>();
    private Hex LastHexOver;

    public void UnhighlightHexes()
    {
        Hex[] hexes = hexMap.AllHexes;
        foreach (Hex hex in hexes)
        {
            if (hex.HexNode.Shown)
            {
                hex.UnHighlight();
            }
        }
    }

    public void ReturntHexesToPreviousColor()
    {
        Hex[] hexes = hexMap.AllHexes;
        foreach (Hex hex in hexes)
        {
            if (hex.HexNode.Shown)
            {
                hex.returnToPreviousColor();
            }
        }
    }

    public void UnHighlightHex(Hex hex)
    {
        hex.UnHighlight();
    }

    public void DeactivateHex(Hex hex)
    {
        hex.DeactivateHex();
    }

    public void HighlightMoveRangeHex(Hex hex)
    {
        hex.HighlightMoveRange();
    }

    public void HighlightMovePointHex(Hex hex)
    {
        hex.HighlightMovePoint();
    }

    public void HighlightAttackRangeHex(Hex hex)
    {
        hex.HighlightAttackRange();
    }

    public void HighlightAttackAreaHex(Hex hex)
    {
        hex.HighlightAttackArea();
    }

    public void HighlightHealRangeHex(Hex hex)
    {
        hex.HighlightHealRange();
    }

    public void HighlightHealPointHex(Hex hex)
    {
        hex.HighlightHealRPoint();
    }

    public void HighlightArmorPointHex(Hex hex)
    {
        hex.HighlightShieldlRange();
    }

    public void HighlightSelectionHex(Hex hex)
    {
        hex.HighlightSelection();
    }

    public void ShowChestPath(Hex ChestHex)
    {
        if (outOfCombatcontroller.LookingInChest || outOfCombatcontroller.cardUsing != null) { return; }
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        if (myCharacter.GetMoving()) { return; }
        Node ClosestNode = GetClosestPathToAdjacentHexes(myCharacter, ChestHex);
        if (ClosestNode != null)
        {
            ClearLastChangedHexes();
            HighlightMovePath(ClosestNode.NodeHex);
        }
    }

    public void ShowDoorPath(Door doorHex)
    {
        if (outOfCombatcontroller.LookingInChest || outOfCombatcontroller.cardUsing != null) { return; }
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        if (myCharacter == null || myCharacter.GetMoving()) { return; }
        if (myCharacter.HexOn == doorHex.GetComponent<Hex>())
        {
            ClearLastChangedHexes();
            return;
        }
        if (doorHex.GetComponent<Node>().isAvailable)
        {
            ClearLastChangedHexes();
            HighlightMovePath(doorHex.GetComponent<Hex>());
        }
        else
        {
            Node ClosestNode = GetClosestPathToAdjacentHexes(myCharacter, doorHex.GetComponent<Hex>());
            if (ClosestNode != null)
            {
                ClearLastChangedHexes();
                HighlightMovePath(ClosestNode.NodeHex);
            }
        }
    }

    public Node GetClosestPathToAdjacentHexes(Character character, Hex hex)
    {
        Node closestNode = FindObjectOfType<HexMapController>().GetClosestNodeFromNeighbors(hex, character);
        if (closestNode == character.HexOn.HexNode) { ClearLastChangedHexes(); }
        return closestNode;
    }

    public void HighlightMovePath(Hex hex)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        List<Node> NodePath = myCharacter.GetPath(hex.HexNode);
        if (NodePath == null) { return; }
        foreach (Node node in NodePath)
        {
            LastHexesChanged.Add(node.NodeHex);
            node.NodeHex.HighlightMovePoint();
        }
    }

    public void HighlightAttackArea(Hex hex)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        List<Node> nodesInAOE = FindObjectOfType<HexMapController>().GetAOE(combatcontroller.GetMyCurrectAction().thisAOE.thisAOEType, myCharacter.HexOn.HexNode, hex.HexNode);
        foreach (Node node in nodesInAOE)
        {
            if (node == null) { break; }
            node.NodeHex.HighlightAttackArea();
            LastHexesChanged.Add(node.NodeHex);
        }
    } 

    public void ClearLastChangedHexes()
    {
        if (LastHexesChanged.Count != 0)
        {
            foreach (Hex lastHex in LastHexesChanged)
            {
                if (lastHex == playerController.SelectPlayerCharacter.HexOn) { continue; }
                lastHex.returnToPreviousColor();
            }
            LastHexesChanged.Clear();
        }
    }

    void ClearLastChangedHexSelf()
    {
        if (LastHexesChanged.Count != 0)
        {
            foreach (Hex lastHex in LastHexesChanged)
            {
                if (lastHex == playerController.SelectPlayerCharacter.HexOn) {
                    lastHex.HighlightSelection() ;
                }
                else { lastHex.returnToPreviousColor(); }
            }
            LastHexesChanged.Clear();
        }
    }

    public void OnHexChanged(Hex hex)
    {
        
        if (LastHexOver == hex) {return;}
        LastHexOver = hex;

        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;

        if (playerController.GetPlayerState() == PlayerController.PlayerState.OutofCombat)
        {
            if (myCharacter.GetMoving()) { return; }
            if (outOfCombatcontroller.LookingInChest) { return; }
            if (outOfCombatcontroller.cardUsing == null)
            {
                ClearLastChangedHexes();
                if (hex == null || !hex.HexNode.Shown) { return; }
                HighlightMovePath(hex);
            }
            else
            {
                switch (outOfCombatcontroller.cardUsing.cardAbility.Actions[0].thisActionType)
                {
                    case ActionType.Scout:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case ActionType.Stealth:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case ActionType.BuffAttack:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case ActionType.BuffMove:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case ActionType.BuffRange:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case ActionType.BuffArmor:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case ActionType.Heal:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                }
            }
        }
        else if (playerController.GetPlayerState() == PlayerController.PlayerState.InCombat && combatcontroller.GetCombatState() == CombatActionController.CombatState.UsingCombatCards)
        {
            if (hex == null || !hex.HexNode.Shown) { return; }
            switch (combatcontroller.GetMyCurrectAction().thisActionType)
            {
                case ActionType.Movement:
                {
                        if (myCharacter.GetMoving()) { return; }
                        if (LastHexesChanged.Count != 0)
                        {
                            foreach (Hex lastHex in LastHexesChanged)
                            {
                                if (myCharacter.HexInMoveRange(lastHex, myCharacter.GetCurrentMoveRange()))
                                {
                                    // The hex my character is on keep OG color
                                    if (myCharacter.HexOn == lastHex) { lastHex.UnHighlight(); }
                                    // else go back to color
                                    else { lastHex.HighlightMoveRange(); }
                                }
                                else
                                {
                                    lastHex.UnHighlight();
                                }
                            }
                            LastHexesChanged.Clear();
                        }
                        if (myCharacter.HexInMoveRange(hex, myCharacter.GetCurrentMoveRange()))
                        {
                            HighlightMovePath(hex);
                        }
                        break;
                }
                case ActionType.Attack:
                {
                        if (combatcontroller.Attacking) { return; }
                        if (myCharacter.CheckIfinAttackRange(hex, myCharacter.GetCurrentAttackRange()))
                        {
                            if (LastHexesChanged.Count != 0) {
                                foreach (Hex lastHex in LastHexesChanged)
                                {
                                    if (myCharacter.CheckIfinAttackRange(lastHex, myCharacter.GetCurrentAttackRange()))
                                    {
                                        lastHex.HighlightAttackRange();
                                    }
                                    else
                                    {
                                        lastHex.UnHighlight();
                                    }
                                }
                                LastHexesChanged.Clear();
                            }
                            HighlightAttackArea(hex);
                        }
                        else
                        {
                            if (LastHexesChanged.Count != 0)
                            {
                                foreach (Hex lastHex in LastHexesChanged)
                                {
                                    if (myCharacter.CheckIfinAttackRange(lastHex, myCharacter.GetCurrentAttackRange()))
                                    {
                                        lastHex.HighlightAttackRange();
                                    }
                                    else
                                    {
                                        lastHex.UnHighlight();
                                    }
                                }
                                LastHexesChanged.Clear();
                            }
                        }
                        break;
                }
                case ActionType.Heal:
                {

                    break;
                }
                case ActionType.Shield:
                {

                    break;
                }
            }
        }
    }

    // Use this for initialization
    void Start () {
        outOfCombatcontroller = GetComponent<OutOfCombatActionController>();
        combatcontroller = GetComponent<CombatActionController>();
        playerController = GetComponent<PlayerController>();
        hexMap = FindObjectOfType<HexMapController>();
        cameraRaycaster = FindObjectOfType<CameraRaycaster>();
        cameraRaycaster.notifyCursorOverHexObservers += OnHexChanged;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

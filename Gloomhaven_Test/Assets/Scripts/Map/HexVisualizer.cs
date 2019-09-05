using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexVisualizer : MonoBehaviour {

    private CameraRaycaster cameraRaycaster;
    private PlayerController playerController;
    private CombatActionController combatcontroller;
    private OutOfCombatActionController outOfCombatcontroller;
    //private Character myCharacter;
    private List<Hex> LastHexesChanged = new List<Hex>();
    private Hex LastHexOver;


    public void HighlightMovePath(Hex hex)
    {
        PlayerCharacter myCharacter = playerController.SelectPlayerCharacter;
        List<Node> NodePath = myCharacter.GetPath(hex.HexNode);
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

    void ClearLastChangedHexes()
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
            if (myCharacter.Moving) { return; }
            if (outOfCombatcontroller.cardUsing == null)
            {
                ClearLastChangedHexes();
                if (hex == null || !hex.HexNode.Shown) { return; }
                HighlightMovePath(hex);
            }
            else
            {
                switch (outOfCombatcontroller.cardUsing.actions[0].thisActionType)
                {
                    case OutOfCombatActionType.Scout:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case OutOfCombatActionType.Stealth:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case OutOfCombatActionType.BuffAttack:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case OutOfCombatActionType.BuffMove:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case OutOfCombatActionType.BuffRange:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case OutOfCombatActionType.BuffArmor:
                        ClearLastChangedHexSelf();
                        if (hex == myCharacter.HexOn)
                        {
                            hex.HighlightHealRPoint();
                            LastHexesChanged.Add(hex);
                        }
                        break;
                    case OutOfCombatActionType.Heal:
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
        else if (playerController.GetPlayerState() == PlayerController.PlayerState.InCombat && playerController.GetCombatState() == PlayerController.CombatState.UsingCombatCards)
        {
            if (hex == null || !hex.HexNode.Shown) { return; }
            switch (combatcontroller.GetMyCurrectAction().thisActionType)
            {
                case ActionType.Movement:
                {
                        if (myCharacter.Moving) { return; }
                        if (LastHexesChanged.Count != 0)
                        {
                            foreach (Hex lastHex in LastHexesChanged)
                            {
                                if (myCharacter.HexInMoveRange(lastHex, myCharacter.CurrentMoveRange))
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
                        if (myCharacter.HexInMoveRange(hex, myCharacter.CurrentMoveRange))
                        {
                            HighlightMovePath(hex);
                        }
                        break;
                }
                case ActionType.Attack:
                {
                        if (combatcontroller.Attacking) { return; }
                        if (myCharacter.CheckIfinAttackRange(hex, myCharacter.CurrentAttackRange))
                        {
                            if (LastHexesChanged.Count != 0) {
                                foreach (Hex lastHex in LastHexesChanged)
                                {
                                    if (myCharacter.CheckIfinAttackRange(lastHex, myCharacter.CurrentAttackRange))
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
                                    if (myCharacter.CheckIfinAttackRange(lastHex, myCharacter.CurrentAttackRange))
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
        //myCharacter = playerController.myCharacter;
        cameraRaycaster = FindObjectOfType<CameraRaycaster>();
        cameraRaycaster.notifyCursorOverHexObservers += OnHexChanged;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

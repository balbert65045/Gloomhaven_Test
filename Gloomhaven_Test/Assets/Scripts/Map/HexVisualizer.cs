using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexVisualizer : MonoBehaviour {

    private CameraRaycaster cameraRaycaster;
    private PlayerController playerController;
    private CombatActionController combatcontroller;
    private Character myCharacter;
    private List<Hex> LastHexesChanged = new List<Hex>();
    private Hex LastHexOver;

    void OnHexChanged(Hex hex)
    {
        if (LastHexOver == hex) { return; }
        LastHexOver = hex;

        if (playerController.GetPlayerState() == PlayerController.PlayerState.OutofCombat)
        {
            if (myCharacter.GetComponent<CharacterAnimationController>().Moving) { return; }
            if (hex == null || !hex.HexNode.Shown) {
                foreach (Hex lastHex in LastHexesChanged)
                {
                    lastHex.returnToPreviousColor();
                }
                LastHexesChanged.Clear();
                return;
            }
            if (LastHexesChanged.Count != 0)
            {
                foreach (Hex lastHex in LastHexesChanged)
                {
                        lastHex.returnToPreviousColor();
                }
                LastHexesChanged.Clear();
            }
            List<Node> NodePath = myCharacter.GetPath(hex.HexNode);
            foreach (Node node in NodePath)
            {
                node.GetComponent<Hex>().HighlightMovePoint();
                LastHexesChanged.Add(node.NodeHex);
                node.NodeHex.HighlightMovePoint();
            }
            
        }
        else if (playerController.GetPlayerState() == PlayerController.PlayerState.InCombat && playerController.GetCombatState() == PlayerController.CombatState.UsingCombatCards)
        {
            if (hex == null || !hex.HexNode.Shown) { return; }
            switch (combatcontroller.GetMyCurrectAction().thisActionType)
            {
                case ActionType.Movement:
                {
                        if (myCharacter.GetComponent<CharacterAnimationController>().Moving) { return; }
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
                            List<Node> NodePath = myCharacter.GetPath(hex.HexNode);
                            foreach (Node node in NodePath)
                            {
                                node.GetComponent<Hex>().HighlightMovePoint();
                                LastHexesChanged.Add(node.NodeHex);
                            }
                            hex.HighlightMovePoint();
                            //LastHexesChanged.Add(hex);
                        }
                        break;
                }
                case ActionType.Attack:
                {
                        if (myCharacter.CheckIfinAttackRange(hex, myCharacter.CurrentAttackRange) && !combatcontroller.Attacking)
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
                            List<Node> nodesInAOE = FindObjectOfType<HexMapController>().GetAOE(combatcontroller.GetMyCurrectAction().thisAOE.thisAOEType, myCharacter.HexOn.HexNode, hex.HexNode);
                            foreach (Node node in nodesInAOE)
                            {
                                if (node == null) { break; }
                                node.NodeHex.HighlightAttackArea();
                                LastHexesChanged.Add(node.NodeHex);
                            }
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
        combatcontroller = GetComponent<CombatActionController>();
        playerController = GetComponent<PlayerController>();
        myCharacter = playerController.myCharacter;
        cameraRaycaster = FindObjectOfType<CameraRaycaster>();
        cameraRaycaster.notifyCursorOverHexObservers += OnHexChanged;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreatArea : MonoBehaviour {

    public List<Node> ThreatNodes = new List<Node>();
    public void AddNodesToThreatNodes(List<Node> nodes)
    {
        foreach (Node node in nodes)
        {
            if (node.edge) { continue; }
            if (!ThreatNodes.Contains(node))
            {
                ThreatNodes.Add(node);
                node.NodeHex.ThreatAreaIn = this;
            }
        }
    }
    public List<Node> ThreatAreasShown()
    {
        List<Node> nodes = new List<Node>();
        foreach (Node node in ThreatNodes)
        {
            if (node.Shown) { nodes.Add(node); }
        }
        return nodes;
    }

    public List<EnemyCharacter> EnemiesInThreatZone = new List<EnemyCharacter>();
    public void AddEnemyCharacter(EnemyCharacter enemyCharacter)
    {
        EnemiesInThreatZone.Add(enemyCharacter);
    }
    public void AddEnemyNodes(List<Node> nodes)
    {
        AddNodesToThreatNodes(nodes);
    }

    MeshGenerator ThreatZoneMeshGen;
    EdgeLine ThreatZoneEdgeLine;
    public void UpdateVisualArea()
    {
        if (ThreatZoneMeshGen == null && ThreatZoneEdgeLine == null)
        {
            ThreatZoneMeshGen = GetComponentInChildren<MeshGenerator>();
            ThreatZoneEdgeLine = GetComponentInChildren<EdgeLine>();
        }
        ThreatZoneMeshGen.DeleteMesh();
        ThreatZoneEdgeLine.DestroyLine();
        List<Vector3> points = FindObjectOfType<HexMapController>().GetHexesSurrounding(ThreatAreasShown()[0], ThreatAreasShown());
        ThreatZoneMeshGen.CreateMesh(points);
        ThreatZoneEdgeLine.CreateLine(points.ToArray());
    }

    public void TurnIntoCombatZone(CombatZone combatZone)
    {
        foreach(EnemyCharacter character in EnemiesInThreatZone)
        {
            character.DelayedSwitchCombatState();
            combatZone.AddCharacterToCombat(character);
        }
        combatZone.AddNodesToCombatNodes(ThreatNodes);
        ShowNodesInArea();
        Destroy(this.gameObject);
    }

    void ShowNodesInArea()
    {
        ExitHex exit = null;
        foreach (Node node in ThreatNodes)
        {
            if (node.edge) { continue; }
            node.GetComponent<HexWallAdjuster>().ShowWall();
            if (!node.Shown)
            {
                node.NodeHex.ShowHex();
                node.GetComponent<HexAdjuster>().RevealEdgeHexes();
                node.Shown = true;
                if (node.GetComponent<Door>() != null)
                {
                    if (node.GetComponent<Door>().door != null) { node.GetComponent<Door>().door.transform.parent.gameObject.SetActive(true); }
                }
                if (node.GetComponent<ExitHex>() != null)
                {
                    exit = node.GetComponent<ExitHex>();
                    node.GetComponent<ExitHex>().ShowExit();
                }
                if (node.NodeHex.EntityToSpawn != null) { node.NodeHex.CreateCharacter(); }
            }
        }
        if (exit != null) { exit.ShowWinArea(); }
    }
}

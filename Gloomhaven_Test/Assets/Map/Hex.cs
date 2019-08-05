using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour {

    public Entity EntityToSpawn;
    public float EntityOffset = 0.1f;
    public Entity EntityHolding;
    public bool Hidden = true;


    public Material SelectionHighlightMaterial;
    public Material MovePointHighlightMaterial;
    public Material MoveRangeHighlightMaterial;
    public Material AttackAreaHighlightMaterial;
    public Material AttackRangeHighlightMaterial;
    public Material HealPointHighlightMaterial;
    public Material HealRangeHighlightMaterial;
    public Material ShieldRangeHighlightMaterial;

    public Material InvisibleMaterial;
    public Material SlightlyVisibleMaterial;

    public Material OGMaterial;

    public Material previousMaterial;

    public bool BlockingLineOfSight = false;

    public Node HexNode;

    private void Start()
    {
        previousMaterial = OGMaterial;
        HexNode = GetComponent<Node>();
        if (!HexNode.Shown)
        {
            HideHex();
        }
    }

    public void ShowHexEditor()
    {
        GetComponent<MeshRenderer>().sharedMaterial = OGMaterial;
    }

    public void HideHexEditor()
    {
        GetComponent<MeshRenderer>().sharedMaterial = InvisibleMaterial;
    }

    public void HideHex()
    {
        GetComponent<MeshRenderer>().material = InvisibleMaterial;
    }

    public void slightlyShowHex()
    {
        GetComponent<MeshRenderer>().material = SlightlyVisibleMaterial;
    }

    public bool HasSameType(Character.CharacterType CT)
    {
        if (EntityHolding != null && EntityHolding.GetComponent<Character>() != null)
        {
            return EntityHolding.GetComponent<Character>().myCT == CT;
        }

        return true;
    }


    public GameObject CreateCharacter()
    {
        GameObject objectMade = GenerateCharacter();
        if (EntityHolding.GetComponent<EnemyCharacter>() != null)
        {
            FindObjectOfType<EnemyController>().LinkSpawnedCharacter(EntityHolding.GetComponent<EnemyCharacter>());
        }
        return objectMade;
    }

    public GameObject GenerateCharacter()
    {
        if (EntityToSpawn != null)
        {
            Vector3 StartPos = new Vector3(transform.position.x, transform.position.y + EntityOffset, transform.position.z);
            Vector3 startingRot = new Vector3(0, -90, 0);
            EntityHolding = Instantiate(EntityToSpawn.gameObject, StartPos, Quaternion.Euler(startingRot)).GetComponent<Entity>();
            EntityHolding.StartOnHex(this);
        }
        return EntityHolding.gameObject;
    }

    public void AddEntityToHex(Entity entity)
    {
        EntityHolding = entity;
    }

    public void RemoveEntityFromHex()
    {
        EntityHolding = null;
    }

    public void HighlightSelection()
    {
       // if (SelectionHighlightMaterial != GetComponent<MeshRenderer>().material) { previousMaterial = GetComponent<MeshRenderer>().material; }
        GetComponent<MeshRenderer>().material = SelectionHighlightMaterial;
    }

    public void HighlightShieldlRange()
    {
        //if (ShieldRangeHighlightMaterial != GetComponent<MeshRenderer>().material) { previousMaterial = GetComponent<MeshRenderer>().material; }
        GetComponent<MeshRenderer>().material = ShieldRangeHighlightMaterial;
    }

    public void HighlightHealRPoint()
    {
        //if (HealPointHighlightMaterial != GetComponent<MeshRenderer>().material) { previousMaterial = GetComponent<MeshRenderer>().material; }
        GetComponent<MeshRenderer>().material = HealPointHighlightMaterial;
    }

    public void HighlightHealRange()
    {
        //if (HealRangeHighlightMaterial != GetComponent<MeshRenderer>().material) { previousMaterial = GetComponent<MeshRenderer>().material; }
        GetComponent<MeshRenderer>().material = HealRangeHighlightMaterial;
    }

    public void HighlightMovePoint()
    {
        //if (MovePointHighlightMaterial != GetComponent<MeshRenderer>().material) { previousMaterial = GetComponent<MeshRenderer>().material; }
        GetComponent<MeshRenderer>().material = MovePointHighlightMaterial;
    }

    public void HighlightMoveRange()
    {
        //if (MoveRangeHighlightMaterial != GetComponent<MeshRenderer>().material) { previousMaterial = GetComponent<MeshRenderer>().material; }
        GetComponent<MeshRenderer>().material = MoveRangeHighlightMaterial;
    }

    public void HighlightAttackRange()
    {
        GetComponent<MeshRenderer>().material = AttackRangeHighlightMaterial;
        previousMaterial = GetComponent<MeshRenderer>().material;
    }

    public void HighlightAttackArea()
    {
        GetComponent<MeshRenderer>().material = AttackAreaHighlightMaterial;
        previousMaterial = GetComponent<MeshRenderer>().material;
    }

    public void returnToPreviousColor()
    {
        GetComponent<MeshRenderer>().material = previousMaterial;
    }

    public void UnHighlight()
    {
        //if (OGMaterial != GetComponent<MeshRenderer>().material) { previousMaterial = GetComponent<MeshRenderer>().material; }
        GetComponent<MeshRenderer>().material = OGMaterial;
        previousMaterial = GetComponent<MeshRenderer>().material;
    }

}

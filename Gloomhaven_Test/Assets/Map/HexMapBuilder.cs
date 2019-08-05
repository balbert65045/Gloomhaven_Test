using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapBuilder : MonoBehaviour {

    public Transform HexPrefab;

    public int gridHeight = 11;
    public int gridWidth = 11;

    float hexHeight = 2.0f;
    float hexWidth = 1.732f;
    public float gap = 0.0f;

    Vector3 startPos;

    

    public void BuildMap()
    {
        //This is used to stop it from continually increasing 
        hexHeight = 2.0f;
        hexWidth = 1.732f;
        AddGap();
        CalculateStartPos();
        CreateGrid();
    }

    public void DestroyMap()
    {
        Hex[] hexes = GetComponentsInChildren<Hex>();
        if (hexes.Length <= 0) { return; }
        foreach (Hex hex in hexes)
        {
            DestroyImmediate(hex.gameObject);
        }
    }
	
	void AddGap()
    {
        hexWidth += hexWidth * gap;
        hexHeight += hexHeight * gap;
    }

    void CalculateStartPos()
    {
        float offset = 0;
        if (gridHeight / 2 % 2 != 0)
        {
            offset = hexWidth / 2;
        }

        float x = hexWidth * (gridWidth / 2) - offset;
        float z = hexHeight * .75f * (gridHeight / 2);

        startPos = new Vector3(x, 0, z);
    }

    void CreateGrid()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Transform hex = Instantiate(HexPrefab) as Transform;
                Vector2 gridPos = new Vector2(x, y);
                hex.position = CalculateWorldPos(gridPos);
                hex.SetParent(this.transform);
                hex.name = "Hex " + x + "|" + y;

                // Node stuff
                hex.GetComponent<Node>().SetNode(x, y);

            }
        }
    }

    Vector3 CalculateWorldPos(Vector2 gridPos)
    {
        float offset = 0;
        if (gridPos.y % 2 != 0)
        {
            offset = hexWidth / 2;
        }

        float x = startPos.x + gridPos.x * hexWidth + offset;
        float z = startPos.z - gridPos.y * hexHeight *.75f;

        return new Vector3(x, 0, z);
    } 

}

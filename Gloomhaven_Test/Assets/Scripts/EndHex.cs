using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndHex : MonoBehaviour {

    public GameObject[] objectsHiding;
    public LayerMask WallLayer;

    public void ShowObjectsHiding()
    {
        foreach (Wall wall in WallsNearAndHiding())
        {
            wall.ShowWall();
        }
    }

    public void HideObjectsHiding()
    {
        foreach (Wall wall in WallsNearAndHiding())
        {
            wall.setInvisible();
        }
    }

    public List<Wall> WallsNearAndHiding()
    {
        List<Wall> wallsHiding = new List<Wall>();
        Vector3 North = new Vector3(2, 2, 0).normalized;
        Ray ray = new Ray(transform.position, North);
        RaycastHit[] Hits = (Physics.RaycastAll(ray, 3, WallLayer));
        foreach (RaycastHit Hit in Hits)
        {
            if (Hit.transform.GetComponentInParent<Wall>())
            {
                wallsHiding.Add(Hit.transform.GetComponentInParent<Wall>());
            }
        }

        Vector3 NorthEast = new Vector3(1, 2, 2).normalized;
        ray = new Ray(transform.position, NorthEast);
        Hits = (Physics.RaycastAll(ray, 3, WallLayer));
        foreach (RaycastHit Hit in Hits)
        {
            if (Hit.transform.GetComponentInParent<Wall>())
            {
                wallsHiding.Add(Hit.transform.GetComponentInParent<Wall>());
            }
        }

        Vector3 NorthWest = new Vector3(1, 2, -2).normalized;
        ray = new Ray(transform.position, NorthWest);
        Hits = (Physics.RaycastAll(ray, 3, WallLayer));
        foreach (RaycastHit Hit in Hits)
        {
            if (Hit.transform.GetComponentInParent<Wall>())
            {
                wallsHiding.Add(Hit.transform.GetComponentInParent<Wall>());
            }
        }

        Vector3 South = new Vector3(-2, 2, 0).normalized;
        ray = new Ray(transform.position, South);
        Hits = (Physics.RaycastAll(ray, 3, WallLayer));
        foreach (RaycastHit Hit in Hits)
        {
            if (Hit.transform.GetComponentInParent<Wall>())
            {
                wallsHiding.Add(Hit.transform.GetComponentInParent<Wall>());
            }
        }

        Vector3 SouthEast = new Vector3(-1, 2, 2).normalized;
        ray = new Ray(transform.position, SouthEast);
        Hits = (Physics.RaycastAll(ray, 3, WallLayer));
        foreach (RaycastHit Hit in Hits)
        {
            if (Hit.transform.GetComponentInParent<Wall>())
            {
                wallsHiding.Add(Hit.transform.GetComponentInParent<Wall>());
            }
        }

        Vector3 SouthWest = new Vector3(-1, 2, -2).normalized;
        ray = new Ray(transform.position, SouthWest);
        Hits = (Physics.RaycastAll(ray, 3, WallLayer));
        foreach (RaycastHit Hit in Hits)
        {
            if (Hit.transform.GetComponentInParent<Wall>())
            {
                wallsHiding.Add(Hit.transform.GetComponentInParent<Wall>());
            }
        }

        return wallsHiding;
    }

	// Use this for initialization
	void Start () {
        

    }
	
	// Update is called once per frame
	void Update () {
        ////+x
        //Vector3 North = new Vector3(transform.position.x + 2f, transform.position.y + 2, transform.position.z);
        //Debug.DrawLine(transform.position, North);
        ////+x+z
        //Vector3 NorthEast = new Vector3(transform.position.x + 1f, transform.position.y + 2, transform.position.z + 2f);
        //Debug.DrawLine(transform.position, NorthEast);
        ////+x+z
        //Vector3 NorthWest = new Vector3(transform.position.x + 1f, transform.position.y + 2, transform.position.z - 2f);
        //Debug.DrawLine(transform.position, NorthWest);
        ////-x
        //Vector3 South = new Vector3(transform.position.x - 2f, transform.position.y + 2, transform.position.z);
        //Debug.DrawLine(transform.position, South);
        ////-x+z
        //Vector3 SouthEast = new Vector3(transform.position.x - 1f, transform.position.y + 2, transform.position.z + 2f);
        //Debug.DrawLine(transform.position, SouthEast);
        ////-x+z
        //Vector3 SouthhWest = new Vector3(transform.position.x - 1f, transform.position.y + 2, transform.position.z - 2f);
        //Debug.DrawLine(transform.position, SouthhWest);
    }
}

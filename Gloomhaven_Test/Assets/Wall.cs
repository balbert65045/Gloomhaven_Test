using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public Material NormalMaterial;
    public Material InvisibleMaterial;

    Transform myCamera;

    public bool invisible = false;
    MeshRenderer myWallMesh;
	// Use this for initialization
	void Start () {
        myCamera = FindObjectOfType<MyCameraController>().transform;
        myWallMesh = GetComponentInChildren<MeshRenderer>();
    }

    public void setInvisible()
    {
        myWallMesh = GetComponentInChildren<MeshRenderer>();
        invisible = true;
        myWallMesh.material = InvisibleMaterial;
    }

    public void ShowWall()
    {
        myWallMesh = GetComponentInChildren<MeshRenderer>();
        invisible = false;
        myWallMesh.material = NormalMaterial;
    }

    // Update is called once per frame
    void Update() {
        //float distance = (transform.position - myCamera.position).magnitude;

        //if (distance < InvisibleDistance)
        //{
        //    if (!invisible)
        //    {
        //        invisible = true;
        //        myWallMesh.material = InvisibleMaterial;
        //    }
        //}
        //else
        //{
        //    if (invisible)
        //    {
        //        invisible = false;
        //        myWallMesh.material = NormalMaterial;
        //    }
        //}
	}
}

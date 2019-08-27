using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public Material NormalMaterial;
    public Material InvisibleMaterial;

    Transform myCamera;

    public float InvisibleDistance = 5;

    float wallRotation;

    bool invisible = false;
    MeshRenderer myWallMesh;
	// Use this for initialization
	void Start () {
        myCamera = FindObjectOfType<MyCameraController>().transform;
        myWallMesh = GetComponentInChildren<MeshRenderer>();
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public Material NormalMaterial;
    public Material InvisibleMaterial;

    Transform myCameraPivot;

    public float CamRot;

    bool invisible = false;
    MeshRenderer myWallMesh;
	// Use this for initialization
	void Start () {
        myCameraPivot = FindObjectOfType<MyCameraController>().Pivot.transform;
        myWallMesh = GetComponentInChildren<MeshRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
         CamRot = myCameraPivot.rotation.eulerAngles.y;
        //f (CamRot > 360) { CamRot -= 360; }
        //float AngleDifference = Mathf.Abs(CamRot - transform.rotation.eulerAngles.y);


        if (CamRot < transform.rotation.eulerAngles.y + 100 && CamRot > transform.rotation.eulerAngles.y - 100)
        {
            if (!invisible)
            {
                invisible = true;
                myWallMesh.material = InvisibleMaterial;
            }
        }
        else
        {
            if (invisible)
            {
                invisible = false;
                myWallMesh.material = NormalMaterial;
            }
        }
	}
}

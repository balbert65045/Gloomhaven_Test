﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCameraController : MonoBehaviour {

    // Use this for initialization
    public GameObject Pivot;
    public GameObject Arm;
    public float zOffset;
    public float xOffset;

    public Transform target;

    public float moveDistance = 50f;

    private Quaternion RotationAngle;

	void Start () {
        target = null;
        RotationAngle = Pivot.transform.rotation;
    }
	
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void LookAt(Transform ObjectToLookAt)
    {
        Pivot.transform.position = ObjectToLookAt.transform.position;
    }

    public void UnLockCamera()
    {
        target = null;
    }

	// Update is called once per frame
	void Update () {
        if (target != null)
        {
            Pivot.transform.position = target.transform.position;
        }
        else
        {
            if (Input.GetKey(KeyCode.S))
            {
                Pivot.transform.localPosition = Vector3.Lerp(Pivot.transform.forward * moveDistance + Pivot.transform.localPosition, Pivot.transform.localPosition, .5f);
            } else if (Input.GetKey(KeyCode.A))
            {
                Pivot.transform.localPosition = Vector3.Lerp(Pivot.transform.right * moveDistance + Pivot.transform.localPosition, Pivot.transform.localPosition, .5f);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                Pivot.transform.localPosition = Vector3.Lerp(Pivot.transform.forward * -moveDistance + Pivot.transform.localPosition, Pivot.transform.localPosition, .5f);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Pivot.transform.localPosition = Vector3.Lerp(Pivot.transform.right * -moveDistance + Pivot.transform.localPosition, Pivot.transform.localPosition, .5f);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                RotationAngle *= Quaternion.AngleAxis(90, Vector3.up);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                RotationAngle *= Quaternion.AngleAxis(-90, Vector3.up);
            }

            Pivot.transform.rotation = Quaternion.Lerp(Pivot.transform.rotation, RotationAngle, 10 * Time.deltaTime);

        }
	}
}

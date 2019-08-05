using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCameraController : MonoBehaviour {

    // Use this for initialization
    public float zOffset;
    public float xOffset;

    public Transform target;

    public float moveDistance = 50f;

	void Start () {
        target = FindObjectOfType<PlayerController>().myCharacter.transform;
        Vector3 pos = target.position;
        zOffset = pos.z - transform.position.z;
        xOffset = pos.x - transform.position.x;
        target = null;
    }
	
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void LookAt(Transform ObjectToLookAt)
    {
        transform.position = new Vector3(ObjectToLookAt.position.x - xOffset, transform.position.y, ObjectToLookAt.position.z - zOffset);
    }

    public void UnLockCamera()
    {
        target = null;
    }

	// Update is called once per frame
	void Update () {
        if (target != null)
        {
            transform.position = new Vector3(target.position.x - xOffset, transform.position.y, target.position.z - zOffset);
        }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                transform.position = Vector3.Lerp(new Vector3(transform.position.x, transform.position.y, transform.position.z + moveDistance), transform.position, .5f);
            } else if (Input.GetKey(KeyCode.S))
            {
                transform.position = Vector3.Lerp(new Vector3(transform.position.x - moveDistance, transform.position.y, transform.position.z), transform.position, .5f);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.position = Vector3.Lerp(new Vector3(transform.position.x, transform.position.y, transform.position.z - moveDistance), transform.position, .5f);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                transform.position = Vector3.Lerp(new Vector3(transform.position.x + moveDistance, transform.position.y, transform.position.z), transform.position, .5f);
            }

        }
	}
}

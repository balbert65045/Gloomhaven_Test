using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexAdjuster : MonoBehaviour {

    public Mesh FullHex;
    public Mesh HalfHex;
    public Mesh FragmentHex;

    public void SetHexToHalf()
    {
        GetComponent<MeshFilter>().mesh = HalfHex;
        GetComponent<Node>().edge = true;
    }

    public void SetHexToFragment()
    {
        GetComponent<MeshFilter>().mesh = FragmentHex;
        GetComponent<Node>().edge = true;
    }

    public void SetHexToFull()
    {
        GetComponent<MeshFilter>().mesh = FullHex;
        GetComponent<Node>().edge = false;
    }

    public void RotateHexToTopRight()
    {
        transform.rotation = Quaternion.Euler(90, 0, 180);
    }

    public void RotateHexToTopMiddle()
    {
        transform.rotation = Quaternion.Euler(90, 0, 240);
    }

    public void RotateHexToTopLeft()
    {
        transform.rotation = Quaternion.Euler(90, 0, -60);
    }

    public void RotateHexToBottomLeft()
    {
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    public void RotateHexToBottomMiddle()
    {
        transform.rotation = Quaternion.Euler(90, 0, 60);
    }

    public void RotateHexToBottomRight()
    {
        transform.rotation = Quaternion.Euler(90, 0, 120);
    }


    public void Rotate60Forward()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + 60);
    }

    public void Rotate60Backward()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z - 60);
    }

    public void Rotate180()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + 180);
    }

}

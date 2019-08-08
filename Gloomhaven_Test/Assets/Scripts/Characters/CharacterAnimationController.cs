using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour {


    Rigidbody myRigidbody;
    Animator myAnimator;
    Vector3 movePosition;
    public bool Moving;
    bool Attacking = false;
    Hex hexMovingTo;
    List<Node> nodesMovingOn;

    Character myCharacter;
	// Use this for initialization
	void Start () {
        myAnimator = GetComponent<Animator>();
        myRigidbody = GetComponent<Rigidbody>();
        myRigidbody.constraints = RigidbodyConstraints.FreezePosition;
        myCharacter = GetComponent<Character>();
    }

    public void SetStealthState(bool stealthed)
    {
        myAnimator.SetBool("Stealthed", stealthed);
    }

    public void SwitchCombatState(bool inCombat)
    {
        myAnimator.SetBool("InCombat", inCombat);
    }

    public void FinishedDying()
    {
        GetComponent<Character>().Die();
    }

    public void Die()
    {
        myAnimator.SetTrigger("Die");
    }

    public void GetHit()
    {
        myAnimator.SetTrigger("Hit");
    }

    public void Hit()
    {
        Attacking = false;
        myCharacter.HitOpponent();
    }

    public void Attack()
    {
        if (!Attacking)
        {
            myAnimator.SetTrigger("Attack");
            Attacking = true;
        }
    }

    public void MoveTowards(Hex hex, List<Node> nextNodes)
    {
        transform.LookAt(new Vector3(hex.transform.position.x, transform.position.y, hex.transform.position.z));
        Moving = true;
        myAnimator.SetBool("moving", true);
        hexMovingTo = hex;
        nodesMovingOn = nextNodes;
        myRigidbody.constraints = RigidbodyConstraints.None;
    }
	
	// Update is called once per frame
	void Update () {
		if (Moving)
        {
            movePosition = new Vector3(hexMovingTo.transform.position.x, transform.position.y, hexMovingTo.transform.position.z);
            float difference = (transform.position - movePosition).magnitude;
            if (difference <= .2f)
            {
                MoveToNextPosition();
            }
        }
	}

    void MoveToNextPosition()
    {
        myCharacter.LinktoHex(hexMovingTo);
        if (myCharacter.Stealthed) { myCharacter.ReduceStealthDuration(); }
        myCharacter.ShowViewArea(hexMovingTo, GetComponent<Character>().ViewDistance);

        bool fight = false;
        if (!myCharacter.Stealthed)
        {
            fight = myCharacter.CheckToFight(); 
        }

        if (nodesMovingOn.Count == 0 || fight)
        {
            Moving = false;
            myRigidbody.constraints = RigidbodyConstraints.FreezePosition;
            myAnimator.SetBool("moving", false);
            myCharacter.FinishedMoving(hexMovingTo);
        }
        else
        {
            myCharacter.RemoveLinkFromHex();
            Node NextHex = nodesMovingOn[0];
            nodesMovingOn.Remove(NextHex);
            MoveTowards(NextHex.NodeHex, nodesMovingOn);
        }
    }
}

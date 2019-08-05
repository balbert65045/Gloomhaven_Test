using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActionCard : MonoBehaviour {

    public bool Shuffle = false;
    public int Initiative;
    public bool AttackAvailable = true;
    public int DamageIncrease = 0;
    public bool MovementAvailable = true;
    public int MovementIncrease = 0;

    public int RangeIncrease = 0;
    public int HealAmount = 0;

    public int ShieldAmount = 0;

    public string characterName;

    Vector3 initPosition;

    public void hideCard()
    {
        transform.position = initPosition;
    }

    void Start () {
        initPosition = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

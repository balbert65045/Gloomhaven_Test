using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingThrowFlip : MonoBehaviour {

    public void Saved()
    {
        GetComponentInParent<HealthBar>().SavingThrowSaved();
        this.gameObject.SetActive(false);
    }

    public void Dead()
    {
        GetComponentInParent<HealthBar>().SavingThrowDead();
        this.gameObject.SetActive(false);
    }
}

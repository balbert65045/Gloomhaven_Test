using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffsPanel : MonoBehaviour {

    public Sprite BuffType;
    public List<BuffArea> Buffs = new List<BuffArea>();


    public void SetUpBuffs(List<Buff> buffs)
    {
        for (int i = 0; i < buffs.Count; i++)
        {
            if (i == 4) { return; }
            BuffArea buffArea = Buffs[i];
            buffArea.gameObject.SetActive(true);
            buffArea.SetUpBuffArea(buffs[i].Amount, buffs[i].Duration, BuffType);
        }
    }

    public void HideBuffs()
    {
        foreach (BuffArea buffArea in Buffs)
        {
            buffArea.gameObject.SetActive(false);
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

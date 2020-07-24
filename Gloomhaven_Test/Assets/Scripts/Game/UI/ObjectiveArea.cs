using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveArea : MonoBehaviour {

    public Slider EnemySlider;
    public Text EnemyAmount;
    public int TotalEnemies;
    public int CurrentEnemies;

    public void SetTotalEnemies(int amount)
    {
        TotalEnemies = amount;
        CurrentEnemies = amount;
        EnemyAmount.text = amount.ToString();
    }

    public void EnemyDied()
    {
        CurrentEnemies--;
        EnemyAmount.text = CurrentEnemies.ToString();
        EnemySlider.value = (float)CurrentEnemies / (float)TotalEnemies;
        if (CurrentEnemies == 0)
        {
            FindObjectOfType<LevelClearedPanel>().TurnOnPanel();
        }
    }
}

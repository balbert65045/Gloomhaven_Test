using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CorruptionCounter : MonoBehaviour {

    public Text CounterText;

    public void StartCounterAt(int CounterStart)
    {
        CounterText.text = CounterStart.ToString();
    }

    public void CorruptionChanged(int amount)
    {
        CounterText.text = amount.ToString();
    }
}

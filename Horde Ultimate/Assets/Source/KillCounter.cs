using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillCounter : MonoBehaviour
{
    public Text[] counterText;

    public int killCount = 0;

    private void Start()
    {
        UpdateKillCounters();
    }

    public void UpdateKillCounters()
    {
        for (int i = 0; i < counterText.Length; i++)
        {
            counterText[i].text = "Defeated " + killCount;
        }
    }

    public void AddKill()
    {
        killCount++;
        UpdateKillCounters();
    }
}

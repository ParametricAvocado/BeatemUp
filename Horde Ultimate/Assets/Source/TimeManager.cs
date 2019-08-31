using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    float baseTimescale = 1;

    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void HitStop(float scale, float duration)
    {
        Time.timeScale = scale;
        StartCoroutine(HitStopRecovery(duration));
    }

    IEnumerator HitStopRecovery(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = baseTimescale;
    }
}

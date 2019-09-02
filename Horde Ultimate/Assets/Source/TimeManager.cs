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

    public void PlayTimescaleEvent(TimescaleEvent timescaleEvent)
    {
        if (timescaleEvent.duration > 0)
        {
            StopAllCoroutines();
            StartCoroutine(TimescaleEventCoroutine(timescaleEvent));
        }
    }

    IEnumerator TimescaleEventCoroutine(TimescaleEvent timescaleEvent)
    {
        Time.timeScale = timescaleEvent.timeScale;
        yield return new WaitForSecondsRealtime(timescaleEvent.duration);
        Time.timeScale = baseTimescale;
    }

    [System.Serializable]
    public struct TimescaleEvent
    {
        public float timeScale;
        public float duration;

        public TimescaleEvent(float timeScale, float duration)
        {
            this.timeScale = timeScale;
            this.duration = duration;
        }
    }
}

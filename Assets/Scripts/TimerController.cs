using System;
using System.Collections;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    public event Action OnIntervalElapsed;
    public event Action OnHighlightedExpired;

    private Coroutine intervalCoroutine;
    private Coroutine highlightCoroutine;

    public void StartInterval(float seconds)
    {
        StopInterval();
        intervalCoroutine = StartCoroutine(Call(seconds, OnIntervalElapsed));
    }

    public void StartHighlight(float seconds)
    {
        StopHighlight();
        highlightCoroutine = StartCoroutine(Call(seconds, OnHighlightedExpired));
    }

    public void StopInterval()
    {
        if (intervalCoroutine != null) StopCoroutine(intervalCoroutine);
        intervalCoroutine = null;
    }

    public void StopHighlight()
    {
        if (highlightCoroutine != null) StopCoroutine(highlightCoroutine);
        highlightCoroutine = null;
    }

    public void StopAllTimers()
    {
        StopInterval();
        StopHighlight();
    }

    private IEnumerator Call(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }
}
using System;
using System.Collections;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public event Action OnIntervalElapsed;
    public event Action OnHighlightedExpired;

    private Coroutine intervalCoroutine;
    private Coroutine highlightCoroutine;

    [Header("Timer Settings")]
    [SerializeField] private float intervalBetweenRounds;
    [SerializeField] private float highlightDuration;
    [SerializeField] private float minIntervalBetweenRounds;
    [SerializeField] private float minHighlightDuration;

    public void StartInterval()
    {
        StopInterval();
        intervalCoroutine = StartCoroutine(Call(intervalBetweenRounds, OnIntervalElapsed));
    }

    public void StartHighlight()
    {
        StopHighlight();
        highlightCoroutine = StartCoroutine(Call(highlightDuration, OnHighlightedExpired));
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
        intervalBetweenRounds -= 0.1f;
        highlightDuration -= 0.15f;

        if (intervalBetweenRounds <= minIntervalBetweenRounds)
            intervalBetweenRounds = minIntervalBetweenRounds;

        if (highlightDuration <= minHighlightDuration)
            highlightDuration = minHighlightDuration;
    }

    private IEnumerator Call(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }
}
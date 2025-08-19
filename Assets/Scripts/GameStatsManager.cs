using System;
using UnityEngine;

public class GameStatsManager : MonoBehaviour
{
    public event Action<bool> HandleUpdateCounter;

    public int SuccessfulClickCount { get; private set; }
    public int FailedClickCount { get; private set; }

    private void UpdateCount(bool IsSuccessfulClick)
    {
        if (IsSuccessfulClick)
        {
            SuccessfulClickCount++;
        }
        else
        {
            FailedClickCount++;
        }
    }

    public void TriggerUpdateCounterEvent(bool IsSuccessfulClick) => HandleUpdateCounter?.Invoke(IsSuccessfulClick);

    void OnEnable()
    {
        HandleUpdateCounter += UpdateCount;
    }

    void OnDisable()
    {
        HandleUpdateCounter -= UpdateCount;
    }
}
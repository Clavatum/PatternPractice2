using System;
using TMPro;
using UnityEngine;

public class InGameStatsUI : MonoBehaviour
{
    public event Action<int, bool> HandleUpdateCounterText;

    [SerializeField] private TextMeshProUGUI successfulClickCountText;
    [SerializeField] private TextMeshProUGUI failedClickCountText;

    private void UpdateCounterText(int count, bool IsSuccessfulClick)
    {
        if (IsSuccessfulClick)
        {
            successfulClickCountText.text = $"Successful Click(s): {count}";
        }
        else
        {
            failedClickCountText.text = $"Failed Click(s): {count}";
        }
    }

    public void TriggerUpdateCounterTextEvent(int count, bool IsSuccessfulClick) => HandleUpdateCounterText?.Invoke(count, IsSuccessfulClick);

    void OnEnable()
    {
        HandleUpdateCounterText += UpdateCounterText;
    }

    void OnDisable()
    {
        HandleUpdateCounterText -= UpdateCounterText;
    }
}
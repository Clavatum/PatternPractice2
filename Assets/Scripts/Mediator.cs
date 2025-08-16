using UnityEngine;
using UnityEngine.UI;

public class Mediator : MonoBehaviour
{
    private ButtonController buttonController;
    private TimerController timerController;

    [SerializeField] private float intervalBetweenRounds = 1.0f;
    [SerializeField] private float highlightDuration = 1.25f;
    private Button currentHighlightedButton;
    private bool isHighlightActive;

    void Awake()
    {
        buttonController = FindAnyObjectByType<ButtonController>();
        timerController = FindAnyObjectByType<TimerController>();
    }

    void Start()
    {
        timerController.StartInterval(intervalBetweenRounds);
    }

    private void HandleIntervalElapsed()
    {
        currentHighlightedButton = buttonController.GetRandomButton();
        isHighlightActive = true;
        buttonController.SetButtonHighlight(currentHighlightedButton, true);
        timerController.StartHighlight(highlightDuration);
    }

    private void HandleButtonClicked(Button clickedButton)
    {
        if (isHighlightActive)
        {
            if (clickedButton == currentHighlightedButton)
            {
                OnSuccessWhileHighlighted();
            }
            else
            {
                OnFailWrongButton();
            }
            EndRoundAndQueueNext();
            return;
        }
        OnFailTooLate();
        EndRoundAndQueueNext();
    }

    private void EndRoundAndQueueNext()
    {
        isHighlightActive = false;
        buttonController.ClearHighlight();
        currentHighlightedButton = null;

        timerController.StopHighlight();
        timerController.StartInterval(intervalBetweenRounds);
    }

    private void HandleHighlightExpired()
    {
        if (isHighlightActive)
        {
            OnFailTooLate();
        }
        EndRoundAndQueueNext();
    }

    private void OnFailTooLate()
    {
        Debug.Log("You are late to press highlighted button");
    }

    private void OnFailWrongButton()
    {
        Debug.Log("You pressed wrong button");
    }

    private void OnSuccessWhileHighlighted()
    {
        Debug.Log("You pressed right button");
    }

    void OnEnable()
    {
        buttonController.OnButtonClicked += HandleButtonClicked;
        timerController.OnIntervalElapsed += HandleIntervalElapsed;
        timerController.OnHighlightedExpired += HandleHighlightExpired;
    }

    void OnDisable()
    {
        buttonController.OnButtonClicked -= HandleButtonClicked;
        timerController.OnIntervalElapsed -= HandleIntervalElapsed;
        timerController.OnHighlightedExpired -= HandleHighlightExpired;
    }
}

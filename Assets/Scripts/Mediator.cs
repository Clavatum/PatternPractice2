using UnityEngine;
using UnityEngine.UI;

public class Mediator : MonoBehaviour
{
    [SerializeField] private ButtonController buttonController;
    [SerializeField] private TransformChanger transformChanger;
    [SerializeField] private Timer timer;

    private Button currentHighlightedButton;
    private bool isHighlightActive;

    void Start()
    {
        timer.StartInterval();
    }

    private void HandleIntervalElapsed()
    {
        currentHighlightedButton = buttonController.GetRandomButton();
        isHighlightActive = true;
        buttonController.SetButtonHighlight(currentHighlightedButton, true);
        timer.StartHighlight();
    }

    private void HandleButtonClicked(Button clickedButton)
    {
        if (isHighlightActive)
        {
            if (clickedButton == currentHighlightedButton)
            {
                OnSuccessWhileHighlighted();
                transformChanger.TriggerTransformChangedEvent(false);
            }
            else
            {
                OnFailWrongButton();
                transformChanger.TriggerTransformChangedEvent(true);
            }
            EndRoundAndQueueNext();
            return;
        }
        transformChanger.TriggerTransformChangedEvent(true);
        OnFailTooLate();
        EndRoundAndQueueNext();
    }

    private void EndRoundAndQueueNext()
    {
        isHighlightActive = false;
        buttonController.ClearHighlight();
        currentHighlightedButton = null;

        timer.StopAllTimers();
        timer.StartInterval();
    }

    private void HandleHighlightExpired()
    {
        if (isHighlightActive)
        {
            OnFailTooLate();
            transformChanger.TriggerTransformChangedEvent(true);
        }
        EndRoundAndQueueNext();
    }

    private void OnFailTooLate()
    {
        Debug.Log("Too late, getting bigger");
    }

    private void OnFailWrongButton()
    {
        Debug.Log("Wrong button, getting bigger");
    }

    private void OnSuccessWhileHighlighted()
    {
        Debug.Log("You pressed right button");
    }

    void OnEnable()
    {
        buttonController.OnButtonClicked += HandleButtonClicked;
        timer.OnIntervalElapsed += HandleIntervalElapsed;
        timer.OnHighlightedExpired += HandleHighlightExpired;
    }

    void OnDisable()
    {
        buttonController.OnButtonClicked -= HandleButtonClicked;
        timer.OnIntervalElapsed -= HandleIntervalElapsed;
        timer.OnHighlightedExpired -= HandleHighlightExpired;
    }
}

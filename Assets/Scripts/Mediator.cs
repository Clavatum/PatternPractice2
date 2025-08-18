using UnityEngine;
using UnityEngine.UI;

public class Mediator : MonoBehaviour
{
    [SerializeField] private ButtonController buttonController;
    [SerializeField] private TransformChanger transformChanger;
    [SerializeField] private Timer timer;
    [SerializeField] private Break_Ghost breakGhost;

    private Button currentHighlightedButton;
    private bool isHighlightActive;
    private bool isOperationsNeedsToBeStopped = false;

    void Start()
    {
        timer.StartInterval();
    }

    private void HandleIntervalElapsed()
    {
        if (isOperationsNeedsToBeStopped) return;
        currentHighlightedButton = buttonController.GetRandomButton();
        isHighlightActive = true;
        buttonController.SetButtonHighlight(currentHighlightedButton, true);
        timer.StartHighlight();
    }

    private void HandleButtonClicked(Button clickedButton)
    {
        if (isOperationsNeedsToBeStopped) return;
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
        if (isOperationsNeedsToBeStopped) return;
        if (isHighlightActive)
        {
            OnFailTooLate();
            transformChanger.TriggerTransformChangedEvent(true);
        }
        EndRoundAndQueueNext();
    }

    private void HandleBalloonPopped()
    {
        isOperationsNeedsToBeStopped = true;
        breakGhost.TriggerOnBalloonPoppedEvent();
        //trigger feedback and sound events and end game
    }

    private void HandleBallonReachedMinSize()
    {
        //trigger feedback and sound events
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
        transformChanger.OnBalloonReachedMaxSize += HandleBalloonPopped;
        transformChanger.OnBalloonReachedMinSize += HandleBallonReachedMinSize;
    }

    void OnDisable()
    {
        buttonController.OnButtonClicked -= HandleButtonClicked;
        timer.OnIntervalElapsed -= HandleIntervalElapsed;
        timer.OnHighlightedExpired -= HandleHighlightExpired;
        transformChanger.OnBalloonReachedMaxSize -= HandleBalloonPopped;
        transformChanger.OnBalloonReachedMinSize -= HandleBallonReachedMinSize;
    }
}

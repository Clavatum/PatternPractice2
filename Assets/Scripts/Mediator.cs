using UnityEngine;
using UnityEngine.UI;

public class Mediator : MonoBehaviour
{
    [SerializeField] private ButtonController buttonController;
    [SerializeField] private TransformChanger transformChanger;
    [SerializeField] private Timer timer;
    [SerializeField] private Break_Ghost breakGhost;
    [SerializeField] private InGameStatsUI inGameStatsUI;
    [SerializeField] private GameStatsManager gameStatsManager;
    [SerializeField] private AudioSystem audioSystem;
    [SerializeField] private GameManager gameManager;

    private Button currentHighlightedButton;
    private bool isHighlightActive;
    private bool isGameEnd = false;

    void Start()
    {
        timer.StartInterval();
    }

    private void HandleIntervalElapsed()
    {
        if (isGameEnd) return;
        currentHighlightedButton = buttonController.GetRandomButton();
        isHighlightActive = true;
        buttonController.SetButtonHighlight(currentHighlightedButton, true);
        timer.StartHighlight();
    }

    private void HandleButtonClicked(Button clickedButton)
    {
        if (isGameEnd) return;
        if (isHighlightActive)
        {
            if (clickedButton == currentHighlightedButton)
            {
                OnSuccessWhileHighlighted();
                audioSystem.TriggerBalloonInflatedEvent(false, false);
                transformChanger.TriggerTransformChangedEvent(false);
                gameStatsManager.TriggerUpdateCounterEvent(true);
                inGameStatsUI.TriggerUpdateCounterTextEvent(gameStatsManager.SuccessfulClickCount, true);
            }
            else
            {
                OnFailWrongButton();
                audioSystem.TriggerBalloonInflatedEvent(false, true);
                transformChanger.TriggerTransformChangedEvent(true);
                gameStatsManager.TriggerUpdateCounterEvent(false);
                inGameStatsUI.TriggerUpdateCounterTextEvent(gameStatsManager.FailedClickCount, false);
            }
            EndRoundAndQueueNext();
            return;
        }
        transformChanger.TriggerTransformChangedEvent(true);
        OnFailTooLate();
        EndRoundAndQueueNext();
    }

    private void HandleHighlightExpired()
    {
        if (isGameEnd) return;
        if (isHighlightActive)
        {
            OnFailTooLate();
            audioSystem.TriggerBalloonInflatedEvent(false, true);
            transformChanger.TriggerTransformChangedEvent(true);
            gameStatsManager.TriggerUpdateCounterEvent(false);
            inGameStatsUI.TriggerUpdateCounterTextEvent(gameStatsManager.FailedClickCount, false);
        }
        EndRoundAndQueueNext();
    }

    private void HandleBalloonPopped()
    {
        isGameEnd = true;
        audioSystem.TriggerBalloonInflatedEvent(true, false);
        breakGhost.TriggerOnBalloonPoppedEvent();
        gameManager.TriggerGameEnd();
    }

    private void HandleBallonReachedMinSize()
    {
        Debug.Log("Balloon reached minimum size");
    }

    private void EndRoundAndQueueNext()
    {
        isHighlightActive = false;
        buttonController.ClearHighlight();
        currentHighlightedButton = null;

        timer.StopAllTimers();
        timer.StartInterval();
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

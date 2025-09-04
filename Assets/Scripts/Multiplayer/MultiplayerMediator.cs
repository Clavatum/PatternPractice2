using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMediator : NetworkBehaviour
{
    [SerializeField] private MultiplayerButtonController MultiplayerButtonController;
    [SerializeField] private MultiplayerTransformChanger MultiplayerTransformChanger;
    [SerializeField] private Timer timer;
    [SerializeField] private Break_Ghost breakGhost;
    [SerializeField] private InGameStatsUI inGameStatsUI;
    [SerializeField] private GameStatsManager gameStatsManager;
    [SerializeField] private AudioSystem audioSystem;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SpawnManager spawnManager;

    private Button currentHighlightedButton;
    private int indexOfCurrentHighlightedButton;
    private bool isHighlightActive;
    private bool isGameEnd = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        breakGhost = spawnManager.MyBalloon.GetComponent<Break_Ghost>();
        MultiplayerTransformChanger = spawnManager.MyBalloon.GetComponentInChildren<MultiplayerTransformChanger>();
        audioSystem.AudioSource = spawnManager.MyBalloon.GetComponentInChildren<AudioSource>();
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(5f);
        if (!IsOwner) yield return null;
        MultiplayerTransformChanger.OnBalloonReachedMaxSize += HandleBalloonPopped;
        MultiplayerTransformChanger.OnBalloonReachedMinSize += HandleBallonReachedMinSize;
        MultiplayerButtonController.OnButtonClicked += HandleButtonClicked;
        timer.OnIntervalElapsed += HandleIntervalElapsed;
        timer.OnHighlightedExpired += HandleHighlightExpired;
        timer.StartInterval();
    }

    private void HandleIntervalElapsed()
    {
        if (!IsServer) return;
        if (isGameEnd) return;
        indexOfCurrentHighlightedButton = MultiplayerButtonController.GetRandomButtonIndex();
        isHighlightActive = true;
        HandleIntervalElapsedClientRpc(indexOfCurrentHighlightedButton);
    }

    [ClientRpc]
    private void HandleIntervalElapsedClientRpc(int indexOfButtonToHighlight)
    {
        List<Button> buttonList = MultiplayerButtonController.GetButtonList();
        Button buttonToHighlight = buttonList[indexOfButtonToHighlight];
        MultiplayerButtonController.SetButtonHighlight(buttonToHighlight, true);
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
                MultiplayerTransformChanger.TriggerTransformChangedEvent(false);
                gameStatsManager.TriggerUpdateCounterEvent(true);
                inGameStatsUI.TriggerUpdateCounterTextEvent(gameStatsManager.SuccessfulClickCount, true);
            }
            else
            {
                OnFailWrongButton();
                audioSystem.TriggerBalloonInflatedEvent(false, true);
                MultiplayerTransformChanger.TriggerTransformChangedEvent(true);
                gameStatsManager.TriggerUpdateCounterEvent(false);
                inGameStatsUI.TriggerUpdateCounterTextEvent(gameStatsManager.FailedClickCount, false);
            }
            EndRoundAndQueueNext();
            return;
        }
        MultiplayerTransformChanger.TriggerTransformChangedEvent(true);
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
            MultiplayerTransformChanger.TriggerTransformChangedEvent(true);
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
        MultiplayerButtonController.ClearHighlight();
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

    void OnDisable()
    {
        MultiplayerButtonController.OnButtonClicked -= HandleButtonClicked;
        timer.OnIntervalElapsed -= HandleIntervalElapsed;
        timer.OnHighlightedExpired -= HandleHighlightExpired;
        MultiplayerTransformChanger.OnBalloonReachedMaxSize -= HandleBalloonPopped;
        MultiplayerTransformChanger.OnBalloonReachedMinSize -= HandleBallonReachedMinSize;
    }
}
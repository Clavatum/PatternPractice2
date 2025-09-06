using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
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
    [SerializeField] private MultiplayerAudioSystem multiplayerAudioSystem;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SpawnManager spawnManager;

    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    private List<Button> buttonList;
    private Button currentHighlightedButton;
    private int indexOfCurrentHighlightedButton;
    private bool isHighlightActive;
    private bool isGameEnd = false;

    private NetworkVariable<bool> isGameStarted = new(
        false,
        writePerm: NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        buttonList = MultiplayerButtonController.GetButtonList();
        if (IsHost)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.onClick.AddListener(StartGame);
        }
        StartCoroutine(AssignReferences());
    }

    private void StartGame()
    {
        isGameStarted.Value = true;
        StartCoroutine(StartGameCoroutine());
        startGameButton.gameObject.SetActive(false);
    }

    private IEnumerator AssignReferences()
    {
        while (spawnManager == null)
            yield return null;

        while (spawnManager.MyBalloon == null)
            yield return null;

        if (!IsOwner) yield break;

        breakGhost = spawnManager.MyBalloon.GetComponent<Break_Ghost>();
        MultiplayerTransformChanger = spawnManager.MyBalloon.GetComponentInChildren<MultiplayerTransformChanger>();
        multiplayerAudioSystem.AudioSource = spawnManager.MyBalloon.GetComponentInChildren<AudioSource>();
    }

    private IEnumerator StartGameCoroutine()
    {
        if (!IsOwner) yield return null;
        MultiplayerTransformChanger.OnBalloonReachedMaxSize += HandleBalloonPopped;
        MultiplayerTransformChanger.OnBalloonReachedMinSize += HandleBallonReachedMinSize;
        MultiplayerButtonController.OnButtonClicked += HandleButtonClicked;
        timer.OnIntervalElapsed += HandleIntervalElapsed;
        timer.OnHighlightedExpired += HandleHighlightExpired;
        feedbackText.text = "Game is starts in 3 seconds";
        yield return new WaitForSeconds(3f);
        timer.StartInterval();
        feedbackText.text = "";
    }

    private void HandleIntervalElapsed()
    {
        if (isGameEnd || !IsServer || !isGameStarted.Value) return;
        indexOfCurrentHighlightedButton = MultiplayerButtonController.GetRandomButtonIndex();
        isHighlightActive = true;
        HandleIntervalElapsedClientRpc(indexOfCurrentHighlightedButton);
    }

    [ClientRpc]
    private void HandleIntervalElapsedClientRpc(int indexOfButtonToHighlight)
    {
        if (!isGameStarted.Value) return;
        currentHighlightedButton = buttonList[indexOfButtonToHighlight];
        MultiplayerButtonController.ClearHighlight();
        MultiplayerButtonController.SetButtonHighlight(currentHighlightedButton, true);
        timer.StartHighlight();
    }

    private void HandleButtonClicked(Button clickedButton)
    {
        if (!IsOwner || isGameEnd || !isGameStarted.Value) return;
        HandleButtonClickedClientRpc(buttonList.IndexOf(clickedButton));
    }

    [ClientRpc]
    private void HandleButtonClickedClientRpc(int indexOfClickedButton)
    {
        if (isHighlightActive)
        {
            if (indexOfClickedButton == buttonList.IndexOf(currentHighlightedButton))
            {
                OnSuccessWhileHighlighted();
                multiplayerAudioSystem.TriggerSound(false, false);
                MultiplayerTransformChanger.TriggerTransformChangedEvent(false);
                gameStatsManager.TriggerUpdateCounterEvent(true);
                inGameStatsUI.TriggerUpdateCounterTextEvent(gameStatsManager.SuccessfulClickCount, true);
            }
            else
            {
                OnFailWrongButton();
                multiplayerAudioSystem.TriggerSound(false, true);
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
            multiplayerAudioSystem.TriggerSound(false, true);
            MultiplayerTransformChanger.TriggerTransformChangedEvent(true);
            gameStatsManager.TriggerUpdateCounterEvent(false);
            inGameStatsUI.TriggerUpdateCounterTextEvent(gameStatsManager.FailedClickCount, false);
        }
        EndRoundAndQueueNext();
    }

    private void HandleBalloonPopped()
    {
        isGameEnd = true;
        multiplayerAudioSystem.TriggerSound(true, false);
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
        ClearHighlightClientRpc();
        currentHighlightedButton = null;

        timer.StopAllTimers();
        timer.StartInterval();
    }

    [ClientRpc]
    private void ClearHighlightClientRpc()
    {
        MultiplayerButtonController.ClearHighlight();
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
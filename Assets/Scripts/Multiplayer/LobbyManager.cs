using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private TMP_Dropdown maxPlayersDropdown;
    [SerializeField] private Toggle isLobbyPrivate;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button browseLobbiesButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button closeLobbyButton;
    [SerializeField] private Button openCreateLobbyPanelButton;
    [SerializeField] private Button closeCreateLobbyPanelButton;
    [SerializeField] private Button openLobbyBrowserPanelButton;
    [SerializeField] private Button closeLobbyBrowserPanelButton;
    [SerializeField] private GameObject createLobbyPanel;
    [SerializeField] private GameObject lobbyBrowserPanel;

    private Lobby currentLobby;
    private Coroutine heartbeatCoroutine;

    void Awake()
    {
        createLobbyButton.onClick.AddListener(CreateLobby);
        joinLobbyButton.onClick.AddListener(JoinLobbyWithCode);
        closeLobbyButton.onClick.AddListener(CloseLobby);
        browseLobbiesButton.onClick.AddListener(BrowseLobbies);
        openCreateLobbyPanelButton.onClick.AddListener(OpenCreateLobbyPanel);
        openLobbyBrowserPanelButton.onClick.AddListener(OpenLobbyBrowserPanel);
        closeCreateLobbyPanelButton.onClick.AddListener(CloseCreateLobbyPanel);
        closeLobbyBrowserPanelButton.onClick.AddListener(CloseLobbyBrowserPanel);
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void CreateLobby()
    {
        string lobbyName = lobbyNameInputField.text;
        int maxPlayers = Convert.ToInt32(maxPlayersDropdown.options[maxPlayersDropdown.value].text);

        CreateLobbyOptions createLobbyOptions = new()
        {
            IsPrivate = isLobbyPrivate.isOn,
            Player = new Player(AuthenticationService.Instance.PlayerId)
        };

        createLobbyOptions.Player.Data = new Dictionary<string, PlayerDataObject>()
        {
            {"Nickname", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, nicknameInputField.text) }
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
        LogPlayersInLobby(currentLobby);
        DontDestroyOnLoad(this);

        Debug.Log("Lobby crated");
        CloseCreateLobbyPanel();
        lobbyCodeText.text = $"Lobby Code: {currentLobby.LobbyCode}";

        nicknameInputField.gameObject.SetActive(false);
        openCreateLobbyPanelButton.gameObject.SetActive(false);
        closeLobbyButton.gameObject.SetActive(true);
        browseLobbiesButton.gameObject.SetActive(false);
        joinLobbyButton.gameObject.SetActive(false);
        lobbyCodeInputField.gameObject.SetActive(false);

        StartHeartbeat(currentLobby.Id, 15f);
    }

    private async void JoinLobbyWithCode()
    {
        var code = lobbyCodeInputField.text;
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
            {
                Player = new Player(AuthenticationService.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        {"Nickname", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, nicknameInputField.text) }
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
            LogPlayersInLobby(lobby);

            nicknameInputField.gameObject.SetActive(false);
            browseLobbiesButton.gameObject.SetActive(false);
            openCreateLobbyPanelButton.gameObject.SetActive(false);
            lobbyCodeInputField.gameObject.SetActive(false);
            joinLobbyButton.gameObject.SetActive(false);

            Debug.Log("joined a lobby with code");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private async void CloseLobby()
    {
        if (currentLobby == null) return;

        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);

            Debug.Log("Lobby deleted by host");

            StopHeartbeat();

            lobbyCodeText.text = "";
            playerListText.text = "";
            nicknameInputField.gameObject.SetActive(true);
            openCreateLobbyPanelButton.gameObject.SetActive(true);
            closeLobbyButton.gameObject.SetActive(false);
            browseLobbiesButton.gameObject.SetActive(true);
            joinLobbyButton.gameObject.SetActive(true);
            lobbyCodeInputField.gameObject.SetActive(true);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"CloseLobby error: {ex.Reason} - {ex.Message}");
        }

        currentLobby = null;
    }

    private async void BrowseLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new()
            {
                Count = 4,

                Filters = new List<QueryFilter>()
                {
                    new(field: QueryFilter.FieldOptions.AvailableSlots,
                    op:QueryFilter.OpOptions.GT,value:"0")
                },

                Order = new List<QueryOrder>()
                {
                    new(asc: true,
                    field: (QueryOrder.FieldOptions)QueryFilter.FieldOptions.AvailableSlots)
                }
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            if (lobbies.Results.Count == 0)
                feedbackText.text = "Couldn't find any lobby";
            else
                feedbackText.text = $"{lobbies.Results.Count} lobby/lobbies found";

            foreach (Lobby foundLobby in lobbies.Results)
            {
                Debug.Log($"Lobby Name: {foundLobby.Name}\n");
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private void LogPlayersInLobby(Lobby lobby)
    {
        playerListText.text = "\tPlayers\n";
        foreach (Player player in lobby.Players)
        {
            playerListText.text += $"{player.Data["Nickname"].Value}\n";
        }
    }

    private IEnumerator HeartbeatLobby(string lobbyID, float interval)
    {
        var delay = new WaitForSeconds(interval);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyID);
            yield return delay;
        }
    }

    private void StartHeartbeat(string lobbyID, float interval = 15f)
    {
        StopHeartbeat();
        heartbeatCoroutine = StartCoroutine(HeartbeatLobby(lobbyID, interval));
    }

    private void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
    }

    private void OpenCreateLobbyPanel()
    {
        createLobbyPanel.SetActive(true);
    }

    private void CloseCreateLobbyPanel()
    {
        createLobbyPanel.SetActive(false);
    }

    private void OpenLobbyBrowserPanel()
    {
        lobbyBrowserPanel.SetActive(true);
        closeLobbyBrowserPanelButton.gameObject.SetActive(true);
    }

    private void CloseLobbyBrowserPanel()
    {
        lobbyBrowserPanel.SetActive(false);
        closeLobbyBrowserPanelButton.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        createLobbyButton.onClick.RemoveAllListeners();
        joinLobbyButton.onClick.RemoveAllListeners();
        closeLobbyButton.onClick.RemoveAllListeners();
        browseLobbiesButton.onClick.RemoveAllListeners();
        openCreateLobbyPanelButton.onClick.RemoveAllListeners();
        openLobbyBrowserPanelButton.onClick.RemoveAllListeners();
        closeCreateLobbyPanelButton.onClick.RemoveAllListeners();
        closeLobbyBrowserPanelButton.onClick.RemoveAllListeners();
    }
}
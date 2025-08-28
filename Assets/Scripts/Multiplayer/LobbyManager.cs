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
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_Dropdown maxPlayersDropdown;
    [SerializeField] private Toggle isLobbyPrivate;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button browseLobbiesButton;
    [SerializeField] private Button openCreateLobbyPanelButton;
    [SerializeField] private Button closeCreateLobbyPanelButton;
    [SerializeField] private Button openLobbyBrowserPanelButton;
    [SerializeField] private Button closeLobbyBrowserPanelButton;
    [SerializeField] private GameObject createLobbyPanel;
    [SerializeField] private GameObject lobbyBrowserPanel;

    void Awake()
    {
        createLobbyButton.onClick.AddListener(CreateLobby);
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
            IsPrivate = isLobbyPrivate.isOn
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
        DontDestroyOnLoad(this);
        Debug.Log("Lobby crated");

        StartCoroutine(HeartbeatLobby(lobby.Id, 15f));
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
            Debug.Log(ex);
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
        browseLobbiesButton.onClick.RemoveAllListeners();
        openCreateLobbyPanelButton.onClick.RemoveAllListeners();
        openLobbyBrowserPanelButton.onClick.RemoveAllListeners();
        closeCreateLobbyPanelButton.onClick.RemoveAllListeners();
        closeLobbyBrowserPanelButton.onClick.RemoveAllListeners();
    }
}

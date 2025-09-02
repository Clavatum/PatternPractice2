using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private TextMeshProUGUI playerCountText;
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
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject playerCardPrefab;
    [SerializeField] private GameObject createLobbyPanel;
    [SerializeField] private GameObject lobbyBrowserPanel;
    [SerializeField] private GameObject playerCardContainer;
    [SerializeField] private GameObject lobbyButtonContainer;
    [SerializeField] private GameObject lobbyThumbnailButton;

    private Lobby currentLobby;
    private Allocation allocation;
    private Coroutine heartbeatCoroutine;
    private string playerId;

    private RelayHostData hostData;
    private RelayJoinData joinData;

    void Awake()
    {
        createLobbyButton.onClick.AddListener(CreateLobby);
        joinLobbyButton.onClick.AddListener(JoinLobbyWithCode);
        leaveRoomButton.onClick.AddListener(LeaveRoom);
        closeLobbyButton.onClick.AddListener(CloseLobby);
        startGameButton.onClick.AddListener(StartGame);
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
        playerId = AuthenticationService.Instance.PlayerId;
    }

    private async void CreateLobby()
    {
        string lobbyName = lobbyNameInputField.text;
        int maxPlayers = Convert.ToInt32(maxPlayersDropdown.options[maxPlayersDropdown.value].text);

        CreateLobbyOptions createLobbyOptions = new()
        {
            IsPrivate = isLobbyPrivate.isOn,
            Player = new Player(AuthenticationService.Instance.PlayerId),
            Data = new Dictionary<string, DataObject>
            {
                { "IsGameStarted", new DataObject(DataObject.VisibilityOptions.Member, "false") },{"RoomCode", new DataObject(DataObject.VisibilityOptions.Member, null)}
            }
        };

        createLobbyOptions.Player.Data = new Dictionary<string, PlayerDataObject>()
        {
            {"Nickname", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, nicknameInputField.text)
            }
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
        LogPlayersInLobby(currentLobby);
        DontDestroyOnLoad(this);

        InvokeRepeating(nameof(UpdateLobby), 0f, 1.5f);
        CloseCreateLobbyPanel();
        lobbyCodeText.text = $"Lobby Code: {currentLobby.LobbyCode}";

        closeLobbyButton.gameObject.SetActive(true);
        SetInLobbyUIActive();

        StartHeartbeat(currentLobby.Id, 15f);
    }

    private async void JoinLobbyWithID(Lobby lobby)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new()
            {
                Player = new Player(AuthenticationService.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        {"Nickname", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, nicknameInputField.text) }
                    }
                }
            };

            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);
            InvokeRepeating(nameof(UpdateLobby), 0f, 1.5f);
            LogPlayersInLobby(lobby);
            lobbyBrowserPanel.SetActive(false);
            closeLobbyBrowserPanelButton.gameObject.SetActive(false);
            SetInLobbyUIActive();
        }
        catch (LobbyServiceException ex)
        {
            feedbackText.text = ex.Message;
        }
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

            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
            InvokeRepeating(nameof(UpdateLobby), 0f, 1.5f);

            LogPlayersInLobby(currentLobby);
            SetInLobbyUIActive();
        }
        catch (LobbyServiceException ex)
        {
            feedbackText.text = ex.Message;
        }
    }

    private async void CloseLobby()
    {
        if (currentLobby == null) return;

        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
            currentLobby = null;
            allocation = null;
            CancelInvoke(nameof(UpdateLobby));
            StopHeartbeat();
            SetLobbyMenuUIActive();
        }
        catch (LobbyServiceException ex)
        {
            feedbackText.text = ex.Message;
        }
    }

    private async void UpdateLobby()
    {
        if (currentLobby == null || !IsInLobby())
        {
            CancelInvoke(nameof(UpdateLobby));
            SetLobbyMenuUIActive();
            return;
        }

        try
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            LogPlayersInLobby(currentLobby);

            if (!IsHost() && IsGameStarted())
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(currentLobby.Data["RoomCode"].Value);

                joinData = new RelayJoinData()
                {
                    IPv4Address = joinAllocation.RelayServer.IpV4,
                    Port = (ushort)joinAllocation.RelayServer.Port,

                    AllocationID = joinAllocation.AllocationId,
                    AllocationIDBytes = joinAllocation.AllocationIdBytes,
                    ConnectionData = joinAllocation.ConnectionData,
                    HostConnectionData = joinAllocation.HostConnectionData,
                    Key = joinAllocation.Key,
                };

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(
                    joinData.IPv4Address,
                    joinData.Port,
                    joinData.AllocationIDBytes,
                    joinData.Key,
                    joinData.ConnectionData,
                    joinData.HostConnectionData
                );

                NetworkManager.Singleton.StartClient();
            }
        }
        catch (LobbyServiceException ex)
        {
            feedbackText.text = ex.Message;
            currentLobby = null;
            allocation = null;
            CancelInvoke(nameof(UpdateLobby));
            SetLobbyMenuUIActive();
        }
    }

    private bool IsInLobby()
    {
        if (currentLobby == null) return false;
        foreach (Player player in currentLobby.Players)
        {
            if (player.Id == playerId)
            {
                return true;
            }
        }
        currentLobby = null;
        allocation = null;
        return false;
    }

    private async void LeaveRoom()
    {
        if (currentLobby == null) return;
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
            CancelInvoke(nameof(UpdateLobby));
            SetLobbyMenuUIActive();
            currentLobby = null;
            allocation = null;
        }
        catch (LobbyServiceException ex)
        {
            feedbackText.text = ex.Message;
        }
    }

    private async void KickPlayer(string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    private async void BrowseLobbies()
    {
        if (lobbyButtonContainer != null && lobbyButtonContainer.transform.childCount > 0)
        {
            foreach (Transform transform in lobbyButtonContainer.transform)
            {
                Destroy(transform.gameObject);
            }
        }

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
                feedbackText.text = "Couldn't find any available lobby";
            else
                feedbackText.text = $"{lobbies.Results.Count} lobby/lobbies found";

            foreach (Lobby foundLobby in lobbies.Results)
            {
                CreateLobbyButtonContainer(foundLobby);
            }
        }
        catch (LobbyServiceException ex)
        {
            feedbackText.text = ex.Message;
        }
    }

    private void CreateLobbyButtonContainer(Lobby lobby)
    {
        var button = Instantiate(lobbyThumbnailButton, Vector3.zero, Quaternion.identity);
        button.GetComponentInChildren<TextMeshProUGUI>().text = lobby.Name;
        var recTransform = button.GetComponent<RectTransform>();
        recTransform.SetParent(lobbyButtonContainer.transform);
        button.GetComponent<Button>().onClick.RemoveAllListeners();
        button.GetComponent<Button>().onClick.AddListener(delegate () { JoinLobbyWithID(lobby); });
    }

    private void LogPlayersInLobby(Lobby lobby)
    {
        if (IsGameStarted()) return;
        if (playerCardContainer != null && playerCardContainer.transform.childCount > 0)
        {
            foreach (Transform transform in playerCardContainer.transform)
            {
                Destroy(transform.gameObject);
            }
        }
        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

        if (lobby.Players.Count == 0) { return; }
        foreach (Player player in lobby.Players)
        {
            GameObject card = Instantiate(playerCardPrefab, Vector3.zero, Quaternion.identity);
            var recTransform = card.GetComponent<RectTransform>();
            recTransform.SetParent(playerCardContainer.transform);
            Button kickButton = card.GetComponentInChildren<Button>();
            card.GetComponentInChildren<TextMeshProUGUI>().text = player.Data["Nickname"].Value;
            if ((player.Id == currentLobby.HostId) || player.Id == playerId)
            {
                kickButton.gameObject.SetActive(false);
            }
            kickButton.onClick.AddListener(delegate { KickPlayer(player.Id); });
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

    private bool IsHost()
    {
        if (currentLobby != null && currentLobby.HostId == playerId)
        {
            return true;
        }
        return false;
    }

    private async void StartGame()
    {
        if (currentLobby == null || !IsHost()) return;
        try
        {
            int maxPlayers = Convert.ToInt32(maxPlayersDropdown.options[maxPlayersDropdown.value].text);
            allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string RoomCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            hostData = new RelayHostData()
            {
                IPv4Address = allocation.RelayServer.IpV4,
                Port = (ushort)allocation.RelayServer.Port,

                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                Key = allocation.Key,
            };

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                hostData.IPv4Address,
                hostData.Port,
                hostData.AllocationIDBytes,
                hostData.Key,
                hostData.ConnectionData
            );
            NetworkManager.Singleton.StartHost();

            UpdateLobbyOptions updateLobbyOptions = new()
            {
                Data = new Dictionary<string, DataObject>
            {
                {"IsGameStarted", new DataObject(DataObject.VisibilityOptions.Member, "true")},
                { "RoomCode", new DataObject(DataObject.VisibilityOptions.Member, RoomCode)}
            }
            };
            currentLobby = await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateLobbyOptions);

            NetworkManager.Singleton.SceneManager.LoadScene("MultiplayerGameScene", LoadSceneMode.Single);
        }
        catch (LobbyServiceException ex)
        {
            feedbackText.text = ex.Message;
        }
    }

    private bool IsGameStarted()
    {
        if (currentLobby == null) return false;
        if (currentLobby.Data["IsGameStarted"].Value == "true") return true;
        return false;
    }

    private void SetLobbyMenuUIActive()
    {
        lobbyCodeText.text = "";
        feedbackText.text = "";
        playerCountText.text = "";
        playerListText.gameObject.SetActive(false);
        leaveRoomButton.gameObject.SetActive(false);
        playerCardContainer.SetActive(false);
        nicknameInputField.gameObject.SetActive(true);
        openCreateLobbyPanelButton.gameObject.SetActive(true);
        closeLobbyButton.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);
        browseLobbiesButton.gameObject.SetActive(true);
        joinLobbyButton.gameObject.SetActive(true);
        lobbyCodeInputField.gameObject.SetActive(true);
    }

    private void SetInLobbyUIActive()
    {
        if (!IsHost())
            leaveRoomButton.gameObject.SetActive(true);
        if (IsHost())
            startGameButton.gameObject.SetActive(true);

        playerListText.gameObject.SetActive(true);
        playerCardContainer.SetActive(true);
        nicknameInputField.gameObject.SetActive(false);
        browseLobbiesButton.gameObject.SetActive(false);
        openCreateLobbyPanelButton.gameObject.SetActive(false);
        lobbyCodeInputField.gameObject.SetActive(false);
        joinLobbyButton.gameObject.SetActive(false);
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
        feedbackText.text = "";
        closeLobbyBrowserPanelButton.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        createLobbyButton.onClick.RemoveAllListeners();
        joinLobbyButton.onClick.RemoveAllListeners();
        closeLobbyButton.onClick.RemoveAllListeners();
        startGameButton.onClick.RemoveAllListeners();
        leaveRoomButton.onClick.RemoveAllListeners();
        browseLobbiesButton.onClick.RemoveAllListeners();
        openCreateLobbyPanelButton.onClick.RemoveAllListeners();
        openLobbyBrowserPanelButton.onClick.RemoveAllListeners();
        closeCreateLobbyPanelButton.onClick.RemoveAllListeners();
        closeLobbyBrowserPanelButton.onClick.RemoveAllListeners();
    }

    public struct RelayHostData
    {
        public string joinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }

    public struct RelayJoinData
    {
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }
}
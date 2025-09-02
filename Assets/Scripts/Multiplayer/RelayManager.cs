using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nickNameInputField;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TMP_InputField maxPlayersInputField;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    private RelayHostData hostData;
    private RelayJoinData joinData;

    public static string RoomCode { get; private set; }
    public static string PlayerNickname { get; private set; }
    public static int MaxPlayers { get; private set; }
    public static bool GameStarted { get; set; } = false;

    void Awake()
    {
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void CreateRoom()
    {
        PlayerNickname = string.IsNullOrWhiteSpace(nickNameInputField.text)
        ? "Host"
        : nickNameInputField.text;

        int maxPlayers = int.Parse(maxPlayersInputField.text);
        if (maxPlayers > 4) { maxPlayers = 4; }
        if (maxPlayers < 1) { maxPlayers = 1; }

        MaxPlayers = maxPlayers;
        GameStarted = false;

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        RoomCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

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

        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Room", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Failed to start host!");
        }
        Debug.Log(NetworkManager.Singleton.ConnectedClients.Count);
        Debug.Log(maxPlayers);
        Debug.Log(MaxPlayers);
    }

    private async void JoinRoom()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count == MaxPlayers)
        {
            feedbackText.text = "The room is full";
            return;
        }
        if (GameStarted)
        {
            feedbackText.text = "The game is started, you can't join";
            return;
        }

        PlayerNickname = string.IsNullOrWhiteSpace(nickNameInputField.text)
        ? "Client"
        : nickNameInputField.text;

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeInputField.text);

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

            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("Failed to start client!");
            }
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError("Join failed: " + ex.Message);
        }
    }

    void OnDisable()
    {
        createRoomButton.onClick.RemoveAllListeners();
        joinRoomButton.onClick.RemoveAllListeners();
    }
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
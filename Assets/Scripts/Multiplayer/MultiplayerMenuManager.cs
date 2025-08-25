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

public class MultiplayerMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nickNameInputField;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TMP_InputField maxPlayersInputField;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    private RelayHostData hostData;
    private RelayJoinData joinData;

    public static string RoomCode { get; private set; }

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
        int maxPlayers = 4;
        if (int.TryParse(maxPlayersInputField.text, out int parsed))
            maxPlayers = Mathf.Clamp(parsed, 2, 8);

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
    }

    private async void JoinRoom()
    {
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
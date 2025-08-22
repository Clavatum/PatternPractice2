using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
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

        var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        RoomCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Room", LoadSceneMode.Single);
    }

    private async void JoinRoom()
    {
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeInputField.text);

            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            unityTransport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
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
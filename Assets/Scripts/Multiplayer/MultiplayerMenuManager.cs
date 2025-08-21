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
    [SerializeField] private TMP_InputField JoinCodeInputField;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    public static string RoomCode { get; private set; }

    void Awake()
    {
        createRoomButton.onClick.AddListener(CrateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        OnSignIn();
    }

    private async void OnSignIn()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void CrateRoom()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(2);

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

        SceneManager.LoadScene("Room");
    }

    private async void JoinRoom()
    {
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(JoinCodeInputField.text);

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

            SceneManager.LoadScene("Room");
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
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Linq;

public class RoomManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button startGameButton;

    private void Awake()
    {
        startGameButton.onClick.AddListener(StartGame);
    }

    void Start()
    {
        if (!IsHost)
            startGameButton.gameObject.SetActive(false);

        if (IsHost)
            roomCodeText.text = $"Room Code: {RelayManager.RoomCode}";
        else
            roomCodeText.gameObject.SetActive(false);

        connectionStatusText.text = IsHost ? "Host" : "Client";

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        UpdatePlayerCount();

        if (IsHost)
        {
            AppendFeedback($"{RelayManager.PlayerNickname} created the room. Waiting for players...\n");
        }
    }

    private void StartGame()
    {
        if (IsHost)
        {
            RelayManager.GameStarted = true;
            NetworkManager.Singleton.SceneManager.LoadScene("MultiplayerGameScene", LoadSceneMode.Single);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (clientId == NetworkManager.Singleton.LocalClientId)
            return;

        string nickname = PlayerInfo.GetNickname(clientId);
        string msg = $"{nickname} joined the room.\n";

        AppendFeedback(msg);

        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                    .Where(id => id != NetworkManager.Singleton.LocalClientId && id != clientId)
                    .ToArray()
            }
        };
        UpdateFeedbackClientRpc(msg, rpcParams);

        UpdatePersonalFeedbackClientRpc("You joined the room.\n", new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        });

        UpdatePlayerCount();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        string nickname = PlayerInfo.GetNickname(clientId);
        string msg = $"{nickname} left the room.\n";

        AppendFeedback(msg);

        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                    .Where(id => id != NetworkManager.Singleton.LocalClientId)
                    .ToArray()
            }
        };
        UpdateFeedbackClientRpc(msg, rpcParams);

        UpdatePlayerCount();
    }

    [ClientRpc]
    private void UpdateFeedbackClientRpc(string message, ClientRpcParams rpcParams = default)
    {
        AppendFeedback(message);
    }

    [ClientRpc]
    private void UpdatePersonalFeedbackClientRpc(string message, ClientRpcParams rpcParams = default)
    {
        AppendFeedback(message);
    }

    private void AppendFeedback(string message)
    {
        feedbackText.text += message;
    }

    private void UpdatePlayerCount()
    {
        int current = NetworkManager.Singleton.ConnectedClientsIds.Count;
        int max = RelayManager.MaxPlayers;
        playerCountText.text = $"Players: {current}/{max}";
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        startGameButton.onClick.RemoveAllListeners();
    }
}
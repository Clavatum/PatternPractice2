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
    [SerializeField] private Button startGameButton;

    void Awake()
    {
        startGameButton.onClick.AddListener(StartGame);
    }

    void Start()
    {
        if (!IsHost)
            startGameButton.gameObject.SetActive(false);

        if (IsHost)
            roomCodeText.text = $"Room Code: {MultiplayerMenuManager.RoomCode}";
        else
            roomCodeText.gameObject.SetActive(false);

        connectionStatusText.text = IsHost ? "Host" : "Client";

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (IsHost)
        {
            AppendFeedback("Room created. Waiting for players...\n");
        }
    }

    private void StartGame()
    {
        if (IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MultiplayerGameScene", LoadSceneMode.Single);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        string message;
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            message = "Room created. Waiting for players...\n";
            AppendFeedback(message);
            return;
        }
        else
        {
            message = $"Client {clientId} joined the room.\n";

            AppendFeedback(message);

            var rpcParameters = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                        .Where(id => id != NetworkManager.Singleton.LocalClientId && id != clientId)
                        .ToArray()
                }
            };

            UpdateFeedbackClientRpc(message, rpcParameters);

            UpdatePersonalFeedbackClientRpc("You joined the room.\n", new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        string message = $"Client {clientId} left the room.\n";

        AppendFeedback(message);

        var rpcParameters = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                    .Where(id => id != NetworkManager.Singleton.LocalClientId)
                    .ToArray()
            }
        };

        UpdateFeedbackClientRpc(message, rpcParameters);
    }

    [ClientRpc]
    private void UpdateFeedbackClientRpc(string message, ClientRpcParams rpcParameters = default)
    {
        AppendFeedback(message);
    }

    [ClientRpc]
    private void UpdatePersonalFeedbackClientRpc(string message, ClientRpcParams rpcParameters = default)
    {
        AppendFeedback(message);
    }

    private void AppendFeedback(string message)
    {
        feedbackText.text += message;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        startGameButton.onClick.RemoveAllListeners();
    }
}

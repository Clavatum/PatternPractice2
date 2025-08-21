using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    [SerializeField] private Button startGameButton;

    void Start()
    {
        if (!NetworkManager.Singleton.IsHost)
            startGameButton.interactable = false;

        if (NetworkManager.Singleton.IsHost)
            roomCodeText.text = $"Room Code: {MultiplayerMenuManager.RoomCode}";
        else
            Destroy(roomCodeText);

        connectionStatusText.text = NetworkManager.Singleton.IsHost ? "Host" : "Connecting...";

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} joined the room.");
        if (NetworkManager.Singleton.IsClient) connectionStatusText.text = "Connected";
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} left the room.");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
}

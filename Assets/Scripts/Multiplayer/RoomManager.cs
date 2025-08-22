using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    [SerializeField] private Button startGameButton;

    void Awake()
    {
        startGameButton.onClick.AddListener(() =>
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("MultiplayerGameScene", LoadSceneMode.Single);
                }
            });
    }

    void Start()
    {
        if (!NetworkManager.Singleton.IsHost)
            startGameButton.gameObject.SetActive(false);

        if (NetworkManager.Singleton.IsHost)
            roomCodeText.text = $"Room Code: {MultiplayerMenuManager.RoomCode}";
        else
            roomCodeText.gameObject.SetActive(false);

        connectionStatusText.text = NetworkManager.Singleton.IsHost ? "Host" : "Client";

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} joined the room.");
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

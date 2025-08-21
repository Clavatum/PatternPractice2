using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetcodeButton : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;

    private void StartHost()
    {
        networkManager.StartHost();
    }

    private void StartClient()
    {
        networkManager.StartClient();
    }

    void OnEnable()
    {
        StartHostButton.onClick.AddListener(StartHost);
        StartClientButton.onClick.AddListener(StartClient);
    }

    void OnDisable()
    {
        StartHostButton.onClick.RemoveAllListeners();
        StartClientButton.onClick.RemoveAllListeners();
    }
}

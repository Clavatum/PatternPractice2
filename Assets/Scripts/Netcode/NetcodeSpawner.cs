using Unity.Netcode;
using UnityEngine;

public class NetcodeSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    [SerializeField] private GameObject balloonPrefab;
    [SerializeField] private GameObject buttonPrefab;

    private void OnEnable()
    {
        networkManager.OnClientConnectedCallback += HandleClientConnected;
    }

    private void OnDisable()
    {
        if (networkManager != null)
            networkManager.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (clientId == networkManager.LocalClientId)
        {
            SpawnSharedObjects();
        }

        SpawnPlayerObjects(clientId);
    }

    private void SpawnSharedObjects()
    {
        var button = Instantiate(buttonPrefab);
        button.GetComponent<NetworkObject>().Spawn(true);
    }

    private void SpawnPlayerObjects(ulong clientId)
    {
        var balloon = Instantiate(balloonPrefab);
        balloon.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
}
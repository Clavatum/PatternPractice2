using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject balloonPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                SpawnBalloonForClient(clientId);
            }

            NetworkManager.Singleton.OnClientConnectedCallback += SpawnBalloonForClient;
        }
    }

    private void Start()
    {
        Debug.Log($"IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, LocalClientId: {NetworkManager.Singleton.LocalClientId}");
    }

    private void SpawnBalloonForClient(ulong clientId)
    {
        Debug.Log($"Spawning balloon for player {clientId}");

        int index = (int)clientId % spawnPoints.Length;

        var balloon = Instantiate(balloonPrefab, spawnPoints[index].localPosition, spawnPoints[index].localRotation);
        balloon.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    public override void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnBalloonForClient;
        }
    }
}

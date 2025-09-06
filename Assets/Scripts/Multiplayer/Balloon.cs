using Unity.Netcode;
using UnityEngine;

public class Balloon : NetworkBehaviour
{
    [SerializeField] private GameObject myCamera;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            var spawnManager = FindAnyObjectByType<SpawnManager>();
            spawnManager.MyBalloon = gameObject;
            myCamera.SetActive(true);
        }
        else
        {
            myCamera.SetActive(false);
        }
    }
}
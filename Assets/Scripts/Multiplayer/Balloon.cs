using Unity.Netcode;
using UnityEngine;

public class Balloon : NetworkBehaviour
{
    private Camera myCamera;

    private void Awake()
    {
        myCamera = GetComponentInChildren<Camera>(true);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            var spawnManager = FindAnyObjectByType<SpawnManager>();
            spawnManager.MyBalloon = gameObject;
            myCamera.enabled = true;
            AudioListener listener = myCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = true;
        }
        else
        {
            myCamera.enabled = false;
            AudioListener listener = myCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
    }
}
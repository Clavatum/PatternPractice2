using Unity.Netcode;
using UnityEngine;

public class MultiplayerAudioSystem : NetworkBehaviour
{
    public AudioSource AudioSource;
    [SerializeField] private AudioClip popBalloonClip;
    [SerializeField] private AudioClip inflateBalloonClip;
    [SerializeField] private AudioClip deflateBalloonClip;

    private void PlaySound(bool isBalloonPopped, bool isBalloonInflated)
    {
        if (isBalloonPopped)
        {
            AudioSource.PlayOneShot(popBalloonClip);
            return;
        }

        if (isBalloonInflated)
        {
            AudioSource.PlayOneShot(inflateBalloonClip);
        }
        else
        {
            AudioSource.PlayOneShot(deflateBalloonClip);
        }
    }

    public void TriggerSound(bool isBalloonPopped, bool isBalloonInflated)
    {
        if (IsServer)
        {
            var target = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { OwnerClientId }
                }
            };

            PlaySoundClientRpc(isBalloonPopped, isBalloonInflated, target);
        }
        else if (IsOwner)
        {
            PlaySound(isBalloonPopped, isBalloonInflated);
        }
    }

    [ClientRpc]
    private void PlaySoundClientRpc(bool isBalloonPopped, bool isBalloonInflated, ClientRpcParams rpcParams = default)
    {
        PlaySound(isBalloonPopped, isBalloonInflated);
    }
}

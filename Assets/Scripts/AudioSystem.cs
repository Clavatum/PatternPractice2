using System;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
    public event Action<bool, bool> OnBalloonInflated;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip popBalloonClip;
    [SerializeField] private AudioClip inflateBalloonClip;
    [SerializeField] private AudioClip deflateBalloonClip;

    private void PlaySound(bool isBalloonPopped, bool isBalloonInflated)
    {
        if (isBalloonPopped)
        {
            audioSource.clip = popBalloonClip;
            audioSource.PlayOneShot(popBalloonClip);
            audioSource.volume = 1f;
            return;
        }
        audioSource.volume = 0.15f;
        if (isBalloonInflated)
        {
            audioSource.clip = inflateBalloonClip;
            audioSource.PlayOneShot(inflateBalloonClip);
        }
        else
        {
            audioSource.clip = deflateBalloonClip;
            audioSource.PlayOneShot(deflateBalloonClip);
        }
    }

    public void TriggerBalloonInflatedEvent(bool isBalloonPopped, bool isBalloonInflated) => OnBalloonInflated?.Invoke(isBalloonPopped, isBalloonInflated);

    void OnEnable()
    {
        OnBalloonInflated += PlaySound;
    }

    void OnDisable()
    {
        OnBalloonInflated -= PlaySound;
    }
}
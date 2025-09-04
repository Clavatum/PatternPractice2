using System;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
    public event Action<bool, bool> OnBalloonInflated;

    public AudioSource AudioSource;
    [SerializeField] private AudioClip popBalloonClip;
    [SerializeField] private AudioClip inflateBalloonClip;
    [SerializeField] private AudioClip deflateBalloonClip;

    private void PlaySound(bool isBalloonPopped, bool isBalloonInflated)
    {
        if (isBalloonPopped)
        {
            AudioSource.clip = popBalloonClip;
            AudioSource.PlayOneShot(popBalloonClip);
            AudioSource.volume = 1f;
            return;
        }
        AudioSource.volume = 0.15f;
        if (isBalloonInflated)
        {
            AudioSource.clip = inflateBalloonClip;
            AudioSource.PlayOneShot(inflateBalloonClip);
        }
        else
        {
            AudioSource.clip = deflateBalloonClip;
            AudioSource.PlayOneShot(deflateBalloonClip);
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
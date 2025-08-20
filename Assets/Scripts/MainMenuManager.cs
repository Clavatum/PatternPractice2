using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playOfflineButton;
    [SerializeField] private Button playOnlineButton;

    private void PlayOffline()
    {
        SceneManager.LoadScene(1);
    }

    private void PlayOnline()
    {
        SceneManager.LoadScene(2);
    }

    void OnEnable()
    {
        playOfflineButton.onClick.AddListener(PlayOffline);
        playOnlineButton.onClick.AddListener(PlayOnline);
    }

    void OnDisable()
    {
        playOfflineButton.onClick.RemoveAllListeners();
        playOnlineButton.onClick.RemoveAllListeners();
    }
}
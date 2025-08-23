using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public event Action OnGameEnd;

    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private Button playAgainButton;

    void Awake()
    {
        playAgainButton.onClick.AddListener(PlayAgain);
    }

    private void SetGameEndPanel()
    {
        gameEndPanel.SetActive(true);
    }

    private void PlayAgain()
    {
        SceneManager.LoadScene("SinglePlayerGameScene");
    }

    public void TriggerGameEnd() => OnGameEnd?.Invoke();

    void OnEnable()
    {
        OnGameEnd += SetGameEndPanel;
    }

    void OnDisable()
    {
        playAgainButton.onClick.RemoveAllListeners();
        OnGameEnd -= SetGameEndPanel;
    }
}

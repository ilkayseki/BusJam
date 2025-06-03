using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IGameStateObserver
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    private void Start()
    {
        GameManager.Instance.RegisterObserver(this);
    }

    public void OnGameStateChanged(GameState newState)
    {
        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);

        switch (newState)
        {
            case GameState.Start:
                startPanel.SetActive(true);
                break;
            case GameState.GameOver:
                gameOverPanel.SetActive(true);
                break;
            case GameState.Finished:
                victoryPanel.SetActive(true);
                break;
        }
    }
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterObserver(this);
    }
}
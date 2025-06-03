using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour, IGameStateObserver
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [Scene] 
    [SerializeField] private string _sceneToLoad;

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
    
    public void LoadSelectedScene()
    {
        if (!Application.CanStreamedLevelBeLoaded(_sceneToLoad))
        {
            Debug.LogWarning($"Scene '{_sceneToLoad}' is not in the build settings or the name is incorrect.");
            return;
        }

        StartCoroutine(LoadSceneAsync(_sceneToLoad));
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");

            // %90'a geldiğinde sahne hazır olur ama aktarılmaz.
            if (asyncLoad.progress >= 0.9f)
            {
                Debug.Log("Scene ready. Activating...");
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
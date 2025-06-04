using UnityEngine;

public class CurrentLevelManager : MonoBehaviourSingletonPersistent<CurrentLevelManager>, IGameStateObserver
{
    private string _currentJsonPath;
    private int _maxUnlockedLevel = 1;
    private int _currentLevelNumber;

    public string CurrentJsonPath => _currentJsonPath;
    public int MaxUnlockedLevel => _maxUnlockedLevel;

    private void Start()
    {
        // PlayerPrefs kontrolü
        if (!PlayerPrefs.HasKey(FilePathManager.Instance.MaxUnlockedLevel))
        {
            PlayerPrefs.SetInt(FilePathManager.Instance.MaxUnlockedLevel, 1);
            PlayerPrefs.Save();
        }
        _maxUnlockedLevel = PlayerPrefs.GetInt(FilePathManager.Instance.MaxUnlockedLevel, 1);
    }

    public void SetCurrentLevel(string jsonPath)
    {
        _currentJsonPath = jsonPath.Replace(FilePathManager.Instance.Json, "");
        _currentLevelNumber = int.Parse(_currentJsonPath.Replace(FilePathManager.Instance.LevelsLevel, ""));
    }

    public void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Finished)
        {
            CompleteLevel(_currentLevelNumber);
        }
    }

    private void CompleteLevel(int levelNumber)
    {
        if (levelNumber >= _maxUnlockedLevel)
        {
            _maxUnlockedLevel = levelNumber + 1;
            PlayerPrefs.SetInt(FilePathManager.Instance.MaxUnlockedLevel, _maxUnlockedLevel);
            PlayerPrefs.Save();
            Debug.Log($"Level {levelNumber} completed! Unlocked level {_maxUnlockedLevel}");
        }
    }

    private void OnDestroy()
    {
        // Observer listesinden çıkar
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterObserver(this);
        }
    }
}
using UnityEngine;

public class CurrentLevelManager : MonoBehaviourSingletonPersistent<CurrentLevelManager>,IGameStateObserver
{
    private string _currentJsonPath;
    private int _maxUnlockedLevel = 1;
    private int _currentLevelNumber;

    public string CurrentJsonPath => _currentJsonPath;
    public int MaxUnlockedLevel => _maxUnlockedLevel;

    private void Start()
    {
        // PlayerPrefs kontrolü
        if (!PlayerPrefs.HasKey("MaxUnlockedLevel"))
        {
            PlayerPrefs.SetInt("MaxUnlockedLevel", 1);
            PlayerPrefs.Save();
        }
        _maxUnlockedLevel = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
    }

    public void SetCurrentLevel(string jsonPath)
    {
        // .json uzantısı varsa kaldır
        _currentJsonPath = jsonPath.Replace(".json", "");
        _currentLevelNumber = int.Parse(_currentJsonPath.Replace("Levels/level", ""));
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
            PlayerPrefs.SetInt("MaxUnlockedLevel", _maxUnlockedLevel);
            PlayerPrefs.Save();
            Debug.Log($"Level {levelNumber} completed! Unlocked level {_maxUnlockedLevel}");
        }
    }
}
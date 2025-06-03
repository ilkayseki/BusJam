using UnityEngine;

public class LevelLoadManager : MonoBehaviourSingleton<LevelLoadManager>
{
    private LevelData _currentLevelData;
    public string COLOR_PATH = "Color/ColorData";


    public LevelData CurrentLevelData => _currentLevelData;

    private void Awake()
    {
        LoadAndSetupLevel();
    }

    public void LoadAndSetupLevel()
    {
        // CurrentLevelManager'dan path'i al
        string levelPath = CurrentLevelManager.Instance.CurrentJsonPath;
    
        TextAsset levelFile = Resources.Load<TextAsset>(levelPath);
        if (levelFile == null)
        {
            Debug.LogError($"Level file not found at: {levelPath}");
            return;
        }

        _currentLevelData = JsonUtility.FromJson<LevelData>(levelFile.text);
    
        // CurrentLevelManager'a level numarasını bildir
        CurrentLevelManager.Instance.SetCurrentLevel(levelPath);
        if (_currentLevelData == null)
        {
            Debug.LogError("Failed to parse level data!");
            return;
        }

        InitializeManagers();
    }
    private void InitializeManagers()
    {
        // ColorData'yı bul
        ColorData colorData = Resources.Load<ColorData>(COLOR_PATH);
        if (colorData == null)
        {
            Debug.LogError("ColorData not found in scene!");
            return;
        }

        // WaitingArea'yi başlat
        if (WaitingArea.Instance != null)
        {
            WaitingArea.Instance.InitializeWaitingArea(_currentLevelData.waitingAreaSize);
        }

        // GridManager'ı başlat
        if (GridManager.Instance != null)
        {
            GridManager.Instance.CreateGridFromLevelData(_currentLevelData, colorData);
        }

        // BusManager'ı başlat
        if (BusManager.Instance != null && _currentLevelData.buses != null)
        {
            BusManager.Instance.InitializeBuses(_currentLevelData.buses, colorData);
        }
        // TimeManager'ı başlat
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.Initialize(_currentLevelData.levelTime);
        }

    }
}
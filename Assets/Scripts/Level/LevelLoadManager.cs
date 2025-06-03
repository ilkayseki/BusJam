using UnityEngine;

public class LevelLoadManager : MonoBehaviourSingleton<LevelLoadManager>
{
    public string LEVEL_PATH = "Levels/level1";
    private LevelData _currentLevelData;
    public string COLOR_PATH = "Color/ColorData";


    public LevelData CurrentLevelData => _currentLevelData;

    private void Awake()
    {
        LoadAndSetupLevel1();
    }

    public void LoadAndSetupLevel1()
    {
        // CurrentLevelManager'dan path'i al
        string levelPath = CurrentLevelManager.Instance.CurrentJsonPath;
    
        TextAsset levelFile = Resources.Load<TextAsset>(LEVEL_PATH);
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

    public void LoadAndSetupLevel()
    {
        TextAsset levelFile = Resources.Load<TextAsset>(LEVEL_PATH);
        if (levelFile == null)
        {
            Debug.LogError($"Level file not found at: {LEVEL_PATH}");
            return;
        }

        _currentLevelData = JsonUtility.FromJson<LevelData>(levelFile.text);
        if (_currentLevelData == null)
        {
            Debug.LogError("Failed to parse level data!");
            return;
        }
        _currentLevelData = JsonUtility.FromJson<LevelData>(levelFile.text);
        InitializeManagers();
    }

    private void InitializeManagers()
    {
        // ColorData'yı bul
        ColorData colorData = Resources.Load<ColorData>(COLOR_PATH);;
        if (colorData == null)
        {
            Debug.LogError("ColorData not found in scene!");
            return;
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
    }
}
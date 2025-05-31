// LevelLoader.cs
using UnityEngine;
using System.IO;

public class LevelLoader : MonoBehaviour
{
    public ColorData colorData;
    public TextAsset levelJsonFile;
    
    public void LoadLevel(LevelData levelData)
    {
        // Implement your grid creation logic here based on the levelData
        Debug.Log($"Loading level: {levelData.width}x{levelData.height}");
        
        for (int i = 0; i < levelData.nodeColors.Length; i++)
        {
            int x = i % levelData.width;
            int y = i / levelData.width;
            string colorName = levelData.nodeColors[i];
            
            if (!string.IsNullOrEmpty(colorName))
            {
                Debug.Log($"Node ({x},{y}) color: {colorName}");
                // Here you would create your grid nodes with the specified colors
            }
        }
    }

    public void LoadLevelFromJson(string json)
    {
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        LoadLevel(levelData);
    }

    [ContextMenu("Load Test Level")]
    public void LoadTestLevel()
    {
        if (levelJsonFile != null)
        {
            LoadLevelFromJson(levelJsonFile.text);
        }
    }
}
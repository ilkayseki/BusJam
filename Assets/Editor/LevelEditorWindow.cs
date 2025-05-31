using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class LevelEditorWindow : EditorWindow
{
    private int gridWidth = 5;
    private int gridHeight = 5;
    private ColorData colorData;
    private string[] colorOptions;
    private int selectedColorIndex = 0;
    
    private LevelData currentLevel;
    private Texture2D[,] gridTextures;
    private Vector2 scrollPosition;
    private string[] availableLevels;
    private int selectedLevelIndex = 0;
    private bool showLevelSelection = true;

    [MenuItem("Window/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        LoadColorData();
        RefreshLevelList();
    }

    private void LoadColorData()
    {
        colorData = Resources.Load<ColorData>("Color/ColorData");
        if (colorData != null && colorData.colors != null)
        {
            colorOptions = colorData.colors.Select(c => c.colorName).ToArray();
        }
        else
        {
            Debug.LogError("ColorData not found at path: Resources/Color/ColorData");
        }
    }

    private void RefreshLevelList()
    {
        TextAsset[] levelAssets = Resources.LoadAll<TextAsset>("Levels");
        availableLevels = levelAssets.Select(level => level.name).ToArray();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        try
        {
            if (colorData == null || colorOptions == null || colorOptions.Length == 0)
            {
                EditorGUILayout.HelpBox("ColorData not loaded properly at path: Resources/Color/ColorData", MessageType.Error);
                if (GUILayout.Button("Reload ColorData"))
                {
                    LoadColorData();
                }
                return;
            }

            // Level Selection Section
            EditorGUILayout.Space();
            showLevelSelection = EditorGUILayout.Foldout(showLevelSelection, "Level Selection", true);
            if (showLevelSelection)
            {
                EditorGUILayout.BeginVertical("box");
                
                if (availableLevels != null && availableLevels.Length > 0)
                {
                    selectedLevelIndex = EditorGUILayout.Popup("Available Levels", selectedLevelIndex, availableLevels);
                    if (GUILayout.Button("Load Selected Level"))
                    {
                        LoadSelectedLevel();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No levels found in Resources/Levels folder", MessageType.Info);
                }

                if (GUILayout.Button("Refresh Level List"))
                {
                    RefreshLevelList();
                }

                EditorGUILayout.EndVertical();
            }

            // Grid Settings Section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
            gridWidth = EditorGUILayout.IntField("Width", gridWidth);
            gridHeight = EditorGUILayout.IntField("Height", gridHeight);
            
            // Color Selection
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Color Selection", EditorStyles.boldLabel);
            selectedColorIndex = EditorGUILayout.Popup("Current Color", selectedColorIndex, colorOptions);

            // Grid Operations
            EditorGUILayout.Space();
            if (GUILayout.Button("Create New Grid"))
            {
                CreateNewGrid();
            }

            // Grid Display
            if (currentLevel != null && gridTextures != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Grid Painting", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Left-click to paint, Right-click to clear", MessageType.Info);
                DrawGridEditor();

                // Save Buttons
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Save Level", GUILayout.Height(30)))
                {
                    SaveLevel();
                }
                if (GUILayout.Button("Save As New", GUILayout.Height(30)))
                {
                    SaveLevelAsNew();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        finally
        {
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }

    private void LoadSelectedLevel()
    {
        TextAsset[] levelAssets = Resources.LoadAll<TextAsset>("Levels");
        if (selectedLevelIndex >= 0 && selectedLevelIndex < levelAssets.Length)
        {
            LoadLevel(levelAssets[selectedLevelIndex]);
        }
    }

    private void LoadLevel(TextAsset levelFile)
    {
        try
        {
            currentLevel = JsonUtility.FromJson<LevelData>(levelFile.text);
            gridWidth = currentLevel.width;
            gridHeight = currentLevel.height;
            
            gridTextures = new Texture2D[gridWidth, gridHeight];
            
            // Load colors directly without flipping (storage matches display)
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    string colorName = currentLevel.nodeColors[y * gridWidth + x];
                    Color color = string.IsNullOrEmpty(colorName) ? Color.white : colorData.GetColor(colorName);
                    gridTextures[x, y] = CreateColorTexture(color);
                }
            }
            
            Debug.Log($"Level loaded successfully: {levelFile.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load level: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to load level:\n{e.Message}", "OK");
        }
    }

    private void CreateNewGrid()
    {
        currentLevel = new LevelData
        {
            width = Mathf.Clamp(gridWidth, 1, 20),
            height = Mathf.Clamp(gridHeight, 1, 20),
            nodeColors = new string[gridWidth * gridHeight]
        };

        gridTextures = new Texture2D[currentLevel.width, currentLevel.height];
        for (int x = 0; x < currentLevel.width; x++)
        {
            for (int y = 0; y < currentLevel.height; y++)
            {
                gridTextures[x, y] = CreateColorTexture(Color.white);
                currentLevel.nodeColors[y * currentLevel.width + x] = "";
            }
        }
    }

    private void DrawGridEditor()
    {
        if (currentLevel == null || gridTextures == null) return;

        // Draw grid with y=0 at the top (natural order)
        for (int y = 0; y < currentLevel.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < currentLevel.width; x++)
            {
                if (x >= gridTextures.GetLength(0) || y >= gridTextures.GetLength(1)) continue;

                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(50), GUILayout.Height(50));
                
                Event evt = Event.current;
                if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
                {
                    if (evt.button == 0 && selectedColorIndex < colorOptions.Length)
                    {
                        string colorName = colorOptions[selectedColorIndex];
                        currentLevel.nodeColors[y * currentLevel.width + x] = colorName;
                        gridTextures[x, y] = CreateColorTexture(colorData.GetColor(colorName));
                    }
                    else if (evt.button == 1)
                    {
                        currentLevel.nodeColors[y * currentLevel.width + x] = "";
                        gridTextures[x, y] = CreateColorTexture(Color.white);
                    }
                    GUI.changed = true;
                }

                if (gridTextures[x, y] != null)
                {
                    EditorGUI.DrawPreviewTexture(rect, gridTextures[x, y]);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void SaveLevel()
    {
        if (selectedLevelIndex >= 0 && selectedLevelIndex < availableLevels.Length)
        {
            string levelName = availableLevels[selectedLevelIndex];
            string path = Path.Combine(Application.dataPath, "Resources", "Levels", levelName + ".json");
            SaveLevelToPath(path);
        }
        else
        {
            SaveLevelAsNew();
        }
    }

    private void SaveLevelAsNew()
    {
        string path = EditorUtility.SaveFilePanel(
            "Save Level As JSON",
            Path.Combine(Application.dataPath, "Resources", "Levels"),
            "NewLevel.json",
            "json");

        if (!string.IsNullOrEmpty(path))
        {
            SaveLevelToPath(path);
            RefreshLevelList();
        }
    }

    private void SaveLevelToPath(string path)
    {
        try
        {
            // Save in display order (no coordinate flipping needed)
            string json = JsonUtility.ToJson(currentLevel, true);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log($"Level saved successfully to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save level: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to save level:\n{e.Message}", "OK");
        }
    }

    private Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(16, 16);
        Color[] pixels = Enumerable.Repeat(color, 16 * 16).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
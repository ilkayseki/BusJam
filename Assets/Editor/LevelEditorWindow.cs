using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

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
        if (colorData != null)
        {
            colorOptions = colorData.GetColorNames();
        }
        else
        {
            Debug.LogError("ColorData not found at path: Resources/Color/ColorData");
            colorOptions = new string[0];
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

        if (colorData == null || colorOptions.Length == 0)
        {
            EditorGUILayout.HelpBox("ColorData not loaded properly", MessageType.Error);
            if (GUILayout.Button("Reload ColorData"))
            {
                LoadColorData();
            }
            return;
        }

        DrawLevelSelectionSection();
        DrawGridSettingsSection();
        DrawColorSelectionSection();
        DrawGridOperationsSection();
        DrawGridDisplaySection();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawLevelSelectionSection()
    {
        EditorGUILayout.Space();
        showLevelSelection = EditorGUILayout.Foldout(showLevelSelection, "Level Selection", true);
        if (!showLevelSelection) return;

        EditorGUILayout.BeginVertical("box");
        
        if (availableLevels.Length > 0)
        {
            selectedLevelIndex = EditorGUILayout.Popup("Available Levels", selectedLevelIndex, availableLevels);
            if (GUILayout.Button("Load Selected Level"))
            {
                LoadSelectedLevel();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No levels found in Resources/Levels", MessageType.Info);
        }

        if (GUILayout.Button("Refresh Level List"))
        {
            RefreshLevelList();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawGridSettingsSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Width", gridWidth);
        gridHeight = EditorGUILayout.IntField("Height", gridHeight);
    }

    private void DrawColorSelectionSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Color Selection", EditorStyles.boldLabel);
        selectedColorIndex = EditorGUILayout.Popup("Current Color", selectedColorIndex, colorOptions);
    }

    private void DrawGridOperationsSection()
    {
        EditorGUILayout.Space();
        if (GUILayout.Button("Create New Grid"))
        {
            CreateNewGrid();
        }
    }

    private void DrawGridDisplaySection()
    {
        if (currentLevel == null || gridTextures == null) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Painting", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Left-click to paint, Right-click to clear", MessageType.Info);

        for (int y = 0; y < currentLevel.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < currentLevel.width; x++)
            {
                DrawGridCell(x, y);
            }
            EditorGUILayout.EndHorizontal();
        }

        DrawSaveButtons();
    }

    private void DrawGridCell(int x, int y)
    {
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(50), GUILayout.Height(50));
        
        if (gridTextures[x, y] != null)
        {
            EditorGUI.DrawPreviewTexture(rect, gridTextures[x, y]);
        }

        string colorName = currentLevel.nodeColors[y * currentLevel.width + x];
        if (!string.IsNullOrEmpty(colorName) && !colorData.ShouldSpawnCharacter(colorName))
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            EditorGUI.LabelField(rect, "X", style);
        }

        HandleCellClick(rect, x, y);
    }

    private void HandleCellClick(Rect rect, int x, int y)
    {
        Event evt = Event.current;
        if (evt.type != EventType.MouseDown || !rect.Contains(evt.mousePosition)) return;

        if (evt.button == 0 && selectedColorIndex < colorOptions.Length)
        {
            currentLevel.nodeColors[y * currentLevel.width + x] = colorOptions[selectedColorIndex];
            gridTextures[x, y] = CreateColorTexture(colorData.GetColor(colorOptions[selectedColorIndex]));
        }
        else if (evt.button == 1)
        {
            currentLevel.nodeColors[y * currentLevel.width + x] = "";
            gridTextures[x, y] = CreateColorTexture(Color.white);
        }
        GUI.changed = true;
    }

    private void DrawSaveButtons()
    {
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
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    string colorName = currentLevel.nodeColors[y * gridWidth + x];
                    Color color = string.IsNullOrEmpty(colorName) ? Color.white : colorData.GetColor(colorName);
                    gridTextures[x, y] = CreateColorTexture(color);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Level load failed: {e.Message}");
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

        gridTextures = new Texture2D[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                gridTextures[x, y] = CreateColorTexture(Color.white);
                currentLevel.nodeColors[y * gridWidth + x] = "";
            }
        }
    }

    private Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void SaveLevel()
    {
        if (selectedLevelIndex >= 0 && selectedLevelIndex < availableLevels.Length)
        {
            string path = Path.Combine(Application.dataPath, "Resources", "Levels", availableLevels[selectedLevelIndex] + ".json");
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
            "Save Level",
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
            string json = JsonUtility.ToJson(currentLevel, true);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Level save failed: {e.Message}");
        }
    }
}
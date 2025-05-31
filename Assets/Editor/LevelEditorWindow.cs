using UnityEditor;
using UnityEngine;
using System.IO;
using System;

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
    private bool initialized = false;

    [MenuItem("Window/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        initialized = false;
        LoadColorData();
    }

    private void LoadColorData()
    {
        string[] guids = AssetDatabase.FindAssets("t:ColorData");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            colorData = AssetDatabase.LoadAssetAtPath<ColorData>(path);
            
            if (colorData != null && colorData.colors != null)
            {
                colorOptions = new string[colorData.colors.Length];
                for (int i = 0; i < colorData.colors.Length; i++)
                {
                    colorOptions[i] = colorData.colors[i].colorName;
                }
                initialized = true;
                return;
            }
        }
        
        Debug.LogError("ColorData not found or invalid!");
        initialized = false;
    }

    private void OnGUI()
    {
        if (!initialized)
        {
            EditorGUILayout.HelpBox("ColorData not loaded properly. Please check your ColorData asset.", MessageType.Error);
            if (GUILayout.Button("Try Reload ColorData"))
            {
                LoadColorData();
            }
            return;
        }

        EditorGUILayout.BeginVertical();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Width", gridWidth);
        gridHeight = EditorGUILayout.IntField("Height", gridHeight);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Color Selection", EditorStyles.boldLabel);
        if (colorOptions != null && colorOptions.Length > 0)
        {
            selectedColorIndex = EditorGUILayout.Popup("Current Color", selectedColorIndex, colorOptions);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Create/Reset Grid"))
        {
            CreateGrid();
        }

        if (currentLevel != null && gridTextures != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid Painting", EditorStyles.boldLabel);
            DrawGridEditor();
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Save Level"))
            {
                SaveLevel();
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void CreateGrid()
    {
        currentLevel = new LevelData
        {
            width = Mathf.Clamp(gridWidth, 1, 20),
            height = Mathf.Clamp(gridHeight, 1, 20),
            nodeColors = new string[gridWidth * gridHeight]
        };

        gridTextures = new Texture2D[currentLevel.width, currentLevel.height];
        InitializeGridTextures();
    }

    private void InitializeGridTextures()
    {
        for (int x = 0; x < currentLevel.width; x++)
        {
            for (int y = 0; y < currentLevel.height; y++)
            {
                gridTextures[x, y] = CreateColorTexture(Color.white);
                currentLevel.nodeColors[y * currentLevel.width + x] = "";
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

    private void DrawGridEditor()
    {
        if (currentLevel == null || gridTextures == null) return;

        EditorGUILayout.LabelField("Click on cells to paint with selected color");
        EditorGUILayout.LabelField("Right-click to clear cell (white = empty)");

        for (int y = currentLevel.height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < currentLevel.width; x++)
            {
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(50), GUILayout.Height(50));
            
                if (x >= gridTextures.GetLength(0) || y >= gridTextures.GetLength(1))
                {
                    continue;
                }

                Event evt = Event.current;
                if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
                {
                    if (evt.button == 0 && selectedColorIndex < colorOptions.Length)
                    {
                        string colorName = colorOptions[selectedColorIndex];
                        currentLevel.nodeColors[y * currentLevel.width + x] = colorName;
                        gridTextures[x, y] = CreateColorTexture(colorName == "White" ? Color.white : colorData.GetColor(colorName));
                    }
                    else if (evt.button == 1)
                    {
                        currentLevel.nodeColors[y * currentLevel.width + x] = "White";
                        gridTextures[x, y] = CreateColorTexture(Color.white);
                    }
                    GUI.changed = true;
                }

                EditorGUI.DrawPreviewTexture(rect, gridTextures[x, y]);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void SaveLevel()
    {
        if (currentLevel == null) return;

        // Grid verilerini doğru sırayla düzenle (y eksenini flip et)
        string[] correctedNodeColors = new string[currentLevel.width * currentLevel.height];
        for (int y = 0; y < currentLevel.height; y++)
        {
            for (int x = 0; x < currentLevel.width; x++)
            {
                int originalIndex = y * currentLevel.width + x;
                int correctedIndex = (currentLevel.height - 1 - y) * currentLevel.width + x;
                correctedNodeColors[correctedIndex] = currentLevel.nodeColors[originalIndex];
            }
        }
        currentLevel.nodeColors = correctedNodeColors;

        string path = EditorUtility.SaveFilePanel("Save Level", "Assets/Resources/Levels", "level", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = JsonUtility.ToJson(currentLevel, true);
            File.WriteAllText(path, json);
        
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                AssetDatabase.Refresh();
            }
            Debug.Log("Level saved to: " + path);
        }
    }
}
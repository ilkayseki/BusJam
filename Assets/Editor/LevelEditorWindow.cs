using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class LevelEditorWindow : EditorWindow
{
    private int gridWidth = 5;
    private int gridHeight = 5;
    private int waitingAreaSize = 3;
    private ColorData colorData;
    private string[] colorOptions;
    private int selectedColorIndex = 0;
    
    private LevelData currentLevel;
    private Texture2D[,] gridTextures;
    private Vector2 scrollPosition;
    private string[] availableLevels;
    private int selectedLevelIndex = 0;
    private bool showLevelSelection = true;
    
    private List<BusData> busConfigurations = new List<BusData>();
    private Vector2 busScrollPosition;
    private int newBusSeatCount = 2;
    private int selectedBusColorIndex = 0;

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
            waitingAreaSize = EditorGUILayout.IntField("Waiting Area Size", waitingAreaSize);
            // Grid Operations
            EditorGUILayout.Space();
            if (GUILayout.Button("Create New Grid"))
            {
                CreateNewGrid();
            }
            
            //Time Settings Section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Time Settings", EditorStyles.boldLabel);
            if (currentLevel != null)
            {
                currentLevel.levelTime = EditorGUILayout.FloatField("Level Time (seconds)", currentLevel.levelTime);
            }
            else
            {
                EditorGUILayout.FloatField("Level Time (seconds)", 60f);
            }
            
            // Color Selection
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Color Selection", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            selectedColorIndex = EditorGUILayout.Popup("Current Color", selectedColorIndex, colorOptions);
            
            // Yeni eklenen refresh butonu
            if (GUILayout.Button("Refresh Colors", GUILayout.Width(120)))
            {
                LoadColorData();
                Debug.Log("ColorData refreshed!");

                // Bus konfigürasyonlarını güncelle
                if (busConfigurations != null)
                {
                    var validColors = colorData.colors.Select(c => c.colorName).ToArray();
        
                    for (int i = 0; i < busConfigurations.Count; i++)
                    {
                        // Eğer mevcut renk artık geçerli değilse, ilk geçerli renk ile değiştir
                        if (!validColors.Contains(busConfigurations[i].colorName))
                        {
                            if (validColors.Length > 0)
                            {
                                busConfigurations[i].colorName = validColors[0];
                                Debug.Log($"Updated bus configuration {i} to use color {validColors[0]}");
                            }
                            else
                            {
                                Debug.LogWarning("No valid colors available in ColorData!");
                            }
                        }
                    }
                }

                // Grid renklerini yenile
                if (currentLevel != null && gridTextures != null)
                {
                    for (int x = 0; x < currentLevel.width; x++)
                    {
                        for (int y = 0; y < currentLevel.height; y++)
                        {
                            string colorName = currentLevel.nodeColors[y * currentLevel.width + x];
                            Color color = string.IsNullOrEmpty(colorName) || colorName == "X" ? Color.white : colorData.GetColor(colorName);
                            gridTextures[x, y] = CreateColorTexture(color);
                        }
                    }
                }

                Repaint(); // Pencerenin yeniden çizilmesini sağla
            }
            EditorGUILayout.EndHorizontal();

            // Bus Configuration Section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bus Configuration", EditorStyles.boldLabel);
            
            // Filter colors that can spawn characters
            var spawnableColors = colorData.colors
                .Where(c => c.spawnCharacter)
                .Select(c => c.colorName)
                .ToArray();
            
            bool busesChanged = false;
            List<BusData> previousBuses = new List<BusData>(busConfigurations);

            busScrollPosition = EditorGUILayout.BeginScrollView(busScrollPosition, GUILayout.Height(150));
            for (int i = 0; i < busConfigurations.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                string prevColor = busConfigurations[i].colorName;
                int prevSeats = busConfigurations[i].seatCount;
                int prevOrder = busConfigurations[i].order;
                
                // Only show spawnable colors in bus configuration
                int currentIndex = Array.IndexOf(spawnableColors, busConfigurations[i].colorName);
                if (currentIndex < 0) currentIndex = 0;
                
                currentIndex = EditorGUILayout.Popup(
                    currentIndex, 
                    spawnableColors);
                
                busConfigurations[i].colorName = spawnableColors[currentIndex];
                
                busConfigurations[i].seatCount = EditorGUILayout.IntField("Seats", busConfigurations[i].seatCount);
                
                busConfigurations[i].order = EditorGUILayout.IntField("Order", busConfigurations[i].order);
                
                if (prevColor != busConfigurations[i].colorName || 
                    prevSeats != busConfigurations[i].seatCount || 
                    prevOrder != busConfigurations[i].order)
                {
                    busesChanged = true;
                }
                
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    busConfigurations.RemoveAt(i);
                    busesChanged = true;
                    i--;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            selectedBusColorIndex = EditorGUILayout.Popup(selectedBusColorIndex, spawnableColors);
            newBusSeatCount = EditorGUILayout.IntField("Seats", newBusSeatCount);
            if (GUILayout.Button("Add Bus"))
            {
                busConfigurations.Add(new BusData {
                    colorName = spawnableColors[selectedBusColorIndex],
                    seatCount = newBusSeatCount,
                    order = busConfigurations.Count + 1
                });
                busesChanged = true;
            }
            EditorGUILayout.EndHorizontal();

            if (busesChanged && currentLevel != null)
            {
                currentLevel.buses = busConfigurations.ToArray();
                EditorUtility.SetDirty(this);
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
            waitingAreaSize = currentLevel.waitingAreaSize;
            
            gridTextures = new Texture2D[gridWidth, gridHeight];
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    string colorName = currentLevel.nodeColors[y * gridWidth + x];
                    Color color = string.IsNullOrEmpty(colorName) || colorName == "X" ? Color.white : colorData.GetColor(colorName);
                    gridTextures[x, y] = CreateColorTexture(color);
                }
            }
            
            busConfigurations = new List<BusData>(currentLevel.buses ?? new BusData[0]);
            
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
        currentLevel = new LevelData {
            width = Mathf.Clamp(gridWidth, 1, 20),
            height = Mathf.Clamp(gridHeight, 1, 20),
            nodeColors = new string[gridWidth * gridHeight],
            buses = busConfigurations.ToArray(),
            waitingAreaSize = waitingAreaSize
        };

        gridTextures = new Texture2D[currentLevel.width, currentLevel.height];
        for (int x = 0; x < currentLevel.width; x++)
        {
            for (int y = 0; y < currentLevel.height; y++)
            {
                gridTextures[x, y] = CreateColorTexture(Color.white);
                currentLevel.nodeColors[y * currentLevel.width + x] = "X"; // Initialize all cells with "X"
            }
        }
    }

    private void DrawGridEditor()
    {
        if (currentLevel == null || gridTextures == null) return;

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
                        // Only allow colors that can spawn characters
                        if (colorData.ShouldSpawnCharacter(colorName))
                        {
                            currentLevel.nodeColors[y * currentLevel.width + x] = colorName;
                            gridTextures[x, y] = CreateColorTexture(colorData.GetColor(colorName));
                        }
                    }
                    else if (evt.button == 1)
                    {
                        currentLevel.nodeColors[y * currentLevel.width + x] = "X";
                        gridTextures[x, y] = CreateColorTexture(Color.white);
                    }
                    GUI.changed = true;
                }

                if (gridTextures[x, y] != null)
                {
                    EditorGUI.DrawPreviewTexture(rect, gridTextures[x, y]);
                }

                string cellColor = currentLevel.nodeColors[y * currentLevel.width + x];
                if (string.IsNullOrEmpty(cellColor) )
                {
                    cellColor = "X";
                    currentLevel.nodeColors[y * currentLevel.width + x] = "X";
                }

                if (cellColor == "X" || !colorData.ShouldSpawnCharacter(cellColor))
                {
                    GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                    style.alignment = TextAnchor.MiddleCenter;
                    EditorGUI.LabelField(rect, "X", style);
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
            if (currentLevel != null)
            {
                currentLevel.buses = busConfigurations.ToArray();
                currentLevel.waitingAreaSize = waitingAreaSize;
            }
            
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
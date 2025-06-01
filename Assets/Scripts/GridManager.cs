using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GridManager : MonoBehaviourSingleton<GridManager>
{
    [Header("Grid Settings")]
    public float cellSize = 2f;
    
    [Header("Prefabs")]
    public GameObject gridNodePrefab;
    public GameObject characterPrefab;
    public ColorData colorData;
    
    [Header("Level Data")]
    public TextAsset defaultLevelData;
    
    private Dictionary<Vector2Int, GridNode> grid = new Dictionary<Vector2Int, GridNode>();
    private LevelData currentLevelData;
    [HideInInspector]public int width;
    [HideInInspector]public int height;

    private void Start()
    {
        if (defaultLevelData != null)
        {
            LoadLevelFromJson(defaultLevelData.text);
            // Initialize buses after loading level
            BusManager.Instance.InitializeBuses(currentLevelData.buses, colorData);
        }
        else
        {
            Debug.LogError("JSON Yükleyemedi");
        }
    }

    public void LoadLevelFromJson(string json)
    {
        currentLevelData = JsonUtility.FromJson<LevelData>(json);
        if (currentLevelData != null)
        {
            CreateGridFromLevelData(currentLevelData);
        }
        else
        {
            Debug.LogError("Failed to parse level data!");
        }
    }

    private void CreateGridFromLevelData(LevelData levelData)
    {
        ClearGrid();
        
        width = levelData.width;
        height = levelData.height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateGridNode(x, y, levelData);
            }
        }
    }

    private void CreateGridNode(int x, int y, LevelData levelData)
    {
        int flippedY = (height - 1) - y;
        Vector2Int gridPos = new Vector2Int(x, y);
        Vector3 worldPos = new Vector3(x * cellSize, 0, flippedY * cellSize);

        GameObject nodeObj = Instantiate(gridNodePrefab, worldPos, Quaternion.identity, transform);
        GridNode node = nodeObj.GetComponent<GridNode>();
        node.Position = gridPos;
        grid.Add(gridPos, node);

        string colorName = levelData.nodeColors[y * width + x];
    
        if (!string.IsNullOrEmpty(colorName) && colorData.ShouldSpawnCharacter(colorName))
        {
            node.SetColor(colorName, colorData);
            CreateCharacter(node, worldPos);
        }
        else
        {
            node.SetColor(colorName, colorData); 
            node.SetOccupied(false, null);
        }
    }

    private void CreateCharacter(GridNode node, Vector3 position)
    {
        GameObject charObj = Instantiate(
            characterPrefab, 
            position + Vector3.up * 0.5f, 
            Quaternion.identity,
            transform
        );
        
        Character character = charObj.GetComponent<Character>();
        character.Initialize(node, colorData);
        node.SetOccupied(true, character);
    }

    private void ClearGrid()
    {
        foreach (var node in grid.Values)
        {
            if (node != null && node.gameObject != null)
            {
                if (node.Occupant != null)
                {
                    Destroy(node.Occupant.gameObject);
                }
                Destroy(node.gameObject);
            }
        }
        grid.Clear();
    }

    public GridNode GetNode(Vector2Int pos)
    {
        grid.TryGetValue(pos, out var node);
        return node;
    }
    
}
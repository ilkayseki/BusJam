using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject gridCellPrefab;
    public int rows = 10;
    public int columns = 10;
    public float spacing = 1.2f;

    [Header("Color Settings")]
    public Color[] availableColors;

    [Header("Character")]
    public GameObject characterPrefab;

    [Header("Bus Settings")]
    public GameObject busPrefab;
    [SerializeField] private Transform busParent;
    public int busCount = 3;
    public float busSpacing = 2f; // Otobüsler arası boşluk

    private GridCell[,] grid;
    private List<GameObject> buses = new List<GameObject>();

    private List<Character> waitingAreaCharacters = new List<Character>();
    
    void Start()
    {
        GenerateGrid();
        SpawnBuses();
    }

    void GenerateGrid()
    {
        grid = new GridCell[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 pos = new Vector3(col * spacing, 0, row * spacing);
                GameObject cellObj = Instantiate(gridCellPrefab, pos, Quaternion.identity, transform);
                GridCell cell = cellObj.GetComponent<GridCell>();

                Color randomColor = availableColors[Random.Range(0, availableColors.Length)];
                cell.Initialize(new Vector2Int(row, col), randomColor);

                grid[row, col] = cell;

                CreateCharacterOnCell(cell);
            }
        }
    }

    void CreateCharacterOnCell(GridCell cell)
    {
        Vector3 spawnPos = cell.transform.position + Vector3.up * 0.5f;
        GameObject characterGO = Instantiate(characterPrefab, spawnPos, Quaternion.identity);
        characterGO.GetComponentInChildren<Renderer>().material.color = cell.cellColor;
        Character character = characterGO.GetComponent<Character>();
        character.Initialize(cell);
    }

    void SpawnBuses()
    {
        if (busPrefab == null || busParent == null)
        {
            Debug.LogError("BusPrefab veya BusParent atanmadı!");
            return;
        }

        buses.Clear();

        // İlk otobüs pozisyonu: busParent pozisyonu
        Vector3 basePos = busParent.position;

        // Otobüs prefabının genişliği alınmalı (localScale x veya collider)
        float busWidth = busPrefab.GetComponent<Renderer>().bounds.size.x;

        for (int i = 0; i < busCount; i++)
        {
            Vector3 spawnPos = basePos + new Vector3(-i * (busWidth + busSpacing), 0, 0);
            GameObject bus = Instantiate(busPrefab, spawnPos, Quaternion.identity, busParent);
            buses.Add(bus);

            // Otobüs renklerini rastgele veya sırayla ver (örnek):
            Color busColor = availableColors[i % availableColors.Length];
            SetBusColor(bus, busColor);
        }
    }

    void SetBusColor(GameObject bus, Color color)
    {
        Renderer rend = bus.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = color;
        }
    }

    // Bu fonksiyon, karakterin rengindeki otobüsün pozisyonunu döndürür.
    public Vector3 GetBusPositionForColor(Color characterColor)
    {
        foreach (var bus in buses)
        {
            Renderer rend = bus.GetComponent<Renderer>();
            if (rend != null && rend.material.color == characterColor)
            {
                return bus.transform.position;
            }
        }

        // Eğer otobüs yoksa örnek bir pozisyon döndür
        return busParent.position + new Vector3(0, 0, columns * spacing + 5);
    }

    public bool CanMoveToBus(GridCell cell)
    {
        Vector2Int pos = cell.gridPosition;

        // Z ekseninde ilerideki hücreler boş mu kontrolü
        for (int z = pos.y + 1; z < columns; z++)
        {
            if (grid[pos.x, z].isOccupied)
                return false;
        }
        return true;
    }

    public bool IsWaitingAreaFull()
    {
        return waitingAreaCharacters.Count >= 4;
    }

    public void AddCharacterToWaitingArea(Character character)
    {
        if (!waitingAreaCharacters.Contains(character))
            waitingAreaCharacters.Add(character);
    }

    public void RemoveCharacterFromWaitingArea(Character character)
    {
        if (waitingAreaCharacters.Contains(character))
            waitingAreaCharacters.Remove(character);
    }

    public void GameOver()
    {
        Debug.Log("Game Over! Bekleme alanı doldu.");
        // Buraya oyun bitti işlemleri gelecek
    }

    public void WinGame()
    {
        Debug.Log("Tebrikler! Tüm otobüsler bitti, oyun kazanıldı!");
        // Kazanma ekranı veya sonraki aşama
    }
}

using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviourSingleton<GridManager>
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private GameObject gridCellPrefab;
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private int rows = 5, columns = 5;
    [SerializeField] private float spacing = 1.5f;
    [SerializeField] private Color[] availableColors;
    [SerializeField] private WaitingArea waitingArea;

    private GridCell[,] grid;

    private void Awake() => Instance = this;

    private void Start() => GenerateGrid();

    private void GenerateGrid()
    {
        grid = new GridCell[rows, columns];

        for (int r = rows - 1; r >= 0; r--)  // Tersten başla, 0'a kadar düş
        {
            for (int c = 0; c < columns; c++)
            {
                Vector3 cellPos = new Vector3(c * spacing, 0, (rows - 1 - r) * spacing);
                GameObject cellObj = Instantiate(gridCellPrefab, cellPos, Quaternion.identity, transform);
                GridCell cell = cellObj.GetComponent<GridCell>();

                Color color = availableColors[Random.Range(0, availableColors.Length)];
                cell.Initialize(new Vector2Int(r, c), color);
                grid[r, c] = cell;

                SpawnCharacter(cell);
            }
        }
    }

    private void SpawnCharacter(GridCell cell)
    {
        Vector3 spawnPos = cell.transform.position + Vector3.up * 0.5f;
        GameObject charObj = Instantiate(characterPrefab, spawnPos, Quaternion.identity);
        Character character = charObj.GetComponent<Character>();
        character.Initialize(cell, waitingArea);
        cell.SetOccupied(true, character);
    }

    public void GameOver() => Debug.Log("Bekleme alanı doldu, oyun bitti!");
    public void CheckWinCondition()
    {
        if (BusManager.Instance.AllBusesFull()) Debug.Log("Tüm otobüsler doldu, oyunu kazandınız!");
    }
    public bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < rows && pos.y >= 0 && pos.y < columns;
    }

    public GridCell GetCellAtPosition(Vector2Int pos)
    {
        if (IsInsideGrid(pos))
            return grid[pos.x, pos.y];
        return null;
    }
    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) && pos.y >= 0 && pos.y < grid.GetLength(1);
    }

    public GridCell GetGridCell(Vector2Int pos)
    {
        if (IsValidPosition(pos))
            return grid[pos.x, pos.y];
        return null;
    }
    
    // Grid boyutları: rows = X, columns = Y

    public List<GridCell> FindPathToXZero(GridCell startCell)
    {
        int maxX = grid.GetLength(0);
        int maxY = grid.GetLength(1);

        Vector2Int startPos = startCell.GridPosition;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(startPos);
        cameFrom[startPos] = startPos;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // Eğer X=0 ise hedefe ulaştık
            if (current.x == 0)
            {
                // Yol geri izlenir
                List<GridCell> path = new List<GridCell>();
                Vector2Int temp = current;

                while (temp != startPos)
                {
                    path.Add(grid[temp.x, temp.y]);
                    temp = cameFrom[temp];
                }
                path.Reverse();
                return path;
            }

            // Komşuları kontrol et (sol, sağ, yukarı, aşağı)
            Vector2Int[] neighbors = new Vector2Int[]
            {
                new Vector2Int(current.x - 1, current.y), // sol (X - 1)
                new Vector2Int(current.x + 1, current.y), // sağ (X + 1)
                new Vector2Int(current.x, current.y - 1), // aşağı (Y - 1)
                new Vector2Int(current.x, current.y + 1)  // yukarı (Y + 1)
            };

            foreach (var neighbor in neighbors)
            {
                if (neighbor.x >= 0 && neighbor.x < maxX && neighbor.y >= 0 && neighbor.y < maxY)
                {
                    var neighborCell = grid[neighbor.x, neighbor.y];
                    // Boş olmayan hücreler engel (doluluk kontrolü)
                    if (!neighborCell.IsOccupied && !cameFrom.ContainsKey(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }
        }

        // Yol bulunamadı
        return null;
    }

}
using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviourSingleton<GridManager>
{
    public int width = 5;
    public int height = 5;
    public float cellSize = 2f;

    public GameObject gridNodePrefab;
    public GameObject characterPrefab;

    private Dictionary<Vector2Int, GridNode> grid = new Dictionary<Vector2Int, GridNode>();

    private void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // y'nin tersini alıyoruz
                int flippedY = (height - 1) - y;

                // Grid pozisyonu orijinal koordinat ile değil, ters y ile
                Vector2Int gridPos = new Vector2Int(x, y);

                // Fiziksel pozisyon ise z ekseninde ters çevrilmiş y yani flippedY kullanılarak oluşturuluyor
                Vector3 worldPos = new Vector3(x * cellSize, 0, flippedY * cellSize);

                GameObject nodeObj = Instantiate(gridNodePrefab, worldPos, Quaternion.identity);
                var node = nodeObj.GetComponent<GridNode>();
                node.Position = gridPos;  // Burada y değişmeden kalıyor, matris pozisyonu normal
                grid.Add(node.Position, node);

                // Renk ataması (örnek)
                node.SetColor((Random.value > 0.5f) ? BusColor.Red : BusColor.Blue);
                
                GameObject charObj = Instantiate(characterPrefab, worldPos + Vector3.up * 0.5f, Quaternion.identity);
                Character character = charObj.GetComponent<Character>();
                character.Init(node);
                node.SetOccupied(true, character);

            }
        }
    }
    
    public GridNode GetNode(Vector2Int pos)
    {
        grid.TryGetValue(pos, out var node);
        return node;
    }

}
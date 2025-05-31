using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public Color CellColor { get; private set; }
    public bool IsOccupied { get; private set; } = false;
    private Character characterInCell;

    public void Initialize(Vector2Int position, Color color)
    {
        GridPosition = position;
        CellColor = color;
        GetComponent<Renderer>().material.color = color;
    }

    public void SetOccupied(bool occupied, Character character = null)
    {
        IsOccupied = occupied;
        characterInCell = occupied ? character : null;
    }

    public Character GetCharacter() => characterInCell;

    // Yardımcı method: Boş mu dolu mu kontrolü
    public bool IsEmpty() => !IsOccupied;
}
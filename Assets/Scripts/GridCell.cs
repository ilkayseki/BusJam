using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public Color cellColor;
    public bool isOccupied = false;

    public void Initialize(Vector2Int position, Color color)
    {
        gridPosition = position;
        cellColor = color;
        GetComponent<Renderer>().material.color = color;
    }
}
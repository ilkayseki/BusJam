using UnityEngine;

public class GridNode : MonoBehaviour
{
    public Vector2Int Position;
    public bool IsOccupied;
    public Character Occupant;
    public string NodeColor;
    
    [SerializeField] private ColorData colorData;
    Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void SetOccupied(bool occupied, Character character)
    {
        IsOccupied = occupied;
        Occupant = character;
        Debug.Log($"Grid {Position} is now {(occupied ? "Occupied" : "Empty")}");
    }

    public void SetColor(string color, ColorData data = null)
    {
        if (data != null) colorData = data;
        NodeColor = color;
        if (rend == null) rend = GetComponent<Renderer>();
        rend.material.color = colorData.GetColor(NodeColor);
    }
    
}
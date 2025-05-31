using UnityEngine;
[RequireComponent(typeof(BoxCollider))]

public class GridNode : MonoBehaviour
{
    public bool IsOccupied;
    public Character Occupant;
    public Vector2Int Position;
    public string NodeColor;
    
    [SerializeField] private ColorData colorData;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        GetComponent<BoxCollider>().size = new Vector3(1, 0.1f, 1);
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
        if (colorData != null) rend.material.color = colorData.GetColor(NodeColor);
    }
    
}
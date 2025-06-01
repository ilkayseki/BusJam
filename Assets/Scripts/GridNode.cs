using UnityEngine;

public class GridNode : MonoBehaviour
{
    public Vector2Int Position;
    public bool IsOccupied;
    public Character Occupant;
    public string NodeColor;
    
    private Renderer rend;
    private ColorData colorData;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null) rend = gameObject.AddComponent<Renderer>();
    }

    public void SetOccupied(bool occupied, Character character)
    {
        IsOccupied = occupied;
        Occupant = character;
    }

    public void SetColor(string color, ColorData data)
    {
        colorData = data;
        NodeColor = color;
        UpdateColorVisual();
    }

    private void UpdateColorVisual()
    {
        if (rend == null) return;
        
        if (string.IsNullOrEmpty(NodeColor))
        {
            rend.material.color = Color.white;
        }
        else
        {
            rend.material.color = colorData.GetColor(NodeColor);
        }
    }

    private void OnDrawGizmos()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        Gizmos.color = IsOccupied ? Color.red : Color.green;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.9f);
    }
}
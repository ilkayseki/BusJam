using UnityEngine;

public class GridNode : MonoBehaviour
{
    public Vector2Int Position;
    public bool IsOccupied;
    public Character Occupant;
    public BusColor NodeColor;

    Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void SetOccupied(bool occupied, Character character)
    {
        IsOccupied = occupied;
        Occupant = character;
        Debug.Log($"Grid {Position} durumu: {(occupied ? "Dolu" : "Boş")}");
    }

    public void SetColor(BusColor color)
    {
        NodeColor = color;
        if (rend == null) rend = GetComponent<Renderer>();

        if (NodeColor == BusColor.Red)
            rend.material.color = Color.red;
        else
            rend.material.color = Color.blue;
    }

    private void OnDrawGizmos()
    {
        if (rend == null)
            rend = GetComponent<Renderer>();

        if (rend != null)
        {
            Gizmos.color = IsOccupied ? Color.red : Color.green;
            Gizmos.DrawCube(transform.position, Vector3.one * 0.9f);
        }
    }
}
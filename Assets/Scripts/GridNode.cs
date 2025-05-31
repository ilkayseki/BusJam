using UnityEngine;

public class GridNode : MonoBehaviour
{
    public Vector2Int Position;
    public bool IsOccupied;
    public Character Occupant;

    public void SetOccupied(bool occupied, Character character)
    {
        IsOccupied = occupied;
        Occupant = character;
        Debug.Log($"Grid {Position} durumu: {(occupied ? "Dolu" : "Boş")}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsOccupied ? Color.red : Color.green;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.9f);
    }
}
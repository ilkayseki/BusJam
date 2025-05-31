using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private GridNode currentNode;
    private Renderer rend;
    public string CharacterColor;
    
    [SerializeField] private ColorData colorData;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
    }

    public void Init(GridNode node, ColorData data)
    {
        colorData = data;
        currentNode = node;
        SetColor(node.NodeColor);
    }

    public void SetColor(string color)
    {
        CharacterColor = color;
        if (rend == null) rend = GetComponent<Renderer>();
        rend.material.color = colorData.GetColor(color);
    }

    private bool CanReachYZero()
    {
        Queue<GridNode> queue = new Queue<GridNode>();
        HashSet<GridNode> visited = new HashSet<GridNode>();

        queue.Enqueue(currentNode);
        visited.Add(currentNode);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node.Position.y == 0) return true;

            foreach (var dir in directions)
            {
                var neighborPos = node.Position + dir;
                var neighbor = GridManager.Instance.GetNode(neighborPos);

                if (neighbor != null && !neighbor.IsOccupied && !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }

        return false;
    }

    private void OnMouseDown()
    {
        var bus = BusManager.Instance.GetActiveBus();

        if (bus == null)
        {
            Debug.Log("Aktif bus yok.");
            return;
        }

        if (!CanReachYZero())
        {
            Debug.Log("Karakter y=0 satırına erişemiyor, işlem iptal.");
            return;
        }

        if (bus.BusColor == CharacterColor)
        {
            Debug.Log("Renkler aynı, karakter yok ediliyor.");
            currentNode.SetOccupied(false, null);
            Destroy(gameObject);
            bus.OccupySeat();
        }
        else
        {
            Debug.Log("Renk farklı, karakter silinmedi.");
        }
    }
}
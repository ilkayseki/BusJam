using UnityEngine;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
    private GridNode currentNode;
    private float cellSize => GridManager.Instance.cellSize;

    public void Init(GridNode node)
    {
        currentNode = node;
    }

    private void OnMouseDown()
    {
        Debug.Log($"Karakter {currentNode.Position} tıklandı.");

        if (currentNode.Position.y == 0)
        {
            Debug.Log("X=0 Ulaşıldı, Karakter siliniyor");
            currentNode.SetOccupied(false, null);
            Destroy(gameObject);
        }
        else
        {
            if (CanReachYZero())
            {
                Debug.Log("Yol var, kendi grid boşaltılıyor");
                currentNode.SetOccupied(false, null);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Yol Yok");
            }
        }
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
}
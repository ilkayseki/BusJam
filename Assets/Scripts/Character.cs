using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Character : MonoBehaviour
{
    private GridNode currentNode;
    private Renderer rend;
    public string CharacterColor;
    private ColorData colorData;
    private bool isMoving = false;
    private Bus bus;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        if (rend == null) rend = GetComponent<Renderer>();
    }

    public void Initialize(GridNode node, ColorData data)
    {
        currentNode = node;
        colorData = data;
        CharacterColor = node.NodeColor;
        UpdateColorVisual();
    }

    private void UpdateColorVisual()
    {
        if (rend == null) return;
        rend.material.color = colorData.GetColor(CharacterColor);
    }

    private void OnMouseDown()
    {
        if (isMoving) return;
        if (!colorData.ShouldSpawnCharacter(CharacterColor)) return;

        bus = BusManager.Instance.GetActiveBus();
        if (bus == null || bus.BusColor != CharacterColor) return;

        if (currentNode.Position.y == 0)
        {
            DestroyCharacter();
            return;
        }

        MoveToNearestYZero();
    }

    private void MoveToNearestYZero()
    {
        List<GridNode> path = FindPathToNearestYZero();
        if (path == null || path.Count == 0) return;

        isMoving = true;
        Sequence movementSequence = DOTween.Sequence();

        foreach (var node in path)
        {
            Vector3 targetPos = GetWorldPosition(node.Position);
            movementSequence.Append(transform.DOMove(targetPos, 0.3f).SetEase(Ease.Linear));
        }

        movementSequence.OnComplete(() => {
            currentNode.SetOccupied(false, null);
            DestroyCharacter();
        });
    }

    private List<GridNode> FindPathToNearestYZero()
    {
        if (currentNode.Position.y == 0) return new List<GridNode>();

        Queue<List<GridNode>> pathQueue = new Queue<List<GridNode>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        pathQueue.Enqueue(new List<GridNode> { currentNode });
        visited.Add(currentNode.Position);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (pathQueue.Count > 0)
        {
            List<GridNode> currentPath = pathQueue.Dequeue();
            GridNode lastNode = currentPath.Last();

            foreach (var dir in directions)
            {
                Vector2Int neighborPos = lastNode.Position + dir;
                if (visited.Contains(neighborPos)) continue;

                GridNode neighbor = GridManager.Instance.GetNode(neighborPos);
                if (neighbor == null || neighbor.IsOccupied) continue;

                List<GridNode> newPath = new List<GridNode>(currentPath) { neighbor };
                
                if (neighborPos.y == 0)
                {
                    newPath.RemoveAt(0);
                    return newPath;
                }

                pathQueue.Enqueue(newPath);
                visited.Add(neighborPos);
            }
        }
        return null;
    }

    private Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        int flippedY = (GridManager.Instance.height - 1) - gridPos.y;
        return new Vector3(
            gridPos.x * GridManager.Instance.cellSize, 
            0.5f,
            flippedY * GridManager.Instance.cellSize
        );
    }

    private void DestroyCharacter()
    {
        if (currentNode != null) currentNode.SetOccupied(false, null);
        if (bus != null) bus.OccupySeat();
        Destroy(gameObject);
    }
}
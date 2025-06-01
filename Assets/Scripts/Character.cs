using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class Character : MonoBehaviour
{
    private GridNode currentNode;
    private Renderer rend;
    public string CharacterColor;
    
    [SerializeField] private ColorData colorData;
    private bool isMoving = false;
    private Bus bus;

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

    private void OnMouseDown()
    {
        if (isMoving) return;

        bus = BusManager.Instance.GetActiveBus();
        if (bus == null || bus.BusColor != CharacterColor) return;

        // If already at y=0, destroy immediately
        if (currentNode.Position.y == 0)
        {
            DestroyCharacter();
            return;
        }

        // Find and move to nearest y=0 node
        MoveToNearestYZero();
    }

    private void MoveToNearestYZero()
    {
        isMoving = true;
        List<GridNode> path = FindShortestPathToYZero();

        if (path == null || path.Count == 0)
        {
            Debug.Log("No valid path to y=0 found");
            isMoving = false;
            return;
        }

        Sequence movementSequence = DOTween.Sequence();
        
        foreach (var node in path)
        {
            Vector3 targetPos = GetWorldPosition(node.Position);
            movementSequence.Append(transform.DOMove(targetPos, 0.3f).SetEase(Ease.Linear));
        }

        movementSequence.OnComplete(() => {
            currentNode.SetOccupied(false, null);
            bus.OccupySeat();
            DestroyCharacter();
        });
    }

    private List<GridNode> FindShortestPathToYZero()
    {
        // If already at y=0, return empty path
        if (currentNode.Position.y == 0) return new List<GridNode>();

        Queue<List<GridNode>> pathsQueue = new Queue<List<GridNode>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // Start with current node
        pathsQueue.Enqueue(new List<GridNode> { currentNode });
        visited.Add(currentNode.Position);

        Vector2Int[] directions = { 
            Vector2Int.up, 
            Vector2Int.down, 
            Vector2Int.left, 
            Vector2Int.right 
        };

        while (pathsQueue.Count > 0)
        {
            List<GridNode> currentPath = pathsQueue.Dequeue();
            GridNode lastNode = currentPath.Last();

            // Check all neighbors
            foreach (var dir in directions)
            {
                Vector2Int neighborPos = lastNode.Position + dir;
                
                // Skip if already visited
                if (visited.Contains(neighborPos)) continue;

                GridNode neighbor = GridManager.Instance.GetNode(neighborPos);
                if (neighbor == null || neighbor.IsOccupied) continue;

                // Create new path with this neighbor
                List<GridNode> newPath = new List<GridNode>(currentPath) { neighbor };
                
                // If reached y=0, return the path
                if (neighborPos.y == 0)
                {
                    // Remove starting node (we're already there)
                    newPath.RemoveAt(0);
                    return newPath;
                }

                // Enqueue the new path
                pathsQueue.Enqueue(newPath);
                visited.Add(neighborPos);
            }
        }

        return null; // No path found
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
        currentNode.SetOccupied(false, null);
        bus.OccupySeat();
        Destroy(gameObject);
    }
}
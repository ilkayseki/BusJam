using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

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

    private List<GridNode> FindPathToYZero()
    {
        Dictionary<GridNode, GridNode> cameFrom = new Dictionary<GridNode, GridNode>();
        Queue<GridNode> queue = new Queue<GridNode>();
        HashSet<GridNode> visited = new HashSet<GridNode>();

        queue.Enqueue(currentNode);
        visited.Add(currentNode);
        cameFrom[currentNode] = null;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node.Position.y == 0)
            {
                // Reconstruct path
                List<GridNode> path = new List<GridNode>();
                GridNode current = node;
                while (current != null)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Reverse();
                return path;
            }

            foreach (var dir in directions)
            {
                var neighborPos = node.Position + dir;
                var neighbor = GridManager.Instance.GetNode(neighborPos);

                if (neighbor != null && !neighbor.IsOccupied && !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = node;
                }
            }
        }

        return null;
    }

    private void OnMouseDown()
    {
        if (isMoving) return;

        bus = BusManager.Instance.GetActiveBus();

        if (bus == null)
        {
            Debug.Log("No active bus.");
            return;
        }

        if (!CanReachYZero())
        {
            Debug.Log("Character cannot reach y=0.");
            return;
        }

        if (bus.BusColor == CharacterColor)
        {
            // Immediate response
            Debug.Log("Colors match, character will be destroyed.");
            currentNode.SetOccupied(false, null); // Immediately free the node
            StartMovement();
        }
        else
        {
            Debug.Log("Colors don't match.");
        }
    }

    private void StartMovement()
    {
        isMoving = true;
        
        List<GridNode> path = FindPathToYZero();
        if (path == null || path.Count == 0)
        {
            DestroyCharacter();
            return;
        }

        // Create and start the movement sequence immediately
        Sequence movementSequence = DOTween.Sequence();
        
        foreach (var node in path)
        {
            int flippedY = (GridManager.Instance.height - 1) - node.Position.y;
            Vector3 targetPos = new Vector3(
                node.Position.x * GridManager.Instance.cellSize, 
                0.5f,
                flippedY * GridManager.Instance.cellSize
            );
            
            movementSequence.Append(transform.DOMove(targetPos, 0.3f).SetEase(Ease.Linear));
        }

        // On complete callback instead of coroutine
        movementSequence.OnComplete(() => {
            bus.OccupySeat();
            DestroyCharacter();
        });
    }

    private void DestroyCharacter()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
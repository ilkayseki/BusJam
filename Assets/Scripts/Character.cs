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
        if (isMoving) return;

        bus = BusManager.Instance.GetActiveBus();

        if (bus == null)
        {
            Debug.Log("No active bus.");
            return;
        }

        if (bus.BusColor == CharacterColor)
        {
            if (CanReachYZero())
            {
                Debug.Log("Colors match, moving to bus.");
                MoveToBus();
            }
        }
        else
        {
            Debug.Log("Colors don't match, moving to waiting area.");
            MoveToWaitingArea();
        }
    }

   
    private void MoveToWaitingArea()
    {
        isMoving = true;
        List<GridNode> path = FindPathToPosition(new Vector2Int(currentNode.Position.x, 0));
        
        if (path == null || path.Count == 0)
        {
            isMoving = false;
            return;
        }

        int? slotIndex = WaitingArea.Instance.GetAvailableSlot();
        if (slotIndex == null)
        {
            Debug.Log("No available slots in waiting area.");
            isMoving = false;
            return;
        }

        Sequence movementSequence = DOTween.Sequence();
        
        // Move to y=0 first
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

        // Then move to waiting area slot
        Vector3 slotPosition = WaitingArea.Instance.GetSlotPosition(slotIndex.Value);
        movementSequence.Append(transform.DOMove(slotPosition, 0.5f));

        movementSequence.OnComplete(() => {
            currentNode.SetOccupied(false, null);
            currentNode = null;
            WaitingArea.Instance.OccupySlot(slotIndex.Value, this);
            isMoving = false;
        });
    }
    
    private void MoveToBus()
    {
        isMoving = true;
    
        // Y=0 hattındaki tüm boş node'ları bul
        List<GridNode> availableTargets = new List<GridNode>();
        for (int x = 0; x < GridManager.Instance.width; x++)
        {
            Vector2Int targetPos = new Vector2Int(x, 0);
            GridNode node = GridManager.Instance.GetNode(targetPos);
            if (node != null && !node.IsOccupied)
            {
                availableTargets.Add(node);
            }
        }

        if (availableTargets.Count == 0)
        {
            Debug.LogError("No available nodes at y=0 line!");
            isMoving = false;
            return;
        }

        // Tüm olası hedefler için yol bulmaya çalış
        foreach (GridNode targetNode in availableTargets)
        {
            List<GridNode> path = FindPathToPosition(targetNode.Position);
        
            if (path != null && path.Count > 0)
            {
                // Yol bulundu, hareketi başlat
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

                movementSequence.OnComplete(() => {
                    currentNode.SetOccupied(false, null);
                    bus.OccupySeat();
                    Destroy(gameObject);
                });
            
                return; // İlk bulunan geçerli yolda hareketi başlat ve çık
            }
        }

        Debug.LogError("No valid path to any y=0 node!");
        isMoving = false;
    }

private List<GridNode> FindPathToPosition(Vector2Int targetPosition)
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
        
        // HEDEF KONTROLÜ DOĞRU YAPILDIĞINDAN EMİN OL
        if (node.Position == targetPosition)
        {
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
                cameFrom[neighbor] = node; // KOMŞUYU MEVCUT NODE'A BAĞLA
            }
        }
    }

    return null;
}

private void DebugNeighbors(GridNode node)
{
    Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    
    foreach (var dir in directions)
    {
        var neighborPos = node.Position + dir;
        var neighbor = GridManager.Instance.GetNode(neighborPos);
        
        if (neighbor != null)
        {
            Debug.Log($"Neighbor at {neighborPos}: " +
                     $"Occupied={neighbor.IsOccupied}, " +
                     $"Color={neighbor.NodeColor}");
        }
    }
}
}
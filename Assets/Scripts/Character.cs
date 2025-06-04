using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Character : MonoBehaviour
{
    private GridNode currentNode;
    private Renderer rend;
    public string CharacterColor { get; private set; }
    private ColorData colorData;
    private bool isMoving = false;
    private Bus bus;
    private Sequence movementSequence;
    private CharacterAnimator characterAnimator;

    // Yeni değişken ekleyelim
    private bool isInWaitingArea = false;
    
    // Waiting Area'da olup olmadığını kontrol eden property
    public bool IsInWaitingArea => isInWaitingArea;
    
    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        characterAnimator = gameObject.AddComponent<CharacterAnimator>();

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
        if (InputManager.Instance.IsInputBlocked || 
            isMoving || 
            isInWaitingArea ||
            !colorData.ShouldSpawnCharacter(CharacterColor)) 
            return;

        bus = BusManager.Instance.GetActiveBus();
        if (bus == null) return;

       
        if (Vector3.Distance(bus.transform.position, BusManager.Instance.stopPosition.transform.position) > 0.1f)
        {
            return; 
        }

        InputManager.Instance.BlockInput(true);
    
        if (currentNode.Position.y == 0)
        {
            HandleYZeroPosition();
            InputManager.Instance.BlockInput(false);
            return;
        }

        StartMovement();
    }
    

    private void StartMovement()
    {
        List<GridNode> path = FindPathToNearestYZero();
        if (path == null || path.Count == 0)
        {
            InputManager.Instance.BlockInput(false);
            return;
        }

        isMoving = true;
        characterAnimator.SetRunning(true); // Hareket başladığında running animasyonu
        
        movementSequence = DOTween.Sequence();

        foreach (var node in path)
        {
            Vector3 targetPos = GetWorldPosition(node.Position);
            movementSequence.Append(transform.DOMove(targetPos, 0.3f).SetEase(Ease.Linear));
        }

        movementSequence.OnComplete(() => {
            currentNode.SetOccupied(false, null);
            characterAnimator.SetRunning(false); // Hareket bittiğinde idle animasyonu
            HandleYZeroPosition();
            isMoving = false;
            InputManager.Instance.BlockInput(false);
        });
    }

    private void HandleYZeroPosition()
    {
        if (bus.BusColor == CharacterColor)
        {
            // Renk uyumu varsa direkt yok ol
            DestroyCharacter();
        }
        else
        {
            // Renk uyumu yoksa bekleme alanına git
            MoveToWaitingArea();
        }
    }
    private void MoveToWaitingArea()
    {
        int? slotIndex = WaitingArea.Instance.GetAvailableSlot();
        if (!slotIndex.HasValue)
        {
            isMoving = false;
            Destroy(gameObject);
            return;
        }

        isInWaitingArea = true;
        
        Vector3 slotPosition = WaitingArea.Instance.GetSlotPosition(slotIndex.Value);
    
        isMoving = true;
        characterAnimator.SetRunning(true);
        currentNode.SetOccupied(false, null);
        movementSequence = DOTween.Sequence();
        movementSequence.Append(transform.DOMove(slotPosition, 0.5f).SetEase(Ease.InOutQuad));
        movementSequence.OnComplete(() => {
            WaitingArea.Instance.OccupySlot(slotIndex.Value, this);
            characterAnimator.SetRunning(false);
            characterAnimator.ResetRotation();
            isMoving = false;
        });
    }

    private void DestroyCharacter()
    {
        if (currentNode != null) currentNode.SetOccupied(false, null);
        if (bus != null) bus.OccupySeat();
        characterAnimator.ResetRotation(); // Otobüse bindiğinde yönü resetle (isteğe bağlı)
        Destroy(gameObject);
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

    

    private void OnDestroy()
    {
        if (movementSequence != null && movementSequence.IsActive())
        {
            movementSequence.Kill();
            InputManager.Instance.BlockInput(false);
        }
    }
}
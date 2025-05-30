using UnityEngine;

public enum CharacterState
{
    Idle,
    MovingToBus,
    WaitingInQueue,
    Boarded,
    Blocked
}

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour
{
    public GridCell currentCell;
    public Color characterColor;
    public CharacterState state = CharacterState.Idle;

    private GridManager gridManager;
    private BusManager busManager;
    private WaitingArea waitingArea;

    private CharacterController controller;
    private Vector3 targetPosition;
    public float moveSpeed = 3f;

    public void Initialize(GridCell cell)
    {
        currentCell = cell;
        characterColor = cell.cellColor;
        GetComponentInChildren<Renderer>().material.color = characterColor;
        gridManager = FindObjectOfType<GridManager>();
        busManager = FindObjectOfType<BusManager>();
        waitingArea = FindObjectOfType<WaitingArea>();

        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (state == CharacterState.MovingToBus)
        {
            MoveTowardsTarget();
        }
    }

    void OnMouseDown()
    {
        HandleClick();
    }

    void HandleClick()
    {
        if (state == CharacterState.Idle)
        {
            if (gridManager.CanMoveToBus(currentCell))
            {
                state = CharacterState.MovingToBus;
                targetPosition = gridManager.GetBusPositionForColor(characterColor);
            }
            else
            {
                state = CharacterState.Blocked;
                Debug.Log("Önünde dolu grid var.");
            }
        }
        else if (state == CharacterState.MovingToBus)
        {
            // Eğer hedefe ulaştıysan:
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                if (busManager.TryBoardCharacter(this))
                {
                    state = CharacterState.Boarded;
                    Destroy(gameObject);
                }
                else
                {
                    if (!waitingArea.IsFull())
                    {
                        waitingArea.AddToWaitingArea(this);
                        state = CharacterState.WaitingInQueue;
                        Debug.Log("Bekleme alanına yönlendirildi.");
                    }
                    else
                    {
                        gridManager.GameOver();
                    }
                }
            }
        }
        else if (state == CharacterState.WaitingInQueue)
        {
            if (waitingArea.IsFull())
            {
                gridManager.GameOver();
            }
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 move = direction * moveSpeed * Time.deltaTime;

        controller.Move(move);

        // Hedefe çok yakınsak doğrudan hedef konuma ayarla (titremesin)
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            transform.position = targetPosition;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterState { Idle, MovingToBus, WaitingInQueue, Boarded, Blocked }

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour
{
    public Color CharacterColor { get; private set; }
    public CharacterState State { get; private set; } = CharacterState.Idle;
    public GridCell HomeCell { get; private set; }

    private CharacterController controller;
    private Vector3 targetPosition;
    private float moveSpeed = 3f;
    private WaitingArea waitingArea;

    public void Initialize(GridCell cell, WaitingArea waiting)
    {
        HomeCell = cell;
        CharacterColor = cell.CellColor;
        GetComponentInChildren<Renderer>().material.color = CharacterColor;
        controller = GetComponent<CharacterController>();

        waitingArea = waiting;
    }

    private void Update()
    {
        if (State == CharacterState.MovingToBus)
            MoveTowardsTarget();
    }

    private void OnMouseDown()
    {
        if (State == CharacterState.Idle)
        {
            if (HomeCell.GridPosition.x == 0)
            {
                Debug.Log("Ulaşıldı!");
                HomeCell.SetOccupied(false, null);
                // Karakter yok olabilir veya başka işlem
                Destroy(gameObject);
                State = CharacterState.Idle;
                return;
            }

            List<GridCell> path = GridManager.Instance.FindPathToXZero(HomeCell);

            if (path == null || path.Count == 0)
            {
                Debug.Log("Yol yok!");
                return;
            }

            // Yol var, karakteri sırayla bu hücrelere götür
            StartCoroutine(MoveAlongPath(path));
        }
    }

    private IEnumerator MoveAlongPath(List<GridCell> path)
    {
        State = CharacterState.MovingToBus; // hareket halinde

        foreach (var cell in path)
        {
            Vector3 targetPos = cell.transform.position + Vector3.up * 0.5f;

            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                Vector3 dir = (targetPos - transform.position).normalized;
                controller.Move(dir * moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Önceki hücreyi boşalt, yeni hücreyi dolu yap
            HomeCell.SetOccupied(false, null);
            cell.SetOccupied(true, this);
            HomeCell = cell;
        }

        // X=0'a ulaştık
        Debug.Log("Ulaşıldı!");
        HomeCell.SetOccupied(false, null);
        State = CharacterState.Idle;
    }



    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        State = CharacterState.MovingToBus;
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        controller.Move(direction * moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            State = CharacterState.Boarded;
            Debug.Log("Karakter otobüse bindi.");
            GridManager.Instance.CheckWinCondition();
        }
    }
}

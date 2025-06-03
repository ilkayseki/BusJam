using UnityEngine;
using DG.Tweening;

public class Bus : MonoBehaviour
{
    public string BusColor { get; private set; }
    public int SeatCount { get; private set; }
    private int occupiedSeats = 0;
    private Renderer rend;
    private Sequence movementSequence;

    public void Initialize(string color, int seatCount, ColorData colorData)
    {
        BusColor = color;
        SeatCount = seatCount;
        
        rend = GetComponent<Renderer>();
        rend.material.color = colorData.GetColor(color);
    }

    public void MoveToStop(Vector3 stopPosition, float duration, System.Action onComplete)
    {
        InputManager.Instance.BlockInput(true);
        movementSequence = DOTween.Sequence();
        movementSequence.Append(transform.DOMove(stopPosition, duration).SetEase(Ease.OutQuad));
        movementSequence.OnComplete(() => {
            onComplete?.Invoke();
            InputManager.Instance.BlockInput(false);
        });
    }

    public void MoveToFinish(Vector3 finishPosition, float duration, System.Action onComplete)
    {
        InputManager.Instance.BlockInput(true);
        movementSequence = DOTween.Sequence();
        movementSequence.Append(transform.DOMove(finishPosition, duration).SetEase(Ease.InQuad));
        movementSequence.OnComplete(() => {
            onComplete?.Invoke();
            Destroy(gameObject); // Finish'e varınca otobüsü yok et
        });
    }

    public void OccupySeat()
    {
        occupiedSeats++;
        if (occupiedSeats >= SeatCount)
        {
            BusManager.Instance.BusFilled(this);
        }
    }

    private void OnDestroy()
    {
        if (movementSequence != null && movementSequence.IsActive())
        {
            movementSequence.Kill();
        }
    }
}
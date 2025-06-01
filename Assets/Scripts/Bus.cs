using UnityEngine;

public class Bus : MonoBehaviour
{
    public string BusColor { get; private set; }
    public int SeatCount { get; private set; }
    private int occupiedSeats = 0;
    private Renderer rend;

    public void Initialize(string color, int seatCount, ColorData colorData)
    {
        BusColor = color;
        SeatCount = seatCount;
        
        rend = GetComponent<Renderer>();
        rend.material.color = colorData.GetColor(color);
    }

    public void OccupySeat()
    {
        occupiedSeats++;
        if (occupiedSeats >= SeatCount)
        {
            BusManager.Instance.ActivateNextBus();
        }
    }
}
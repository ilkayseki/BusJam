using UnityEngine;
public enum BusColor { Red, Blue }
public class Bus : MonoBehaviour
{
    public BusColor BusColor;
    public int seatCount = 2;
    private int occupiedSeats = 0;

    Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void Init(BusColor color)
    {
        BusColor = color;
        SetColorVisual();
    }

    void SetColorVisual()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        rend.material.color = (BusColor == BusColor.Red) ? Color.red : Color.blue;
    }

    public void OccupySeat()
    {
        occupiedSeats++;
        if (occupiedSeats >= seatCount)
        {
            BusManager.Instance.BusFull();
        }
    }
}

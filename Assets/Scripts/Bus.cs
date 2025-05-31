using UnityEngine;

public class Bus : MonoBehaviour
{
    public string BusColor;
    public int seatCount = 2;
    private int occupiedSeats = 0;
    
    [SerializeField] private ColorData colorData;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void Init(string color, ColorData data)
    {
        colorData = data;
        BusColor = color;
        SetColorVisual();
    }

    void SetColorVisual()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        rend.material.color = colorData.GetColor(BusColor);
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
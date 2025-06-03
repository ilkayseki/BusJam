[System.Serializable]
public class LevelData
{
    public int width;
    public int height;
    public string[] nodeColors;
    public BusData[] buses;
    public int waitingAreaSize = 3; // Varsayılan değer 3
}

[System.Serializable]
public class BusData
{
    public string colorName;
    public int seatCount = 2;
    public int order;
}

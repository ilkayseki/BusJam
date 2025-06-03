[System.Serializable]
public class LevelData
{
    public int width;
    public int height;
    public string[] nodeColors;
    public BusData[] buses;
    public int waitingAreaSize = 3;
    public float levelTime = 60f; // Varsayılan 60 saniye
}

[System.Serializable]
public class BusData
{
    public string colorName;
    public int seatCount = 2;
    public int order;
}

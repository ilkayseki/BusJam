[System.Serializable]
public class LevelData
{
    public int width;
    public int height;
    public string[] nodeColors;
    public BusData[] buses;
}

[System.Serializable]
public class BusData
{
    public string colorName;
    public int seatCount = 2;
    public int order;
}

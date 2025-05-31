using UnityEngine;

[CreateAssetMenu(fileName = "ColorData", menuName = "Game/Color Data")]
public class ColorData : ScriptableObject
{
    [System.Serializable]
    public class ColorInfo
    {
        public string colorName;
        public Color colorValue;
    }

    public ColorInfo[] colors;

    public Color GetColor(string name)
    {
        foreach (var colorInfo in colors)
        {
            if (colorInfo.colorName == name)
            {
                return colorInfo.colorValue;
            }
        }
        Debug.LogWarning($"Color {name} not found, returning white");
        return Color.white;
    }

    public string GetRandomColorName()
    {
        if (colors.Length == 0) return "";
        return colors[Random.Range(0, colors.Length)].colorName;
    }
}
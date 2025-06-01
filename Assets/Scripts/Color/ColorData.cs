using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorData", menuName = "Game/Color Data")]
public class ColorData : ScriptableObject
{
    [System.Serializable]
    public class ColorInfo
    {
        public string colorName;
        public Color colorValue;
        public bool spawnCharacter = true;
    }

    public ColorInfo[] colors;

    public Color GetColor(string name)
    {
        if (colors == null) return Color.white;
        
        foreach (var colorInfo in colors)
        {
            if (colorInfo.colorName == name)
            {
                return colorInfo.colorValue;
            }
        }
        return Color.white;
    }

    public bool ShouldSpawnCharacter(string colorName)
    {
        if (string.IsNullOrEmpty(colorName)) return false;
        if (colors == null) return false;
        
        foreach (var colorInfo in colors)
        {
            if (colorInfo.colorName == colorName)
            {
                return colorInfo.spawnCharacter;
            }
        }
        return false;
    }

    public string[] GetColorNames()
    {
        if (colors == null) return new string[0];
        return colors.Select(c => c.colorName).ToArray();
    }
}
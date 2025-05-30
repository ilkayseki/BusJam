using UnityEngine;
using System.Collections.Generic;

public class BusManager : MonoBehaviour
{
    [System.Serializable]
    public class BusData
    {
        public Color color;
    }

    public GameObject busPrefab;
    public List<BusData> busesData;
    public int seatsPerBus = 4;
    public Transform busParent; // İlk otobüs için konum
    public float spacing = 1f;  // Otobüsler arası boşluk

    private Dictionary<Color, int> currentBusSeats = new Dictionary<Color, int>();
    private Dictionary<Color, Vector3> busPositions = new Dictionary<Color, Vector3>();

    void Start()
    {
        if (busPrefab == null || busesData == null || busesData.Count == 0)
        {
            Debug.LogError("Bus prefab veya bus verileri eksik!");
            return;
        }

        float busWidth = GetPrefabWidth(busPrefab);
        Vector3 startPosition = busParent.position;

        for (int i = 0; i < busesData.Count; i++)
        {
            var busData = busesData[i];
            Vector3 busPos = startPosition + Vector3.left * i * (busWidth + spacing);

            GameObject busObj = Instantiate(busPrefab, busPos, Quaternion.identity, busParent);
            Renderer busRenderer = busObj.GetComponent<Renderer>();
            if (busRenderer != null)
            {
                busRenderer.material.color = busData.color;
            }

            currentBusSeats[busData.color] = seatsPerBus;
            busPositions[busData.color] = busPos;
        }
    }

    public Vector3 GetBusPositionForColor(Color color)
    {
        if (busPositions.ContainsKey(color))
            return busPositions[color];
        return Vector3.zero;
    }

    public bool TryBoardCharacter(Character character)
    {
        Color color = character.characterColor;
        if (currentBusSeats.ContainsKey(color) && currentBusSeats[color] > 0)
        {
            currentBusSeats[color]--;
            Debug.Log($"{color} otobüsüne karakter bindi. Kalan koltuk: {currentBusSeats[color]}");

            if (currentBusSeats[color] == 0)
            {
                Debug.Log($"{color} otobüsü doldu.");
            }

            return true;
        }
        return false;
    }

    private float GetPrefabWidth(GameObject prefab)
    {
        Renderer rend = prefab.GetComponent<Renderer>();
        if (rend != null)
        {
            return rend.bounds.size.x;
        }
        else
        {
            Debug.LogWarning("Prefab'ın Renderer bileşeni yok, width 1 olarak alındı.");
            return 1f; // Default genişlik
        }
    }
}

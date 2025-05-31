using UnityEngine;
using System.Collections.Generic;

public class BusManager : MonoBehaviourSingleton<BusManager>
{
    [System.Serializable]
    public class BusData
    {
        public Color color;
    }

    [SerializeField] private GameObject busPrefab;
    [SerializeField] private Transform busParent;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private List<BusData> busesData;
    [SerializeField] private int seatsPerBus = 4;

    private Dictionary<Color, int> busSeats = new Dictionary<Color, int>();
    private Dictionary<Color, Vector3> busPositions = new Dictionary<Color, Vector3>();

    private void Start() => SpawnBuses();

    private void SpawnBuses()
    {
        float busWidth = busPrefab.GetComponent<Renderer>().bounds.size.x;
        Vector3 startPos = busParent.position;

        for (int i = 0; i < busesData.Count; i++)
        {
            var data = busesData[i];
            Vector3 pos = startPos + Vector3.left * i * (busWidth + spacing);
            GameObject bus = Instantiate(busPrefab, pos, Quaternion.identity, busParent);
            bus.GetComponent<Renderer>().material.color = data.color;

            busSeats[data.color] = seatsPerBus;
            busPositions[data.color] = pos;
        }
    }

    public bool TryBoardCharacter(Character character)
    {
        Color color = character.CharacterColor;
        if (busSeats.ContainsKey(color) && busSeats[color] > 0)
        {
            busSeats[color]--;
            if (busSeats[color] == 0) Debug.Log($"{color} otobüsü doldu!");
            return true;
        }
        return false;
    }

    public Vector3 GetBusPosition(Color color)
    {
        return busPositions.ContainsKey(color) ? busPositions[color] : Vector3.zero;
    }

    public bool AllBusesFull()
    {
        foreach (var seat in busSeats.Values)
        {
            if (seat > 0) return false;
        }
        return true;
    }
}
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BusManager : MonoBehaviourSingleton<BusManager>
{
    [SerializeField] public GameObject busPrefab;
    private List<Bus> activeBuses = new List<Bus>();
    private int currentBusIndex = 0;
    public event Action OnAllBusesFull;

    public void InitializeBuses(BusData[] busData, ColorData colorData)
    {
        ClearBuses();
        
        var orderedBuses = busData.OrderBy(b => b.order).ToArray();
        for (int i = 0; i < orderedBuses.Length; i++)
        {
            GameObject busObj = Instantiate(busPrefab, new Vector3(transform.position.x-orderedBuses[i].order,transform.position.y,transform.position.z), Quaternion.identity);
            busObj.transform.parent = transform;
            Bus bus = busObj.GetComponent<Bus>();
            
            bus.Initialize(
                orderedBuses[i].colorName, 
                orderedBuses[i].seatCount, 
                colorData
            );
            
            busObj.SetActive(i == 0);
            activeBuses.Add(bus);
        }
    }

    public Bus GetActiveBus()
    {
        if (currentBusIndex < activeBuses.Count)
            return activeBuses[currentBusIndex];
        return null;
    }

    public void ActivateNextBus()
    {
        if (currentBusIndex < activeBuses.Count - 1)
        {
            activeBuses[currentBusIndex].gameObject.SetActive(false);
            currentBusIndex++;
            activeBuses[currentBusIndex].gameObject.SetActive(true);
            
            // Yeni otobüs aktif olduğunda bekleme alanını kontrol et
            CheckWaitingAreaForMatches();
        }
        else
        {
            activeBuses[currentBusIndex].gameObject.SetActive(false);
            OnAllBusesFull?.Invoke();
        }
    }
    private void CheckWaitingAreaForMatches()
    {
        if (activeBuses.Count > currentBusIndex)
        {
            string currentBusColor = activeBuses[currentBusIndex].BusColor;
            WaitingArea.Instance.CheckForMatchingCharacters(currentBusColor);
        }
    }

    private void ClearBuses()
    {
        foreach (var bus in activeBuses)
        {
            if (bus != null) Destroy(bus.gameObject);
        }
        activeBuses.Clear();
        currentBusIndex = 0;
    }
}
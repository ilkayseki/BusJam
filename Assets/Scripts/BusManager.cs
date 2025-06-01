using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BusManager : MonoBehaviourSingleton<BusManager>
{
    public GameObject busPrefab;
    private List<Bus> activeBuses = new List<Bus>();
    private int currentBusIndex = 0;

    public void InitializeBuses(BusData[] busData, ColorData colorData)
    {
        ClearBuses();
        
        // Order buses by their order value
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
            
            busObj.SetActive(i == 0); // Only activate first bus
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
        }
        else
        {
            Debug.Log("No more buses available");
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
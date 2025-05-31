using System.Collections.Generic;
using UnityEngine;

public class BusManager : MonoBehaviourSingleton<BusManager>
{
    public GameObject busPrefab;
    public int busCount = 3;
    public ColorData colorData;
    
    private Queue<Bus> busQueue = new Queue<Bus>();
    private Bus activeBus;

    private void Start()
    {
        CreateBuses();
        ActivateNextBus();
    }

    private void CreateBuses()
    {
        for (int i = 0; i < busCount; i++)
        {
            GameObject busObj = Instantiate(busPrefab, transform.position, Quaternion.identity);
            busObj.transform.parent = this.transform;
            Bus bus = busObj.GetComponent<Bus>();
            string color = colorData.GetRandomColorName();
            bus.Init(color, colorData);
            bus.gameObject.SetActive(false);
            busQueue.Enqueue(bus);
        }
    }

    public void ActivateNextBus()
    {
        // First check waiting area for matching characters
        if (activeBus != null)
        {
            WaitingArea.Instance.CheckForMatchingCharacters(activeBus.BusColor);
        }

        if (activeBus != null)
        {
            Destroy(activeBus.gameObject);
        }

        if (busQueue.Count > 0)
        {
            activeBus = busQueue.Dequeue();
            activeBus.gameObject.SetActive(true);
            Debug.Log($"New bus activated: {activeBus.BusColor}");
            
            // Check waiting area again for new bus color
            WaitingArea.Instance.CheckForMatchingCharacters(activeBus.BusColor);
        }
        else
        {
            activeBus = null;
            Debug.Log("No more buses.");
        }
    }

    public Bus GetActiveBus()
    {
        return activeBus;
    }

    public void BusFull()
    {
        Debug.Log($"Bus {activeBus.BusColor} is full.");
        ActivateNextBus();
    }
}
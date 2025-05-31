using System.Collections.Generic;
using UnityEngine;

public class BusManager : MonoBehaviourSingleton<BusManager>
{
    public GameObject busPrefab;
    public int busCount = 3;
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
            BusColor color = (Random.value > 0.5f) ? BusColor.Red : BusColor.Blue;
            bus.Init(color);
            bus.gameObject.SetActive(false);
            busQueue.Enqueue(bus);
        }
    }

    public void ActivateNextBus()
    {
        if (activeBus != null)
        {
            Destroy(activeBus.gameObject);
        }

        if (busQueue.Count > 0)
        {
            activeBus = busQueue.Dequeue();
            activeBus.gameObject.SetActive(true);
            Debug.Log($"Yeni bus aktif oldu: {activeBus.BusColor}");
        }
        else
        {
            activeBus = null;
            Debug.Log("Bus kalmadı.");
        }
    }

    public Bus GetActiveBus()
    {
        return activeBus;
    }

    public void BusFull()
    {
        Debug.Log($"Bus {activeBus.BusColor} doldu.");
        ActivateNextBus();
    }
}
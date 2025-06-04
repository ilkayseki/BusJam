using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class BusManager : MonoBehaviourSingleton<BusManager>
{
    [SerializeField] public GameObject busPrefab;
    [SerializeField] public Transform stopPosition;
    [SerializeField] private Transform finishPosition;
    [SerializeField] private float busMoveDuration = 2f;
    
    private List<Bus> activeBuses = new List<Bus>();
    private int currentBusIndex = 0;
    public event Action OnAllBusesFull;

    public void InitializeBuses(BusData[] busData, ColorData colorData)
    {
        if(busPrefab == null)
        {
            Debug.LogError("BusPrefab is null! Assign prefab in inspector or load it.");
            return;
        }
        
        ClearBuses();
        
        var orderedBuses = busData.OrderBy(b => b.order).ToArray();
        for (int i = 0; i < orderedBuses.Length; i++)
        {
            Vector3 spawnPos = new Vector3(-1 - (i * 10), 0, transform.position.z); // Staggered spawn positions
            GameObject busObj = Instantiate(busPrefab, spawnPos, busPrefab.transform.rotation);
            busObj.transform.parent = transform;
            Bus bus = busObj.GetComponent<Bus>();
            
            bus.Initialize(
                orderedBuses[i].colorName, 
                orderedBuses[i].seatCount, 
                colorData
            );
            
            busObj.SetActive(true);
            activeBuses.Add(bus);
        }

        // Start moving the first bus to the stop
        if (activeBuses.Count > 0)
        {
            MoveCurrentBusToStop();
        }
    }

    public void BusFilled(Bus filledBus)
    {
        // Move filled bus to finish position
        filledBus.MoveToFinish(finishPosition.transform.position, busMoveDuration, () => {
            // Otobüs yok edildikten sonra sıradaki otobüsü hareket ettir
            activeBuses.Remove(filledBus); // Listeden kaldır
        
            if (activeBuses.Count > 0)
            {
                MoveCurrentBusToStop();
            }
            else
            {
                OnAllBusesFull?.Invoke();
            }
        });
    }

    private void MoveCurrentBusToStop()
    {
        if (currentBusIndex < activeBuses.Count)
        {
            activeBuses[currentBusIndex].MoveToStop(stopPosition.transform.position, busMoveDuration, () => {
                // When bus arrives at stop, check for matching characters
                CheckWaitingAreaForMatches();
            });
        }
    }

    public Bus GetActiveBus()
    {
        if (currentBusIndex < activeBuses.Count)
            return activeBuses[currentBusIndex];
        return null;
    }

    private void CheckWaitingAreaForMatches()
    {
        if (currentBusIndex < activeBuses.Count)
        {
            string currentBusColor = activeBuses[currentBusIndex].BusColor;
            WaitingArea.Instance.CheckForMatchingCharacters(currentBusColor);
        }
    }

    private void ClearBuses()
    {
        // Artık direkt yok etmeye gerek yok, MoveToFinish içinde yok ediliyor
        activeBuses.Clear();
        currentBusIndex = 0;
    }
}
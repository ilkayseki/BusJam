using UnityEngine;
using System.Collections.Generic;

public enum GameState { Start, Playing, Finished, GameOver }

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    private GameState _currentState = GameState.Start;
    private List<IGameStateObserver> _observers = new List<IGameStateObserver>();

    public GameState CurrentState => _currentState;

    private void Start()
    {
        BusManager.Instance.OnAllBusesFull += OnAllBusesFull;
        WaitingArea.Instance.OnWaitingAreaFull += OnWaitingAreaFull;
        ChangeState(GameState.Start);
    }

    public void RegisterObserver(IGameStateObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void UnregisterObserver(IGameStateObserver observer)
    {
        if (_observers.Contains(observer))
        {
            _observers.Remove(observer);
        }
    }

    public void StartGame()
    {
        ChangeState(GameState.Playing);
    }

    private void OnAllBusesFull()
    {
        ChangeState(GameState.Finished);
    }

    private void OnWaitingAreaFull()
    {
        ChangeState(GameState.GameOver);
    }

    private void ChangeState(GameState newState)
    {
        _currentState = newState;
        NotifyObservers();
    
        switch (_currentState)
        {
            case GameState.Finished:
                Debug.Log("TEBRİKLER! Tüm otobüsler doldu.");
                CurrentLevelManager.Instance.OnGameStateChanged(GameState.Finished);
                break;
            case GameState.GameOver:
                Debug.Log("OYUN BİTTİ! Bekleme alanı doldu.");
                break;
        }
    }
    private void NotifyObservers()
    {
        foreach (var observer in _observers)
        {
            observer.OnGameStateChanged(_currentState);
        }
    }

    private void OnDestroy()
    {
        if (BusManager.Instance != null)
            BusManager.Instance.OnAllBusesFull -= OnAllBusesFull;
        
        if (WaitingArea.Instance != null)
            WaitingArea.Instance.OnWaitingAreaFull -= OnWaitingAreaFull;
    }
}

public interface IGameStateObserver
{
    void OnGameStateChanged(GameState newState);
}
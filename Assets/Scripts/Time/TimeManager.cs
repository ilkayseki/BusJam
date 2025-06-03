using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class TimeManager : MonoBehaviourSingleton<TimeManager>, IGameStateObserver
{
    private float _levelTime;
    private float _remainingTime;
    private bool _isTimerRunning;
    private Coroutine _timerCoroutine;

    public float RemainingTime => _remainingTime;
    public float LevelTime => _levelTime;
    [SerializeField] private TextMeshProUGUI _timeText;

    public event Action OnTimeOver;

    private void Start()
    {
        GameManager.Instance.RegisterObserver(this);
    }

    public void Initialize(float levelTime)
    {
        _levelTime = levelTime;
        _remainingTime = levelTime;
        UpdateTimeDisplay(); // İlk güncelleme
        
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }
        
        _isTimerRunning = true;
        _timerCoroutine = StartCoroutine(CountdownTimer());
    }

    private IEnumerator CountdownTimer()
    {
        while (_isTimerRunning && _remainingTime > 0)
        {
            yield return new WaitForSeconds(1f);
            
            if (!_isTimerRunning) yield break;
            
            _remainingTime--;
            UpdateTimeDisplay();

            if (_remainingTime <= 0)
            {
                OnTimeOver?.Invoke();
                yield break;
            }
        }
    }

    private void UpdateTimeDisplay()
    {
        if (_timeText != null)
        {
            int minutes = Mathf.FloorToInt(_remainingTime / 60);
            int seconds = Mathf.FloorToInt(_remainingTime % 60);
            _timeText.text = $"Time: {minutes}:{seconds:00}";
            
            // Opsiyonel: Zaman azaldıkça renk değişimi
            float timeRatio = _remainingTime / _levelTime;
            _timeText.color = Color.Lerp(Color.red, Color.white, timeRatio);
        }
    }
    
    public void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                _isTimerRunning = true;
                break;
        
            case GameState.Finished:
                StopTimer();
                break;

            case GameState.GameOver:
                StopTimer();
                break;
        }
    }

    public void StopTimer()
    {
        _isTimerRunning = false;
    }
}
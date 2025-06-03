using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    private int levelNumber;
    private Button button;
    [SerializeField] private Image iconImage; // Tek bir Image bileşeni
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Sprites")]
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

    [Scene] 
    [SerializeField] private string _sceneToLoad;

    private void LoadSelectedScene()
    {
        if (!Application.CanStreamedLevelBeLoaded(_sceneToLoad))
        {
            Debug.LogWarning($"Scene '{_sceneToLoad}' is not in the build settings or the name is incorrect.");
            return;
        }

        StartCoroutine(LoadSceneAsync(_sceneToLoad));
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");

            // %90'a geldiğinde sahne hazır olur ama aktarılmaz.
            if (asyncLoad.progress >= 0.9f)
            {
                Debug.Log("Scene ready. Activating...");
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(int levelNum, bool isUnlocked)
    {
        levelNumber = levelNum;
        levelText.text = levelNum.ToString();
        UpdateButtonState(isUnlocked);

        button.onClick.AddListener(OnButtonClicked);
    }

    private void UpdateButtonState(bool isUnlocked)
    {
        // Sprite'ları değiştirerek durumu güncelle
        iconImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;
        button.interactable = isUnlocked;
    }

    private void OnButtonClicked()
    {
        string jsonPath = $"Levels/level{levelNumber}";
        CurrentLevelManager.Instance.SetCurrentLevel(jsonPath);
        Debug.Log($"Level {levelNumber} selected, path: {jsonPath}");
        LoadSelectedScene();
    }
}
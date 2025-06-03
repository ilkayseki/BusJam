using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelUIManager : MonoBehaviour
{
    [SerializeField] private Transform levelButtonsParent;
    [SerializeField] private GameObject levelButtonPrefab;

    private void Start()
    {
        LoadLevelButtons();
    }

    private void LoadLevelButtons()
    {
        // Resources/Levels klasöründeki tüm level dosyalarını yükle
        var levelFiles = Resources.LoadAll<TextAsset>("Levels");
        var maxUnlocked = CurrentLevelManager.Instance.MaxUnlockedLevel;

        // Level numaralarına göre sırala (level1, level2, ...)
        var sortedLevels = levelFiles
            .OrderBy(f => int.Parse(f.name.Replace("level", "")))
            .ToList();

        foreach (var levelFile in sortedLevels)
        {
            // Dosya adından level numarasını çıkar (örneğin "level1" -> 1)
            int levelNumber = int.Parse(levelFile.name.Replace("level", ""));
            
            // Buton oluştur
            var buttonObj = Instantiate(levelButtonPrefab, levelButtonsParent);
            var levelButton = buttonObj.GetComponent<LevelButton>();
            
            // Level açıksa true, değilse false gönder
            bool isUnlocked = levelNumber <= maxUnlocked;
            levelButton.Initialize(levelNumber, isUnlocked);
        }
    }
}
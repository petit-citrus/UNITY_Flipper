
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LevelUI : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;

    private TMP_Text levelText;

    private void Awake()
    {
        levelText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (levelManager == null)
            levelManager = FindFirstObjectByType<LevelManager>();

        if (levelManager == null)
        {
            Debug.LogWarning("LevelUI: LevelManager não encontrado.");
            return;
        }

        levelManager.OnLevelChanged += UpdateLevel;
        UpdateLevel(levelManager.CurrentLevel);
    }

    private void OnDestroy()
    {
        if (levelManager != null)
            levelManager.OnLevelChanged -= UpdateLevel;
    }

    private void UpdateLevel(int level)
    {
        levelText.text = "NIVEAU : " + level;
    }

}


using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField, Min(1)] private int startingLevel = 1;

    [Tooltip("Score necessário para atingir os níveis 2, 3, 4...")]
    [SerializeField] private int[] scoreThresholds =
    {
        200,
        500,
        1000
    };

    public int CurrentLevel { get; private set; }

    public event Action<int> OnLevelChanged;

    private ScoreManager scoreManager;

    private void Awake()
    {
        CurrentLevel = startingLevel;
    }

    private void Start()
    {
        scoreManager = ScoreManager.Instance;

        if (scoreManager == null)
        {
            Debug.LogWarning(
                "LevelManager: ScoreManager não encontrado."
            );
            return;
        }

        scoreManager.onScoreChanged.AddListener(CheckProgression);

        CheckProgression(scoreManager.CurrentScore);
    }

    private void OnDestroy()
    {
        if (scoreManager != null)
            scoreManager.onScoreChanged.RemoveListener(CheckProgression);
    }

    private void CheckProgression(int score)
    {
        int newLevel = 1;

        for (int i = 0; i < scoreThresholds.Length; i++)
        {
            if (score >= scoreThresholds[i])
                newLevel = i + 2;
            else
                break;
        }

        SetLevel(newLevel);
    }

    public void NextLevel()
    {
        SetLevel(CurrentLevel + 1);
    }

    public void SetLevel(int level)
    {
        level = Mathf.Max(1, level);

        if (CurrentLevel == level)
            return;

        CurrentLevel = level;

        OnLevelChanged?.Invoke(CurrentLevel);
    }

    public void ResetLevel()
    {
        SetLevel(startingLevel);
    }

    [ContextMenu("Test - Next Level")]
    private void TestNextLevel()
    {
        NextLevel();
    }
}

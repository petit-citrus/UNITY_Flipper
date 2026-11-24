
using UnityEngine;
using UnityEngine.Events;

public class HighScoreManager : MonoBehaviour
{
    private const string HighScoreKey = "SkiPinball_HighScore";

    public int BestScore { get; private set; }

    public UnityEvent<int> onBestScoreChanged = new UnityEvent<int>();

    private ScoreManager scoreManager;

    private void Start()
    {
        BestScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        scoreManager = ScoreManager.Instance;

        if (scoreManager == null)
        {
            Debug.LogWarning("HighScoreManager: ScoreManager não encontrado.");
            return;
        }

        scoreManager.onScoreChanged.AddListener(CheckHighScore);
        onBestScoreChanged.Invoke(BestScore);
    }

    private void CheckHighScore(int currentScore)
    {
        if (currentScore <= BestScore)
            return;

        BestScore = currentScore;

        PlayerPrefs.SetInt(HighScoreKey, BestScore);
        PlayerPrefs.Save();

        onBestScoreChanged.Invoke(BestScore);
    }

    private void OnDestroy()
    {
        if (scoreManager != null)
            scoreManager.onScoreChanged.RemoveListener(CheckHighScore);
    }
}

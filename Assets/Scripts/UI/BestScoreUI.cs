
using TMPro;
using UnityEngine;

public class BestScoreUI : MonoBehaviour
{
    [SerializeField] private HighScoreManager highScoreManager;
    [SerializeField] private TMP_Text bestScoreText;

    private void Start()
    {
        if (highScoreManager == null)
            highScoreManager =
                FindFirstObjectByType<HighScoreManager>();

        if (bestScoreText == null)
            bestScoreText = GetComponent<TMP_Text>();

        if (highScoreManager == null || bestScoreText == null)
        {
            Debug.LogWarning("BestScoreUI: referências em falta.");
            return;
        }

        highScoreManager.onBestScoreChanged.AddListener(UpdateBestScore);

        UpdateBestScore(highScoreManager.BestScore);
    }

    private void UpdateBestScore(int score)
    {
        bestScoreText.text = "BEST: " + score;
    }

    private void OnDestroy()
    {
        if (highScoreManager != null)
            highScoreManager.onBestScoreChanged.RemoveListener(UpdateBestScore);
    }
}

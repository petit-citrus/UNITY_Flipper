using TMPro;
using UnityEngine;

/// <summary>
/// Atualiza um texto TextMeshPro com a pontuação do ScoreManager.
/// Colocar no objeto de texto do HUD.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class ScoreUI : MonoBehaviour
{
    [SerializeField] private string prefix = "PONTOS: ";

    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (ScoreManager.Instance == null)
        {
            Debug.LogWarning("ScoreUI : ScoreManager não encontrado na cena.", this);
            return;
        }

        ScoreManager.Instance.onScoreChanged.AddListener(Refresh);
        Refresh(ScoreManager.Instance.CurrentScore);
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.onScoreChanged.RemoveListener(Refresh);
    }

    private void Refresh(int score)
    {
        label.text = prefix + score;
    }
}

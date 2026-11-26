using TMPro;
using UnityEngine;

/// <summary>
/// Met à jour un texte TextMeshPro avec le score du ScoreManager.
/// À placer sur l'objet texte du HUD.
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

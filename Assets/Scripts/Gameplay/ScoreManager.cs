using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    // Singleton - acessível de qualquer script via ScoreManager.Instance
    public static ScoreManager Instance { get; private set; }

    [Header("Pontuação")]
    [SerializeField]
    private int currentScore = 0;

    public int CurrentScore => currentScore;

    [Header("Eventos (opcional - UI, som, efeitos)")]
    public UnityEvent<int> onScoreChanged; // dispara com o novo score total
    public UnityEvent<int> onScoreAdded;   // dispara com a quantidade adicionada

    void Awake()
    {
        // Garante que só existe um ScoreManager na cena
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Adiciona pontos à pontuação atual.
    /// Chamado por bumpers, targets, ou qualquer elemento que dê pontos.
    /// </summary>
    public void AddScore(int points)
    {
        if (points == 0)
            return;

        currentScore += points;

        onScoreAdded?.Invoke(points);
        onScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// Reinicia a pontuação, por exemplo no início de uma nova partida.
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        onScoreChanged?.Invoke(currentScore);
    }
}
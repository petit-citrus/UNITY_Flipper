
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gère le score du joueur pendant la partie.
/// Permet d'ajouter, de réinitialiser et de restaurer
/// les points lors d'un changement de niveau.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // Instance unique accessible depuis les autres scripts.
    public static ScoreManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField]
    private int currentScore = 0;

    public int CurrentScore => currentScore;

    [Header("Événements (interface, sons, effets)")]
    public UnityEvent<int> onScoreChanged;
    public UnityEvent<int> onScoreAdded;

    private void Awake()
    {
        // Vérifie qu'il n'existe qu'un seul ScoreManager dans la scène.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Ajoute les points obtenus pendant la partie.
    /// Notifie les éléments de l'interface et les effets associés.
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
    /// Réinitialise le score au début d'une nouvelle partie.
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        onScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// Restaure le score sauvegardé lors d'un changement de niveau,
    /// sans déclencher l'événement d'ajout de points.
    /// </summary>
    public void RestoreScore(int savedScore)
    {
        currentScore = Mathf.Max(0, savedScore);
        onScoreChanged?.Invoke(currentScore);
    }

    private void OnDestroy()
    {
        // Libère la référence lorsque cette instance est détruite.
        if (Instance == this)
            Instance = null;
    }
}

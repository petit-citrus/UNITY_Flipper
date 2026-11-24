
using System;
using UnityEngine;

/// <summary>
/// Gère la progression du joueur entre les différents niveaux.
/// Le changement de niveau dépend du score obtenu pendant la partie.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Progression des niveaux")]
    [SerializeField, Min(1)] private int startingLevel = 1;

    [Tooltip("Score nécessaire pour atteindre les niveaux 2, 3, 4, etc.")]
    [SerializeField] private int[] scoreThresholds =
    {
        500,
        1000
    };

    // Niveau actuellement atteint par le joueur.
    public int CurrentLevel { get; private set; }

    // Événement déclenché lorsqu'un changement de niveau se produit.
    public event Action<int> OnLevelChanged;

    private ScoreManager scoreManager;

    /// <summary>
    /// Initialise le niveau de départ.
    /// </summary>
    private void Awake()
    {
        CurrentLevel = startingLevel;
    }

    /// <summary>
    /// Récupère le gestionnaire de score et surveille
    /// les changements de points pour gérer la progression.
    /// </summary>
    private void Start()
    {
        scoreManager = ScoreManager.Instance;

        if (scoreManager == null)
        {
            Debug.LogWarning(
                "LevelManager : ScoreManager introuvable."
            );
            return;
        }

        scoreManager.onScoreChanged.AddListener(CheckProgression);

        CheckProgression(scoreManager.CurrentScore);
    }

    /// <summary>
    /// Supprime l'abonnement à l'événement du score
    /// lorsque le gestionnaire est détruit.
    /// </summary>
    private void OnDestroy()
    {
        if (scoreManager != null)
            scoreManager.onScoreChanged.RemoveListener(CheckProgression);
    }

    /// <summary>
    /// Vérifie si le score actuel permet d'atteindre
    /// un nouveau niveau selon les seuils définis.
    /// </summary>
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

    /// <summary>
    /// Passe au niveau suivant.
    /// </summary>
    public void NextLevel()
    {
        SetLevel(CurrentLevel + 1);
    }

    /// <summary>
    /// Définit le niveau actuel et notifie
    /// les autres systèmes si celui-ci a changé.
    /// </summary>
    public void SetLevel(int level)
    {
        level = Mathf.Max(1, level);

        if (CurrentLevel == level)
            return;

        CurrentLevel = level;

        OnLevelChanged?.Invoke(CurrentLevel);
    }

    /// <summary>
    /// Réinitialise la progression au niveau de départ.
    /// </summary>
    public void ResetLevel()
    {
        SetLevel(startingLevel);
    }

    /// <summary>
    /// Permet de tester manuellement le passage au niveau suivant
    /// depuis le menu contextuel du composant dans l'Inspector.
    /// </summary>
    [ContextMenu("Test - Niveau suivant")]
    private void TestNextLevel()
    {
        NextLevel();
    }
}

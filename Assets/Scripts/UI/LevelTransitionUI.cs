
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Affiche la transition entre les niveaux,
/// conserve les données de la partie et charge
/// la scène suivante après un délai.
/// </summary>
public class LevelTransitionUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LifeManager lifeManager;
    [SerializeField] private TMP_Text transitionText;

    [Header("Transition")]
    [SerializeField] private string sourceScene = "Level_1_recup";
    [SerializeField] private string nextScene = "Level_2_test";
    [SerializeField, Min(0f)] private float duration = 5f;

    private Coroutine currentTransition;
    private bool transitionStarted;

    private void Start()
    {
        if (levelManager == null)
            levelManager = FindFirstObjectByType<LevelManager>();

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (scoreManager == null)
            scoreManager = ScoreManager.Instance;

        if (lifeManager == null)
            lifeManager = FindFirstObjectByType<LifeManager>();

        if (transitionText != null)
            transitionText.gameObject.SetActive(false);

        if (levelManager == null ||
            gameManager == null ||
            scoreManager == null ||
            lifeManager == null ||
            transitionText == null)
        {
            Debug.LogError(
                "LevelTransitionUI : références manquantes.", this);
            return;
        }

        levelManager.OnLevelChanged += HandleLevelChanged;
    }

    private void OnDestroy()
    {
        if (levelManager != null)
            levelManager.OnLevelChanged -= HandleLevelChanged;
    }

    /// <summary>
    /// Déclenche le changement de scène lorsque le
    /// joueur atteint le niveau suivant.
    /// </summary>
    private void HandleLevelChanged(int newLevel)
    {
        if (transitionStarted || newLevel != 2)
            return;

        // Empêche la scène de test de déclencher
        // une nouvelle transition vers elle-même.
        if (SceneManager.GetActiveScene().name != sourceScene)
            return;

        if (!gameManager.IsPlaying)
            return;

        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError(
                "LevelTransitionUI : scène introuvable : " +
                nextScene, this);
            return;
        }

        transitionStarted = true;

        // Conserve les données avant le changement de scène.
        LevelSession.Save(
            scoreManager.CurrentScore,
            lifeManager.CurrentLives,
            newLevel
        );

        // Suspend immédiatement la partie.
        gameManager.BeginLevelTransition();

        currentTransition = StartCoroutine(
            TransitionRoutine(newLevel)
        );
    }

    /// <summary>
    /// Affiche le niveau suivant pendant cinq secondes,
    /// puis charge la nouvelle scène.
    /// </summary>
    private IEnumerator TransitionRoutine(int newLevel)
    {
        transitionText.text = "NIVEAU " + newLevel + " !";
        transitionText.gameObject.SetActive(true);

        Debug.Log(
            "LevelTransitionUI : transition vers le niveau " +
            newLevel, this);

        // Continue le compte à rebours même si Time.timeScale vaut zéro.
        yield return new WaitForSecondsRealtime(duration);

        transitionText.gameObject.SetActive(false);

        // Rétablit le temps avant le chargement.
        Time.timeScale = 1f;

        SceneManager.LoadScene(nextScene);
    }
}

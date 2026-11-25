using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Advance once at the configured score, preserving the run across the two scenes.</summary>
public class LevelTransitionUI : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LifeManager lifeManager;
    [SerializeField] private TMP_Text transitionText;
    [SerializeField] private string sourceScene = "Level_1_recup";
    [SerializeField] private string nextScene = "Level_2_test";
    [SerializeField, Min(0f)] private float duration = 5f;
    private Coroutine currentTransition;
    private bool transitionStarted;

    private void Start()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            if (levelManager == null) levelManager = root.GetComponentInChildren<LevelManager>(true);
            if (gameManager == null) gameManager = root.GetComponentInChildren<GameManager>(true);
            if (scoreManager == null) scoreManager = root.GetComponentInChildren<ScoreManager>(true);
            if (lifeManager == null) lifeManager = root.GetComponentInChildren<LifeManager>(true);
        }
        if (transitionText != null) transitionText.gameObject.SetActive(false);
        if (levelManager == null || gameManager == null || scoreManager == null || lifeManager == null)
        {
            Debug.LogError("LevelTransitionUI: missing gameplay managers.", this);
            return;
        }
        levelManager.OnLevelChanged += HandleLevelChanged;
        scoreManager.onScoreChanged.AddListener(HandleScoreChanged);
        gameManager.OnStateChanged += HandleStateChanged;
        TryAdvance();
    }

    public static bool ShouldAdvance(int score, int required, bool playing, bool started, bool source)
    {
        return playing && !started && source && score >= required;
    }

    private void HandleLevelChanged(int level) { TryAdvance(); }
    private void HandleScoreChanged(int score) { TryAdvance(); }
    private void HandleStateChanged(GameState state)
    {
        if (currentTransition != null && state != GameState.Transitioning)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
            transitionStarted = false;
            LevelSession.Clear();
        }
        TryAdvance();
    }

    private void TryAdvance()
    {
        if (!ShouldAdvance(scoreManager.CurrentScore, levelManager.ScoreForLevelTwo,
            gameManager.IsPlaying, transitionStarted, gameObject.scene.name == sourceScene)) return;
        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError("LevelTransitionUI: next scene is not enabled in the build: " + nextScene, this);
            return;
        }
        transitionStarted = true;
        levelManager.SetLevel(2);
        LevelSession.Save(scoreManager.CurrentScore, lifeManager.CurrentLives, 2);
        gameManager.BeginLevelTransition();
        currentTransition = StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        if (transitionText != null)
        {
            transitionText.text = "NIVEAU 2 !";
            transitionText.gameObject.SetActive(true);
        }
        yield return new WaitForSecondsRealtime(duration);
        if (transitionText != null) transitionText.gameObject.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextScene);
    }

    private void OnDestroy()
    {
        if (levelManager != null) levelManager.OnLevelChanged -= HandleLevelChanged;
        if (scoreManager != null) scoreManager.onScoreChanged.RemoveListener(HandleScoreChanged);
        if (gameManager != null) gameManager.OnStateChanged -= HandleStateChanged;
    }
}

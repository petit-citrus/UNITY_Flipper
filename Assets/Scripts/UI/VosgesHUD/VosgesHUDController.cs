using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Synchronise le marqueur des Vosges avec les gestionnaires existants.
/// </summary>
public class VosgesHUDController : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text bestText;
    public TMP_Text livesText;
    public TMP_Text levelText;
    public TMP_Text statusText;
    public TMP_Text hintText;

    public TMP_Text rankingText;
    public Image progress;
    public RectTransform animatedStatus;
    private float transitionStart;
    private float transitionDuration = 5f;
    private int previousRecord;
    private bool recorded;
    private GameState shownState;
    private bool initialized;
    private ScoreManager scoreManager;
    private HighScoreManager highScoreManager;
    private LifeManager lifeManager;
    private LevelManager levelManager;
    private GameManager gameManager;
    private InputAction leftAction;
    private InputAction rightAction;
    private bool leftEnabledHere;
    private bool rightEnabledHere;

    private void Start()
    {
        scoreManager = ScoreManager.Instance;
        highScoreManager = FindFirstObjectByType<HighScoreManager>();
        lifeManager = FindFirstObjectByType<LifeManager>();
        levelManager = FindFirstObjectByType<LevelManager>();
        gameManager = FindFirstObjectByType<GameManager>();

        if (scoreManager != null)
            scoreManager.onScoreChanged.AddListener(UpdateScore);
        if (highScoreManager != null)
            highScoreManager.onBestScoreChanged.AddListener(UpdateBest);
        if (lifeManager != null)
            lifeManager.OnLivesChanged += UpdateLives;
        if (levelManager != null)
            levelManager.OnLevelChanged += UpdateLevel;
        if (gameManager != null)
            gameManager.OnStateChanged += UpdateState;

        // Réutilise les commandes des palettes sans ajouter de nouveaux raccourcis.
        NewMonoBehaviourScript flippers = FindFirstObjectByType<NewMonoBehaviourScript>(FindObjectsInactive.Include);
        if (flippers != null)
        {
            leftAction = flippers.leftFlipAction != null ? flippers.leftFlipAction.action : null;
            rightAction = flippers.rightFlipAction != null ? flippers.rightFlipAction.action : null;
        }
        if (leftAction != null && !leftAction.enabled)
        {
            leftAction.Enable();
            leftEnabledHere = true;
        }
        if (rightAction != null && !rightAction.enabled && rightAction != leftAction)
        {
            rightAction.Enable();
            rightEnabledHere = true;
        }

        previousRecord = PlayerPrefs.GetInt("SkiPinball_HighScore", 0);
        if (LevelSession.HasPendingData) previousRecord = PlayerPrefs.GetInt("Vosges_RunRecord", previousRecord);
        else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level_2_test") previousRecord = PlayerPrefs.GetInt("Vosges_RunRecord", previousRecord);
        UpdateScore(scoreManager != null ? scoreManager.CurrentScore : 0);
        UpdateBest(highScoreManager != null ? highScoreManager.BestScore : PlayerPrefs.GetInt("SkiPinball_HighScore", 0));
        UpdateLives(lifeManager != null ? lifeManager.CurrentLives : 0);
        UpdateLevel(levelManager != null ? levelManager.CurrentLevel : 1);
        UpdateState(gameManager != null ? gameManager.State : GameState.Ready);
    }

    private void Update()
    {
        if (gameManager != null && gameManager.State == GameState.Transitioning) {
            float elapsed = Time.unscaledTime - transitionStart;
            float fraction = Mathf.Clamp01(elapsed / Mathf.Max(.01f, transitionDuration));
            hintText.text = "NOUVELLE PISTE DANS " + Mathf.CeilToInt(Mathf.Max(0, transitionDuration-elapsed)) + " S";
            if (progress != null) progress.fillAmount = fraction;
            if (animatedStatus != null) animatedStatus.localScale = Vector3.one * (1f + .09f * Mathf.Sin(elapsed * 7f));
        }
#if UNITY_EDITOR
        if (gameManager != null && Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame) {
            if (gameManager.State == GameState.Playing) gameManager.PauseGame();
            else if (gameManager.State == GameState.Paused) gameManager.ResumeGame();
        }
#endif
        // Les palettes peuvent désactiver leurs actions en changeant d’état.
        if (gameManager != null && (gameManager.State == GameState.Ready || gameManager.State == GameState.GameOver)) {
            if (leftAction != null && !leftAction.enabled) leftAction.Enable();
            if (rightAction != null && !rightAction.enabled) rightAction.Enable();
        }
        // Ces deux commandes servent uniquement au démarrage et au redémarrage.
        if (gameManager == null || (gameManager.State != GameState.Ready && gameManager.State != GameState.GameOver))
            return;
        bool pressed = (leftAction != null && leftAction.WasPressedThisFrame()) ||
                       (rightAction != null && rightAction.WasPressedThisFrame());
        #if UNITY_EDITOR
        // Raccourci de test indépendant des commandes physiques.
        pressed |= Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#endif
        if (!pressed)
            return;
        if (gameManager.State == GameState.Ready)
            gameManager.StartGame();
        else
            RestartFromLevelOne();
    }

    private void RestartFromLevelOne()
    {
        recorded = false; previousRecord = PlayerPrefs.GetInt("SkiPinball_HighScore",0); PlayerPrefs.SetInt("Vosges_RunRecord",previousRecord);
        // Le redémarrage depuis le niveau 2 recharge d'abord le premier niveau.
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "Level_2_test" || sceneName == "Level2")
        {
            LevelSession.Clear();
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level_1_recup");
        }
        else
        {
            gameManager.RestartGame();
        }
    }

    private void UpdateScore(int value)
    {
        if (scoreText != null) scoreText.text = value.ToString("D6");
    }

    private void UpdateBest(int value)
    {
        if (bestText != null) bestText.text = value.ToString("D6");
    }

    private void UpdateLives(int value)
    {
        if (livesText != null) { livesText.enabled = true; livesText.text = value.ToString(); }
    }

    private void UpdateLevel(int value)
    {
        if (levelText != null) levelText.text = value.ToString("00");
        if (gameManager != null && gameManager.State == GameState.Transitioning)
            UpdateState(GameState.Transitioning);
    }

    private void UpdateState(GameState state)
    {
        if (statusText == null || hintText == null) return;
        bool changed = !initialized || shownState != state;
        initialized = true; shownState = state;
        if (progress != null) { progress.gameObject.SetActive(state == GameState.Transitioning); if(changed) progress.fillAmount = 0; }
        if (animatedStatus != null && state != GameState.Transitioning) animatedStatus.localScale = Vector3.one;
        if (rankingText != null) rankingText.gameObject.SetActive(state == GameState.Ready || state == GameState.GameOver || state == GameState.Paused);
        if (state == GameState.Ready && changed) { recorded = false; previousRecord = PlayerPrefs.GetInt("SkiPinball_HighScore",0); PlayerPrefs.SetInt("Vosges_RunRecord",previousRecord); }
        switch (state)
        {
            case GameState.Ready:
                statusText.text = "PRÊT À JOUER";
                hintText.text = "APPUYEZ SUR UN FLIPPER";
#if UNITY_EDITOR
                hintText.text += " / ESPACE";
#endif
                break;
            case GameState.Playing:
                statusText.text = "SKI PINBALL";
                hintText.text = "MASSIF DES VOSGES";
                break;
            case GameState.Transitioning:
                if (changed) {
                    transitionStart = Time.unscaledTime;
                    var transition = FindFirstObjectByType<LevelTransitionUI>();
                    if (transition != null) { var field = typeof(LevelTransitionUI).GetField("duration", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic); if(field != null) transitionDuration = (float)field.GetValue(transition); }
                }
                statusText.text = "NIVEAU " + (levelManager != null ? levelManager.CurrentLevel : 2);
                hintText.text = "NOUVELLE PISTE · 5 S";
                break;
            case GameState.GameOver:
                int final = scoreManager != null ? scoreManager.CurrentScore : 0;
                if (!recorded) { SaveRanking(final); recorded = true; }
                statusText.text = final > previousRecord ? "GAME OVER - NOUVEAU RECORD !" : "GAME OVER";
                hintText.text = "SCORE FINAL " + (scoreManager != null ? scoreManager.CurrentScore.ToString("N0") : "0") + " · FLIPPER POUR REJOUER";
#if UNITY_EDITOR
                hintText.text += " / ESPACE";
#endif
                break;
            default:
                statusText.text = "PAUSE";
                hintText.text = "COMMANDE PAUSE POUR REPRENDRE";
#if UNITY_EDITOR
                hintText.text += " / P";
#endif
                break;
        }
        if (rankingText != null) { var parts = new List<string>(); for(int i=0;i<5;i++) parts.Add((i+1)+". "+(PlayerPrefs.HasKey("Vosges_Top_"+i)?PlayerPrefs.GetInt("Vosges_Top_"+i).ToString("D6"):"------")); rankingText.text = "TOP 5   |   " + string.Join("   |   ",parts); }
    }
    private void SaveRanking(int score) {
        var scores = new List<int>();
        for(int i=0;i<5;i++) if(PlayerPrefs.HasKey("Vosges_Top_"+i)) scores.Add(PlayerPrefs.GetInt("Vosges_Top_"+i));
        scores.Add(score); scores.Sort((a,b)=>b.CompareTo(a));
        for(int i=0;i<Mathf.Min(5,scores.Count);i++) PlayerPrefs.SetInt("Vosges_Top_"+i,scores[i]);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (scoreManager != null) scoreManager.onScoreChanged.RemoveListener(UpdateScore);
        if (highScoreManager != null) highScoreManager.onBestScoreChanged.RemoveListener(UpdateBest);
        if (lifeManager != null) lifeManager.OnLivesChanged -= UpdateLives;
        if (levelManager != null) levelManager.OnLevelChanged -= UpdateLevel;
        if (gameManager != null) gameManager.OnStateChanged -= UpdateState;
        if (leftEnabledHere && leftAction != null) leftAction.Disable();
        if (rightEnabledHere && rightAction != null) rightAction.Disable();
    }
}

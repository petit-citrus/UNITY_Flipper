
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum GameState
{
    Ready,
    Playing,
    Paused,
    GameOver,
    Transitioning
}

/// <summary>
/// Coordonne les états de la partie, les systèmes de jeu
/// et la restauration des données entre les niveaux.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Systèmes")]
    [SerializeField] private LifeManager lifeManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameOverManager gameOverManager;

    [Header("Gameplay")]
    [SerializeField] private Behaviour[] gameplayBehaviours;

    [Header("Menu principal")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;

    [Header("Menu pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button pauseButton;

    [Header("Affichage de l'état")]
    [SerializeField] private TMP_Text stateLabel;

    [Header("Commandes")]
    [SerializeField] private InputActionReference pauseAction;

    public GameState State { get; private set; } = GameState.Ready;
    public bool IsPlaying => State == GameState.Playing;

    public event Action<GameState> OnStateChanged;

    private InputAction pause;
    private bool pauseEnabledByManager;

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (pauseRestartButton != null)
            pauseRestartButton.onClick.AddListener(RestartGame);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
    }

    private void OnEnable()
    {
        pause = pauseAction != null ? pauseAction.action : null;

        if (pause != null && !pause.enabled)
        {
            pause.Enable();
            pauseEnabledByManager = true;
        }
    }

    private void OnDisable()
    {
        if (pauseEnabledByManager && pause != null)
            pause.Disable();

        pauseEnabledByManager = false;
    }

    private void Start()
    {
        if (scoreManager == null)
            scoreManager = ScoreManager.Instance;

        if (lifeManager != null)
            lifeManager.OnOutOfLives += HandleOutOfLives;
        else
            Debug.LogWarning(
                "GameManager : LifeManager non assigné.", this);

        if (gameOverManager != null)
        {
            gameOverManager.RestartRequested += RestartGame;
            gameOverManager.MainMenuRequested += ReturnToMainMenu;
        }

        if (scoreManager == null)
            Debug.LogWarning(
                "GameManager : ScoreManager introuvable.", this);

        if (LevelSession.HasPendingData)
            RestorePreviousGame();
        else
        {
            PrepareNewGame();
            SetState(GameState.Ready);
        }
    }

    private void OnDestroy()
    {
        if (lifeManager != null)
            lifeManager.OnOutOfLives -= HandleOutOfLives;

        if (gameOverManager != null)
        {
            gameOverManager.RestartRequested -= RestartGame;
            gameOverManager.MainMenuRequested -= ReturnToMainMenu;
        }

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (pause == null || !pause.WasPressedThisFrame())
            return;

        if (State == GameState.Playing)
            PauseGame();
        else if (State == GameState.Paused)
            ResumeGame();
    }

    /// <summary>
    /// Démarre la partie depuis l'état initial.
    /// </summary>
    public void StartGame()
    {
        if (State != GameState.Ready)
            return;

        SetState(GameState.Playing);
    }

    /// <summary>
    /// Met la partie en pause.
    /// </summary>
    public void PauseGame()
    {
        if (State != GameState.Playing)
            return;

        SetState(GameState.Paused);
    }

    /// <summary>
    /// Reprend la partie après une pause.
    /// </summary>
    public void ResumeGame()
    {
        if (State != GameState.Paused)
            return;

        SetState(GameState.Playing);
    }

    /// <summary>
    /// Lance une transition et suspend le gameplay.
    /// </summary>
    public void BeginLevelTransition()
    {
        if (State != GameState.Playing)
            return;

        SetState(GameState.Transitioning);
    }

    /// <summary>
    /// Recommence une nouvelle partie dans la scène actuelle.
    /// </summary>
    public void RestartGame()
    {
        LevelSession.Clear();
        PrepareNewGame();

        LevelManager levelManager =
            FindFirstObjectByType<LevelManager>();

        if (levelManager != null)
            levelManager.ResetLevel();

        SetState(GameState.Playing);
    }

    /// <summary>
    /// Revient à l'état initial de la partie.
    /// </summary>
    public void ReturnToMainMenu()
    {
        LevelSession.Clear();
        PrepareNewGame();

        LevelManager levelManager =
            FindFirstObjectByType<LevelManager>();

        if (levelManager != null)
            levelManager.ResetLevel();

        SetState(GameState.Ready);
    }

    /// <summary>
    /// Réinitialise les données pour une nouvelle partie.
    /// </summary>
    private void PrepareNewGame()
    {
        if (scoreManager != null)
            scoreManager.ResetScore();

        if (lifeManager != null)
            lifeManager.ResetLives();

        if (gameOverManager != null)
            gameOverManager.Hide();
    }

    /// <summary>
    /// Restaure les données conservées pendant
    /// le chargement d'une nouvelle scène.
    /// </summary>
    private void RestorePreviousGame()
    {
        int savedScore = LevelSession.Score;
        int savedLives = LevelSession.Lives;
        int savedLevel = LevelSession.Level;

        if (scoreManager != null)
            scoreManager.RestoreScore(savedScore);

        if (lifeManager != null)
            lifeManager.RestoreLives(savedLives);

        LevelManager levelManager =
            FindFirstObjectByType<LevelManager>();

        if (levelManager != null)
            levelManager.SetLevel(savedLevel);

        if (gameOverManager != null)
            gameOverManager.Hide();

        LevelSession.Clear();

        SetState(GameState.Playing);

        Debug.Log(
            "GameManager : partie restaurée. Score = " +
            savedScore + ", Vies = " + savedLives +
            ", Niveau = " + savedLevel, this);
    }

    /// <summary>
    /// Termine la partie lorsque toutes les vies sont perdues.
    /// </summary>
    private void HandleOutOfLives()
    {
        if (State != GameState.Playing)
            return;

        SetState(GameState.GameOver);

        if (gameOverManager != null)
        {
            int finalScore =
                scoreManager != null ? scoreManager.CurrentScore : 0;

            gameOverManager.Show(finalScore);
        }
    }

    /// <summary>
    /// Met à jour les commandes, les panneaux,
    /// le temps et la physique selon l'état du jeu.
    /// </summary>
    private void SetState(GameState newState)
    {
        State = newState;

        bool playing = newState == GameState.Playing;

        if (gameplayBehaviours != null)
        {
            foreach (Behaviour behaviour in gameplayBehaviours)
            {
                if (behaviour != null)
                    behaviour.enabled = playing;
            }
        }

        // Suspend le temps pendant la pause ou la transition.
        bool stopped =
            newState == GameState.Paused ||
            newState == GameState.Transitioning;

        Time.timeScale = stopped ? 0f : 1f;

        if (lifeManager != null)
        {
            lifeManager.SetTracking(playing);

            if (newState == GameState.Ready ||
                newState == GameState.GameOver ||
                newState == GameState.Transitioning)
            {
                lifeManager.SetBallFrozen(true);
            }
            else if (playing)
            {
                lifeManager.SetBallFrozen(false);
            }
        }

        if (startPanel != null)
            startPanel.SetActive(newState == GameState.Ready);

        if (pausePanel != null)
            pausePanel.SetActive(newState == GameState.Paused);

        if (pauseButton != null)
            pauseButton.gameObject.SetActive(playing);

        UpdateStateLabel();

        OnStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// Actualise l'indication textuelle de l'état de la partie.
    /// </summary>
    private void UpdateStateLabel()
    {
        if (stateLabel == null)
            return;

        switch (State)
        {
            case GameState.Ready:
                stateLabel.text = "PRÊT";
                break;

            case GameState.Paused:
                stateLabel.text = "PAUSE";
                break;

            case GameState.GameOver:
                stateLabel.text = "GAME OVER";
                break;

            default:
                stateLabel.text = string.Empty;
                break;
        }

        stateLabel.gameObject.SetActive(
            State == GameState.Ready ||
            State == GameState.Paused ||
            State == GameState.GameOver
        );
    }
}

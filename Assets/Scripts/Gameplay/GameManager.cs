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
    GameOver
}

/// <summary>
/// Coordena o estado global da partida e os sistemas LifeManager, ScoreManager e GameOverManager.
/// Colocar num objeto "GameManager" (nunca num painel de UI que seja desativado).
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Sistemas")]
    [SerializeField] private LifeManager lifeManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameOverManager gameOverManager;

    [Header("Jogabilidade")]
    [Tooltip("Scripts que só funcionam no estado Playing (ex.: FlipperController, BallLauncher)")]
    [SerializeField] private Behaviour[] gameplayBehaviours;

    [Header("Menu inicial")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;

    [Header("Menu de pausa")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [Tooltip("Opcional")]
    [SerializeField] private Button pauseRestartButton;
    [Tooltip("Opcional: botão de pausa visível durante o jogo")]
    [SerializeField] private Button pauseButton;

    [Header("Indicação de estado (opcional)")]
    [SerializeField] private TMP_Text stateLabel;

    [Header("Comandos")]
    [Tooltip("Ação de pausa (ex.: Escape ou P)")]
    [SerializeField] private InputActionReference pauseAction;

    public GameState State { get; private set; } = GameState.Ready;
    public bool IsPlaying => State == GameState.Playing;

    public event Action<GameState> OnStateChanged;

    private InputAction pause;
    private bool pauseEnabledByManager;

    private void Awake()
    {
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (pauseRestartButton != null) pauseRestartButton.onClick.AddListener(RestartGame);
        if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);
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
            Debug.LogWarning("GameManager : LifeManager não atribuído.", this);

        if (gameOverManager != null)
        {
            gameOverManager.RestartRequested += RestartGame;
            gameOverManager.MainMenuRequested += ReturnToMainMenu;
        }
        else
        {
            Debug.LogWarning("GameManager : GameOverManager não atribuído.", this);
        }

        if (scoreManager == null)
            Debug.LogWarning("GameManager : ScoreManager não encontrado.", this);

        PrepareNewGame();
        SetState(GameState.Ready);
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

        // Garante que o tempo não fica parado ao recarregar a cena.
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

    // ------------------------------------------------------------------
    // API pública (também usada pelos botões)
    // ------------------------------------------------------------------

    /// <summary>Ready -> Playing (botão Start).</summary>
    public void StartGame()
    {
        if (State != GameState.Ready)
            return;

        SetState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (State != GameState.Playing)
            return;

        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused)
            return;

        SetState(GameState.Playing);
    }

    /// <summary>Reinicia a partida imediatamente (pausa ou Game Over).</summary>
    public void RestartGame()
    {
        PrepareNewGame();
        SetState(GameState.Playing);
    }

    /// <summary>Volta ao menu inicial (botão Main Menu do Game Over).</summary>
    public void ReturnToMainMenu()
    {
        PrepareNewGame();
        SetState(GameState.Ready);
    }

    // ------------------------------------------------------------------
    // Interno
    // ------------------------------------------------------------------

    private void PrepareNewGame()
    {
        if (scoreManager != null) scoreManager.ResetScore();
        if (lifeManager != null) lifeManager.ResetLives();
        if (gameOverManager != null) gameOverManager.Hide();
    }

    // O LifeManager avisa quando as vidas chegam a zero.
    private void HandleOutOfLives()
    {
        if (State != GameState.Playing)
            return;

        SetState(GameState.GameOver);

        if (gameOverManager != null)
        {
            int finalScore = scoreManager != null ? scoreManager.CurrentScore : 0;
            gameOverManager.Show(finalScore);
        }
    }

    private void SetState(GameState newState)
    {
        State = newState;

        bool playing = newState == GameState.Playing;

        // Só se pode jogar no estado Playing.
        if (gameplayBehaviours != null)
        {
            foreach (Behaviour behaviour in gameplayBehaviours)
            {
                if (behaviour != null)
                    behaviour.enabled = playing;
            }
        }

        Time.timeScale = newState == GameState.Paused ? 0f : 1f;

        if (lifeManager != null)
        {
            lifeManager.SetTracking(playing);

            // Bola parada antes de começar e depois do fim da partida.
            if (newState == GameState.Ready || newState == GameState.GameOver)
                lifeManager.SetBallFrozen(true);
            else if (playing)
                lifeManager.SetBallFrozen(false);
        }

        if (startPanel != null) startPanel.SetActive(newState == GameState.Ready);
        if (pausePanel != null) pausePanel.SetActive(newState == GameState.Paused);
        if (pauseButton != null) pauseButton.gameObject.SetActive(playing);

        UpdateStateLabel();
        OnStateChanged?.Invoke(newState);
    }

    private void UpdateStateLabel()
    {
        if (stateLabel == null)
            return;

        switch (State)
        {
            case GameState.Ready:
                stateLabel.text = "PRONTO";
                break;
            case GameState.Paused:
                stateLabel.text = "PAUSA";
                break;
            case GameState.GameOver:
                stateLabel.text = "GAME OVER";
                break;
            default:
                stateLabel.text = string.Empty;
                break;
        }

        stateLabel.gameObject.SetActive(State != GameState.Playing);
    }
}

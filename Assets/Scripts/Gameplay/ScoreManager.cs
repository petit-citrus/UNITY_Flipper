
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
    public bool BallInPlay { get; private set; }
    private bool launchPending;
    private Rigidbody launchedBall;
    private Vector3 servePosition;
    private float scoreAfter;
    private GameManager game;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticInstance() { Instance = null; }

    [Header("Événements (interface, sons, effets)")]
    public UnityEvent<int> onScoreChanged = new UnityEvent<int>();
    public UnityEvent<int> onScoreAdded = new UnityEvent<int>();

    private void Awake()
    {
        // Vérifie qu'il n'existe qu'un seul ScoreManager dans la scène.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentScore = 0;
        EndBallInPlay();
        ResolveGame();
    }

    private void ResolveGame()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            if (game == null) game = root.GetComponentInChildren<GameManager>(true);
    }

    public static bool CanAwardPoints(int points, bool ballInPlay, bool playing)
    { return points > 0 && ballInPlay && playing; }

    public static bool HasLeftServe(float time, float firstAllowedTime, float distanceSquared)
    { return time >= firstAllowedTime && distanceSquared >= 1f; }

    public void BeginBallInPlay(Rigidbody ball)
    {
        if (BallInPlay || launchPending || ball == null) return;
        if (game == null) ResolveGame();
        if (game == null || !game.IsPlaying) return;
        launchedBall = ball;
        servePosition = ball.position;
        scoreAfter = Time.time + .15f;
        launchPending = true;
    }

    public void EndBallInPlay()
    { BallInPlay = false; launchPending = false; launchedBall = null; }

    private void Update()
    {
        if (!launchPending || launchedBall == null || game == null || !game.IsPlaying || launchedBall.isKinematic) return;
        if (!HasLeftServe(Time.time, scoreAfter, (launchedBall.position - servePosition).sqrMagnitude)) return;
        BallInPlay = true;
        launchPending = false;
    }

    /// <summary>
    /// Ajoute les points obtenus pendant la partie.
    /// Notifie les éléments de l'interface et les effets associés.
    /// </summary>
    public void AddScore(int points)
    {
        if (game == null) ResolveGame();
        if (!CanAwardPoints(points, BallInPlay, game != null && game.IsPlaying))
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
        EndBallInPlay();
        currentScore = 0;
        onScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// Restaure le score sauvegardé lors d'un changement de niveau,
    /// sans déclencher l'événement d'ajout de points.
    /// </summary>
    public void RestoreScore(int savedScore)
    {
        EndBallInPlay();
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

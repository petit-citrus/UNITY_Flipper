
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère les vies du joueur, la réapparition de la balle
/// et la mise à jour de l'interface des vies.
/// La détection de la chute est assurée par DeathZone.
/// </summary>
public class LifeManager : MonoBehaviour
{
    [Header("Vies")]
    [Min(1)]
    [SerializeField] private int startingLives = 3;

    [Header("Balle")]
    [SerializeField] private Rigidbody ball;

    [Tooltip("Point de réapparition. Si non défini, utilise la position initiale de la balle.")]
    [SerializeField] private Transform ballSpawnPoint;

    [SerializeField] private string ballTag = "Ball";

    [Tooltip("Délai en secondes avant la réapparition de la balle.")]
    [Min(0f)]
    [SerializeField] private float respawnDelay = 1f;

    [Header("Interface")]
    [SerializeField] private TMP_Text livesText;

    [Tooltip("Icône servant de modèle pour représenter les vies restantes.")]
    [SerializeField] private GameObject lifeIconTemplate;

    [SerializeField] private Color activeIconColor = Color.white;

    [SerializeField]
    private Color lostIconColor =
        new Color(1f, 1f, 1f, 0.2f);

    [Header("Débogage")]
    [SerializeField] private bool debugLogs = true;

    public int CurrentLives { get; private set; }
    public int StartingLives => startingLives;

    // Événement déclenché lorsque le joueur n'a plus de vies.
    public event Action OnOutOfLives;

    // Événement déclenché à chaque modification du nombre de vies.
    public event Action<int> OnLivesChanged;

    private readonly List<Image> lifeIcons = new List<Image>();

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private bool tracking;
    private bool handlingLoss;
    private Transform table;

    private void Awake()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                if (item.name == "BaseLvl_1") table = item;
        // Récupère la position à laquelle la balle doit réapparaître.
        if (ball == null)
        {
            Debug.LogWarning(
                "LifeManager : aucune balle assignée.", this);
        }
        else if (ballSpawnPoint != null)
        {
            spawnPosition = ballSpawnPoint.position;
            spawnRotation = ballSpawnPoint.rotation;
        }
        else
        {
            spawnPosition = ball.position;
            spawnRotation = ball.rotation;
        }

        CurrentLives = startingLives;

        BuildIcons();
        RefreshUI();
    }

    /// <summary>
    /// Active ou désactive la détection des pertes de balle.
    /// </summary>
    public void SetTracking(bool value)
    {
        tracking = value;
    }

    /// <summary>
    /// Appelé par DeathZone lorsqu'un objet entre dans la zone de chute.
    /// Vérifie la balle, retire une vie et lance sa réapparition.
    /// </summary>
    public void HandleBallLost(Collider other)
    {
        if (other == null) return;
        if (!tracking || handlingLoss)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "LifeManager : chute ignorée. " +
                    "tracking=" + tracking +
                    ", handlingLoss=" + handlingLoss,
                    this
                );
            }

            return;
        }

        bool isBall =
            other.CompareTag(ballTag) ||
            (ball != null && other.attachedRigidbody == ball);

        if (!isBall)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "LifeManager : objet ignoré : " +
                    other.name, this);
            }

            return;
        }

        Rigidbody lostBall = other.attachedRigidbody != null ? other.attachedRigidbody : ball;
        if (lostBall == null) return;
        if (table != null)
        {
            Vector3 local = Quaternion.Inverse(table.rotation) * (lostBall.worldCenterOfMass - table.position);
            if (!DeathZone.IsDrainPosition(local.z)) return;
        }

        if (debugLogs)
        {
            Debug.Log(
                "LifeManager : balle perdue. Vies : " +
                CurrentLives + " -> " +
                (CurrentLives - 1), this);
        }

        handlingLoss = true;
        if (ScoreManager.Instance != null) ScoreManager.Instance.EndBallInPlay();

        CurrentLives = Mathf.Max(0, CurrentLives - 1);

        RefreshUI();
        OnLivesChanged?.Invoke(CurrentLives);

        // Immobilise la balle pendant le traitement de la perte.
        SetBallFrozen(true);

        if (CurrentLives <= 0)
        {
            OnOutOfLives?.Invoke();
            return;
        }

        StartCoroutine(RespawnRoutine());
    }

    /// <summary>
    /// Réinitialise les vies et replace la balle
    /// au début d'une nouvelle partie.
    /// </summary>
    public void ResetLives()
    {
        StopAllCoroutines();

        CurrentLives = startingLives;
        handlingLoss = false;

        ResetBall();

        RefreshUI();
        OnLivesChanged?.Invoke(CurrentLives);
    }

    /// <summary>
    /// Restaure les vies conservées lors d'un changement de niveau.
    /// Replace également la balle à sa position initiale.
    /// </summary>
    public void RestoreLives(int savedLives)
    {
        StopAllCoroutines();

        CurrentLives = Mathf.Clamp(
            savedLives, 0, startingLives);

        handlingLoss = false;

        ResetBall();

        RefreshUI();
        OnLivesChanged?.Invoke(CurrentLives);

        if (debugLogs)
        {
            Debug.Log(
                "LifeManager : vies restaurées : " +
                CurrentLives, this);
        }
    }

    /// <summary>
    /// Replace la balle à sa position de départ
    /// et annule ses vitesses linéaire et angulaire.
    /// </summary>
    public void ResetBall()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.EndBallInPlay();
        if (ball == null)
            return;

        ball.isKinematic = false;

        ClearVelocity();

        ball.position = spawnPosition;
        ball.rotation = spawnRotation;

        ball.transform.SetPositionAndRotation(
            spawnPosition, spawnRotation);

        ball.Sleep();
    }

    /// <summary>
    /// Immobilise ou libère la balle selon l'état de la partie.
    /// </summary>
    public void SetBallFrozen(bool frozen)
    {
        if (ball == null)
            return;

        if (frozen)
        {
            if (!ball.isKinematic)
                ClearVelocity();

            ball.isKinematic = true;
        }
        else
        {
            ball.isKinematic = false;
        }
    }

    /// <summary>
    /// Attend le délai configuré avant de replacer la balle.
    /// </summary>
    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        ResetBall();
        handlingLoss = false;
    }

    /// <summary>
    /// Annule les mouvements de la balle.
    /// Compatible avec Unity 6 et les versions précédentes.
    /// </summary>
    private void ClearVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        ball.linearVelocity = Vector3.zero;
#else
        ball.velocity = Vector3.zero;
#endif

        ball.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Crée les icônes correspondant au nombre initial de vies.
    /// </summary>
    private void BuildIcons()
    {
        if (lifeIconTemplate == null)
            return;

        lifeIconTemplate.SetActive(false);

        for (int i = 0; i < startingLives; i++)
        {
            GameObject icon = Instantiate(
                lifeIconTemplate,
                lifeIconTemplate.transform.parent
            );

            icon.name = "LifeIcon_" + (i + 1);
            icon.SetActive(true);

            Image image = icon.GetComponentInChildren<Image>();

            if (image != null)
                lifeIcons.Add(image);
        }
    }

    /// <summary>
    /// Actualise le texte et les icônes affichant
    /// le nombre de vies restantes.
    /// </summary>
    private void RefreshUI()
    {
        if (livesText != null)
            livesText.text = "Vies: " + CurrentLives;

        for (int i = 0; i < lifeIcons.Count; i++)
        {
            lifeIcons[i].color =
                i < CurrentLives
                    ? activeIconColor
                    : lostIconColor;
        }
    }
}

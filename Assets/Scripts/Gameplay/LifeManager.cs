using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gere as vidas, a reposição da bola e a interface das vidas.
/// A deteção da queda é feita pelo DeathZone, que chama HandleBallLost().
/// </summary>
public class LifeManager : MonoBehaviour
{
    [Header("Vidas")]
    [Min(1)]
    [SerializeField] private int startingLives = 3;

    [Header("Bola")]
    [SerializeField] private Rigidbody ball;
    [Tooltip("Posição de reposição. Se vazio, usa a posição inicial da bola na cena.")]
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private string ballTag = "Ball";
    [Tooltip("Tempo (s) entre a queda e a reposição da bola")]
    [Min(0f)]
    [SerializeField] private float respawnDelay = 1f;

    [Header("Interface")]
    [SerializeField] private TMP_Text livesText;
    [Tooltip("Ícone de bola (Image) que serve de modelo. É desativado e clonado uma vez por vida.")]
    [SerializeField] private GameObject lifeIconTemplate;
    [SerializeField] private Color activeIconColor = Color.white;
    [SerializeField] private Color lostIconColor = new Color(1f, 1f, 1f, 0.2f);

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public int CurrentLives { get; private set; }
    public int StartingLives => startingLives;

    /// <summary>Disparado quando a última vida é perdida (o GameManager escuta).</summary>
    public event Action OnOutOfLives;

    /// <summary>Disparado sempre que o número de vidas muda.</summary>
    public event Action<int> OnLivesChanged;

    private readonly List<Image> lifeIcons = new List<Image>();

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private bool tracking;      // só conta quedas durante o estado Playing
    private bool handlingLoss;  // evita perder várias vidas na mesma queda

    private void Awake()
    {
        if (ball == null)
        {
            Debug.LogWarning("LifeManager : bola não atribuída.", this);
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

    // ------------------------------------------------------------------
    // API pública
    // ------------------------------------------------------------------

    /// <summary>Chamado pelo GameManager: ativa/desativa a deteção de quedas.</summary>
    public void SetTracking(bool value)
    {
        tracking = value;
    }

    /// <summary>Chamado pelo DeathZone quando algo entra na zona.</summary>
    public void HandleBallLost(Collider other)
    {
        if (!tracking || handlingLoss)
        {
            if (debugLogs)
                Debug.Log("LifeManager : queda ignorada (tracking=" + tracking +
                          ", handlingLoss=" + handlingLoss + "). " +
                          "Se tracking=False, o jogo não está em Playing (carregou em START?).", this);
            return;
        }

        bool isBall = other.CompareTag(ballTag) ||
                      (ball != null && other.attachedRigidbody == ball);

        if (!isBall)
        {
            if (debugLogs)
                Debug.Log("LifeManager : '" + other.name + "' (tag " + other.tag +
                          ") entrou na DeathZone mas não é a bola.", this);
            return;
        }

        if (debugLogs)
            Debug.Log("LifeManager : bola perdida. Vidas: " + CurrentLives + " -> " + (CurrentLives - 1), this);

        handlingLoss = true;

        CurrentLives = Mathf.Max(0, CurrentLives - 1);
        RefreshUI();
        OnLivesChanged?.Invoke(CurrentLives);

        // Bola parada enquanto a vida é processada.
        SetBallFrozen(true);

        if (CurrentLives <= 0)
        {
            OnOutOfLives?.Invoke();
            return;
        }

        StartCoroutine(RespawnRoutine());
    }

    /// <summary>Nova partida: vidas ao máximo e bola na posição inicial.</summary>
    public void ResetLives()
    {
        StopAllCoroutines();

        CurrentLives = startingLives;
        handlingLoss = false;

        ResetBall();
        RefreshUI();
        OnLivesChanged?.Invoke(CurrentLives);
    }

    /// <summary>Reposiciona a bola e anula velocidade e rotação.</summary>
    public void ResetBall()
    {
        if (ball == null)
            return;

        // Só se pode definir velocidade num Rigidbody não cinemático.
        ball.isKinematic = false;
        ClearVelocity();

        ball.position = spawnPosition;
        ball.rotation = spawnRotation;
        ball.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        ball.Sleep();
    }

    /// <summary>Imobiliza (ou liberta) a bola.</summary>
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

    // ------------------------------------------------------------------
    // Interno
    // ------------------------------------------------------------------

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        ResetBall();
        handlingLoss = false;
    }

    private void ClearVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        ball.linearVelocity = Vector3.zero;
#else
        ball.velocity = Vector3.zero;
#endif
        ball.angularVelocity = Vector3.zero;
    }

    private void BuildIcons()
    {
        if (lifeIconTemplate == null)
            return;

        lifeIconTemplate.SetActive(false);

        for (int i = 0; i < startingLives; i++)
        {
            GameObject icon = Instantiate(lifeIconTemplate, lifeIconTemplate.transform.parent);
            icon.name = "LifeIcon_" + (i + 1);
            icon.SetActive(true);

            Image image = icon.GetComponentInChildren<Image>();
            if (image != null)
                lifeIcons.Add(image);
        }
    }

    private void RefreshUI()
    {
        if (livesText != null)
            livesText.text = "VIDAS: " + CurrentLives;

        for (int i = 0; i < lifeIcons.Count; i++)
            lifeIcons[i].color = i < CurrentLives ? activeIconColor : lostIconColor;
    }
}
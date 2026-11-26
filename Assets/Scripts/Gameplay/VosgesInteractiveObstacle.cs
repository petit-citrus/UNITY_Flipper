using System.Collections.Generic;
using UnityEngine;

/// <summary>Score, rebond contrôlé et déplacement des nouveaux obstacles hivernaux.</summary>
[RequireComponent(typeof(Rigidbody))]
public class VosgesInteractiveObstacle : MonoBehaviour
{
    public int points = 50;
    public float kick = 3.5f;
    public float cooldown = .4f;
    public bool passageOnly;
    public float rotationSpeed;
    public Renderer accent;

    private GameManager game;
    private Rigidbody body;
    private Quaternion restRotation;
    private float angle;
    private float nextScoreTime;
    private float flashUntil;
    private MaterialPropertyBlock feedback;
    private Color restingColor;
    private readonly HashSet<Rigidbody> enteringGate = new HashSet<Rigidbody>();

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        restRotation = body.rotation;
        foreach (GameManager candidate in FindObjectsByType<GameManager>(FindObjectsSortMode.None))
            if (candidate.gameObject.scene == gameObject.scene) { game = candidate; break; }
        feedback = new MaterialPropertyBlock();
        if (accent != null) restingColor = accent.sharedMaterial.color;
    }

    private void FixedUpdate()
    {
        if (game == null || !game.IsPlaying || Mathf.Approximately(rotationSpeed, 0)) return;
        angle = Mathf.Repeat(angle + rotationSpeed * Time.fixedDeltaTime, 360);
        body.MoveRotation(restRotation * Quaternion.AngleAxis(angle, Vector3.up));
    }

    private bool CanScore(Rigidbody ball)
    {
        return game != null && game.IsPlaying && ball != null && ball.CompareTag("Ball")
            && ScoreManager.Instance != null && ScoreManager.Instance.BallInPlay && Time.time >= nextScoreTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (passageOnly || !CanScore(collision.rigidbody)) return;
        Vector3 normal = Vector3.ProjectOnPlane(collision.GetContact(0).point - transform.position, transform.up);
        if (normal.sqrMagnitude < .001f)
            normal = Vector3.ProjectOnPlane(-collision.relativeVelocity, transform.up);
        if (normal.sqrMagnitude > .001f)
            collision.rigidbody.AddForce(normal.normalized * kick, ForceMode.Impulse);
        Award();
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody ball = other.attachedRigidbody;
        if (!passageOnly || !CanScore(ball)) return;
        // Repère une entrée nette par le bas ; attribue les points seulement après la sortie de l'autre côté.
        if (Vector3.Dot(ball.linearVelocity, transform.forward) <= .4f) return;
        if (Mathf.Abs(transform.InverseTransformPoint(ball.worldCenterOfMass).x) > .64f) return;
        enteringGate.Add(ball);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody ball = other.attachedRigidbody;
        if (!passageOnly || ball == null || !enteringGate.Remove(ball) || !CanScore(ball)) return;
        Vector3 local = transform.InverseTransformPoint(ball.worldCenterOfMass);
        if (local.z <= 0 || Mathf.Abs(local.x) > .64f
            || Vector3.Dot(ball.linearVelocity, transform.forward) <= .4f) return;
        Award();
    }

    private void Award()
    {
        nextScoreTime = Time.time + Mathf.Max(.1f, cooldown);
        flashUntil = Time.time + .22f;
        ScoreManager.Instance.AddScore(points);
    }

    private void Update()
    {
        if (accent == null || feedback == null) return;
        float strength = Mathf.Clamp01((flashUntil - Time.time) / .22f);
        feedback.SetColor("_BaseColor", Color.Lerp(restingColor, Color.white, strength));
        feedback.SetColor("_EmissionColor", new Color(.85f, .95f, 1) * strength * .35f);
        accent.SetPropertyBlock(feedback);
    }

    private void OnDisable()
    {
        enteringGate.Clear();
        if (accent != null) accent.SetPropertyBlock(null);
    }
}

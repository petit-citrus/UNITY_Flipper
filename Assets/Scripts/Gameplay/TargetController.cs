using UnityEngine;

public class TargetController : MonoBehaviour
{
    [Header("Force de la Target")]
    [Tooltip("Force de l'impulsion appliquée à la balle lors du contact")]
    public float bounceForce = 10f;

    [Header("Détection")]
    [Tooltip("Tag utilisé pour identifier la balle")]
    public string ballTag = "Ball";

    [Header("Score")]
    [Tooltip("Points attribués lors d'une collision")]
    public int scoreValue = 100;

    [Header("Animation")]
    [Tooltip("Nom du trigger de l'Animator déclenché lors d'un impact")]
    public string impactTrigger = "Impact";

    private Animator monAnimator;

    void Start()
    {
        // Cherche l'Animator sur l'objet, puis dans les parents, puis dans les enfants
        monAnimator = GetComponent<Animator>();
        if (monAnimator == null) monAnimator = GetComponentInParent<Animator>();
        if (monAnimator == null) monAnimator = GetComponentInChildren<Animator>();

        if (monAnimator == null)
            Debug.LogWarning("TargetController : aucun Animator trouvé sur " + gameObject.name, this);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Vérifie que l'objet en collision est bien la balle
        if (!collision.gameObject.CompareTag(ballTag))
            return;

        Rigidbody ballRigidbody = collision.rigidbody;

        if (ballRigidbody == null)
            return;

        // Direction du rebond : du centre de la target vers le point de contact
        ContactPoint contact = collision.GetContact(0);
        Vector3 bounceDirection = (contact.point - transform.position).normalized;

        // Applique l'impulsion pour repousser la balle
        ballRigidbody.AddForce(
            bounceDirection * bounceForce,
            ForceMode.Impulse
        );

        OnTargetHit();
    }

    protected virtual void OnTargetHit()
    {
        // Points
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(scoreValue);

        // Animation d'impact
        if (monAnimator != null)
            monAnimator.SetTrigger(impactTrigger);
    }
}
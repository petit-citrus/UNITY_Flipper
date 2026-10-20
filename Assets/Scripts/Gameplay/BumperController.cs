using UnityEngine;

public class BumperController : MonoBehaviour
{
    [Header("Force du Bumper")]
    [Tooltip("Force de l'impulsion appliquée à la balle lors du contact")]
    public float bounceForce = 10f;

    [Header("Détection")]
    [Tooltip("Tag utilisé pour identifier la balle")]
    public string ballTag = "Ball";

    [Header("Score / Effets")]
    [Tooltip("Points attribués lors d'une collision")]
    public int scoreValue = 100;

    // Référence pour tes animations
    private Animator monAnimator;

    void Start()
    {
        // Récupère l'Animator attaché à l'objet au démarrage
        monAnimator = GetComponent<Animator>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Vérifie que l'objet en collision est bien la balle
        if (!collision.gameObject.CompareTag(ballTag))
            return;

        Rigidbody ballRigidbody = collision.rigidbody;

        if (ballRigidbody == null)
            return;

        // Calcule la direction du rebond : du centre du bumper vers le point de contact
        ContactPoint contact = collision.GetContact(0);
        Vector3 bounceDirection = (contact.point - transform.position).normalized;

        // Applique l'impulsion pour repousser la balle
        ballRigidbody.AddForce(
            bounceDirection * bounceForce,
            ForceMode.Impulse
        );

        OnBumperHit(ballRigidbody);
    }

    protected virtual void OnBumperHit(Rigidbody ball)
    {
        // Ajoute les points via le ScoreManager de ton collègue
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        // Déclenche ton animation "Impact"
        if (monAnimator != null)
        {
            monAnimator.SetTrigger("Impact");
        }
    }
}
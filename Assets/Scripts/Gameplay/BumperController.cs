using UnityEngine;

public class BumperController : MonoBehaviour
{
    [Header("Force du Bumper")]
    [Tooltip("Force de l'impulsion appliquée à la balle lors du contact")]
    public float bounceForce = 10f;

    [Header("Détection")]
    [Tooltip("Tag utilisé pour identifier la balle")]
    public string ballTag = "Ball";

    [Header("Futur - Score / Effets (optionnel)")]
    [Tooltip("Points attribués lors d'une collision (utilisé dans une version ultérieure)")]
    public int scoreValue = 100;

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

    // Point d'extension pour les futures fonctionnalités
    // (score, animation, effets visuels, sons, etc.)
    protected virtual void OnBumperHit(Rigidbody ball)
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(scoreValue);
    }
}
using UnityEngine;

public class BumperController : MonoBehaviour
{
    [Header("Force du Bumper")]
    [Tooltip("Force de l'impulsion appliquée à la balle lors du contact")]
    public float bounceForce = 10f;

    [Header("Détection")]
    [Tooltip("Tag utilisé pour identifier la balle")]
    public string ballTag = "Ball";

    void OnCollisionEnter(Collision collision)
    {
        if (GetComponent<TargetController>() != null || GetComponent<TargetHitRelay>() != null
            || GetComponentInParent<TargetController>() != null) return;
        Rigidbody ballRigidbody = collision.rigidbody;
        if (ballRigidbody == null
            || (!collision.gameObject.CompareTag(ballTag) && !ballRigidbody.CompareTag(ballTag))
            || collision.contactCount == 0) return;

        ContactPoint contact = collision.GetContact(0);
        Collider bumperCollider = GetComponent<Collider>();
        Vector3 bounceCenter = bumperCollider != null ? bumperCollider.bounds.center : transform.position;
        Vector3 bounceDirection = contact.point - bounceCenter;
        if (bounceDirection.sqrMagnitude < 0.0001f)
            bounceDirection = ballRigidbody.worldCenterOfMass - bounceCenter;
        if (bounceDirection.sqrMagnitude < 0.0001f) return;

        ballRigidbody.AddForce(
            bounceDirection.normalized * bounceForce,
            ForceMode.Impulse);
    }
}

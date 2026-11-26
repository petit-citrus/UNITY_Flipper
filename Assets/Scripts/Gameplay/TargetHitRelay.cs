using UnityEngine;

/// <summary>Transmet l'impact d'un collider enfant à l'unique contrôleur de score de son animation.</summary>
[RequireComponent(typeof(Collider))]
public class TargetHitRelay : MonoBehaviour
{
    public TargetController target;

    private void OnCollisionEnter(Collision collision)
    {
        if (target != null && collision.contactCount > 0)
            target.TryHit(collision.rigidbody, collision.GetContact(0).point);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target != null && other.attachedRigidbody != null)
            target.TryHit(other.attachedRigidbody, GetComponent<Collider>().ClosestPoint(other.attachedRigidbody.worldCenterOfMass));
    }
}

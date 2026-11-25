using UnityEngine;

/// <summary>Forward a child collider's impact to the single scoring controller of its animation.</summary>
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

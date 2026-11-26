using UnityEngine;

/// <summary>Un impact comptabilisé et un rebond par cible animée, y compris avec ses colliders enfants.</summary>
public class TargetController : MonoBehaviour
{
    public float bounceForce = 10f;
    public string ballTag = "Ball";
    public int scoreValue = 100;
    public string impactTrigger = "Impact";
    [SerializeField] private Animator impactAnimator;
    [SerializeField] private Transform table;
    [SerializeField, Min(.05f)] private float hitCooldown = .18f;
    private GameManager game;
    private float nextHitTime;
    private bool hasImpactTrigger;

    public bool Configure(Animator animator, Transform board, int points)
    {
        bool changed = impactAnimator != animator || table != board || scoreValue != points;
        impactAnimator = animator;
        table = board;
        scoreValue = points;
        CacheTrigger();
        return changed;
    }

    private void Start()
    {
        if (impactAnimator == null) impactAnimator = GetComponent<Animator>();
        if (impactAnimator == null) impactAnimator = GetComponentInParent<Animator>();
        if (impactAnimator == null) impactAnimator = GetComponentInChildren<Animator>();
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            if (game == null) game = root.GetComponentInChildren<GameManager>(true);
            if (table == null)
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                    if (item.name == "BaseLvl_1") table = item;
        }
        CacheTrigger();
    }

    private void CacheTrigger()
    {
        hasImpactTrigger = false;
        if (impactAnimator == null || impactAnimator.runtimeAnimatorController == null) return;
        foreach (AnimatorControllerParameter parameter in impactAnimator.parameters)
            if (parameter.name == impactTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
            { hasImpactTrigger = true; break; }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount > 0) TryHit(collision.rigidbody, collision.GetContact(0).point);
    }

    public bool TryHit(Rigidbody ball, Vector3 contactPoint)
    {
        if (!isActiveAndEnabled || game == null || !game.IsPlaying || ball == null || ball.isKinematic
            || !ball.CompareTag(ballTag) || ScoreManager.Instance == null || !ScoreManager.Instance.BallInPlay
            || Time.time < nextHitTime) return false;
        nextHitTime = Time.time + hitCooldown;
        Vector3 normal = table != null ? table.up : Vector3.up;
        Vector3 direction = Vector3.ProjectOnPlane(ball.worldCenterOfMass - contactPoint, normal);
        if (direction.sqrMagnitude < .0001f) direction = Vector3.ProjectOnPlane(-ball.linearVelocity, normal);
        if (direction.sqrMagnitude > .0001f) ball.AddForce(direction.normalized * bounceForce, ForceMode.Impulse);
        OnTargetHit();
        return true;
    }

    protected virtual void OnTargetHit()
    {
        ScoreManager.Instance.AddScore(Mathf.Max(1, scoreValue));
        if (impactAnimator != null && impactAnimator.isActiveAndEnabled && hasImpactTrigger)
        {
            impactAnimator.ResetTrigger(impactTrigger);
            impactAnimator.SetTrigger(impactTrigger);
        }
    }
}

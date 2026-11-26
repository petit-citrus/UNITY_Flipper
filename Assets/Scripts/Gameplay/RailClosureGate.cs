using System.Collections;
using UnityEngine;

public class RailClosureGate : MonoBehaviour
{
    [SerializeField] private LifeManager lifeManager;
    [SerializeField] private BoxCollider passageTrigger;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Quaternion openRotation;
    [SerializeField] private Vector3 openScale;
    [SerializeField] private Vector3 raisedPosition;
    [SerializeField] private Quaternion raisedRotation;
    [SerializeField] private Vector3 raisedScale;
    [SerializeField, Min(0.01f)] private float transitionDuration = 0.35f;

    private Rigidbody body;
    private Coroutine transition;
    private bool isRaised;

    public void Configure(
        LifeManager manager,
        BoxCollider trigger,
        Vector3 restPosition,
        Quaternion restRotation,
        Vector3 restScale,
        Vector3 targetPosition,
        Quaternion targetRotation,
        Vector3 targetScale)
    {
        UnsubscribeFromLifeManager();
        lifeManager = manager;
        passageTrigger = trigger;
        openPosition = restPosition;
        openRotation = restRotation;
        openScale = restScale;
        raisedPosition = targetPosition;
        raisedRotation = targetRotation;
        raisedScale = targetScale;
        SubscribeToLifeManager();
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        SubscribeToLifeManager();
    }

    private void OnDisable()
    {
        UnsubscribeFromLifeManager();
        if (transition != null) StopCoroutine(transition);
        transition = null;
    }

    private void SubscribeToLifeManager()
    {
        if (isActiveAndEnabled && lifeManager != null)
            lifeManager.OnBallLost += ReturnToOpenPosition;
    }

    private void UnsubscribeFromLifeManager()
    {
        if (lifeManager != null)
            lifeManager.OnBallLost -= ReturnToOpenPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (passageTrigger == null || other == null) return;

        Rigidbody ball = other.attachedRigidbody;
        if (ball == null || (!other.CompareTag("Ball") && !ball.CompareTag("Ball"))) return;

        float minimumHeight = passageTrigger.center.y - passageTrigger.size.y * 0.5f - 0.2f;
        if (transform.InverseTransformPoint(ball.worldCenterOfMass).y < minimumHeight) return;
        MoveToRaisedPosition();
    }

    private void MoveToRaisedPosition()
    {
        if (isRaised) return;
        isRaised = true;
        StartTransition(raisedPosition, raisedRotation, raisedScale);
    }

    private void ReturnToOpenPosition()
    {
        isRaised = false;
        StartTransition(openPosition, openRotation, openScale);
    }

    private void StartTransition(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(TransitionTo(position, rotation, scale));
    }

    private IEnumerator TransitionTo(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, transitionDuration);

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            Vector3 nextPosition = Vector3.LerpUnclamped(startPosition, position, amount);
            Quaternion nextRotation = Quaternion.SlerpUnclamped(startRotation, rotation, amount);

            if (body != null)
            {
                body.MovePosition(nextPosition);
                body.MoveRotation(nextRotation);
            }
            else
            {
                transform.SetPositionAndRotation(nextPosition, nextRotation);
            }

            transform.localScale = Vector3.LerpUnclamped(startScale, scale, amount);
            yield return new WaitForFixedUpdate();
        }

        if (body != null)
        {
            body.MovePosition(position);
            body.MoveRotation(rotation);
        }
        else
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        transform.localScale = scale;
        transition = null;
    }
}

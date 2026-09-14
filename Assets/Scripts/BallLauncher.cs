using UnityEngine;
using UnityEngine.InputSystem;

public class BallLauncher : MonoBehaviour
{
    [Header("References")]
    public Transform cube; // if empty, uses its own transform

    [Header("Input")]
    public InputActionReference kickAction; // assign the "Kick" action asset here

    [Header("Kick Direction (in local space)")]
    public Vector3 kickDirection = Vector3.forward;

    [Header("Size")]
    public Vector3 kickSize = new Vector3(1f, 1f, 2f); // axis that stretches

    [Header("Animation Speed")]
    public float expansionSpeed = 15f;
    public float returnSpeed = 8f;

    [Header("Kick Force")]
    public float kickForce = 15f;
    public float verticalForce = 2f;

    [Header("Ball Detection")]
    public float detectionRadius = 1.5f;
    public LayerMask ballLayer;

    private Vector3 normalSize;
    private Vector3 targetSize;
    private bool kicking = false;

    private Vector3 basePosition;    // position of the BACK FACE, fixed
    private Vector3 localDirection;  // normalized kick direction

    void Awake()
    {
        if (cube == null)
            cube = transform;
    }

    void OnEnable()
    {
        if (kickAction != null)
        {
            kickAction.action.Enable();
            kickAction.action.performed += OnKickPerformed;
        }
    }

    void OnDisable()
    {
        if (kickAction != null)
        {
            kickAction.action.performed -= OnKickPerformed;
            kickAction.action.Disable();
        }
    }

    void Start()
    {
        normalSize = cube.localScale;
        targetSize = normalSize;

        localDirection = kickDirection.normalized;

        // Back face position = current center minus half the size in the kick direction
        basePosition = cube.localPosition - Vector3.Scale(localDirection * 0.5f, normalSize);
    }

    void Update()
    {
        float speed = kicking ? expansionSpeed : returnSpeed;

        cube.localScale = Vector3.MoveTowards(
            cube.localScale,
            targetSize,
            speed * Time.deltaTime
        );

        // Repositions to keep the back face fixed and grow only forward
        cube.localPosition = basePosition + Vector3.Scale(localDirection * 0.5f, cube.localScale);

        if (kicking && Vector3.Distance(cube.localScale, kickSize) < 0.01f)
        {
            kicking = false;
            targetSize = normalSize;
        }
    }

    void OnKickPerformed(InputAction.CallbackContext context)
    {
        Kick();
    }

    void Kick()
    {
        kicking = true;
        targetSize = kickSize;

        Vector3 worldDirection = transform.TransformDirection(localDirection);

        Vector3 detectionCenter = transform.position + worldDirection * (detectionRadius * 0.5f);

        Collider[] hits = Physics.OverlapSphere(
            detectionCenter,
            detectionRadius,
            ballLayer
        );

        foreach (Collider col in hits)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null)
            {
                rb.AddForce(
                    worldDirection * kickForce + Vector3.up * verticalForce,
                    ForceMode.Impulse
                );
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 dir = transform.TransformDirection(kickDirection.normalized);
        Vector3 center = transform.position + dir * (detectionRadius * 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, detectionRadius);
    }
}
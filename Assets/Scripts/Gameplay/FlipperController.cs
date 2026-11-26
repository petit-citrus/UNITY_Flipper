using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform leftFlipper;
    public Transform rightFlipper;

    [Header("Mouvement des palettes")]
    [Header("Axe de rotation")]
    [Tooltip("Objet de la table (son axe 'haut' sert d'axe de rotation). Laisser vide pour utiliser l'axe Y du monde.")]
    public Transform table;

    [Tooltip("Cochez si une palette tourne dans le mauvais sens")]
    public bool invertDirection = false;
    [Min(0f)] public float rotationSpeed = 500f;
    [Min(0f)] public float returnSpeed = 250f;
    [Min(0f)] public float activeAngle = 45f;

    [Header("Commandes")]
    public InputActionReference leftFlipAction;
    public InputActionReference rightFlipAction;

    [Header("Audio")]
    [SerializeField] private AudioClip flipperSound;

    private sealed class FlipperMotion
    {
        public Rigidbody body;
        public Vector3 pivotPosition;
        public Vector3 initialPosition;
        public Quaternion initialRotation;
        public float swingDirection;
    }

    private FlipperMotion leftMotion;
    private FlipperMotion rightMotion;

    private void OnEnable()
    {
        leftFlipAction?.action?.Enable();
        rightFlipAction?.action?.Enable();
    }

    private void OnDisable()
    {
        leftFlipAction?.action?.Disable();
        rightFlipAction?.action?.Disable();
    }

    private void Start()
    {
        leftMotion = CreateFlipperMotion(leftFlipper, rightFlipper, true, -1f);
        rightMotion = CreateFlipperMotion(rightFlipper, leftFlipper, false, 1f);
    }

    private void FixedUpdate()
    {
        bool leftPressed = leftFlipAction?.action?.IsPressed() == true;
        bool rightPressed = rightFlipAction?.action?.IsPressed() == true;

        MoveFlipper(leftMotion, leftPressed);
        MoveFlipper(rightMotion, rightPressed);
    }

    private void Update()
    {
        if (flipperSound == null)
            return;

        bool leftPressed = leftFlipAction?.action?.WasPressedThisFrame() == true;
        bool rightPressed = rightFlipAction?.action?.WasPressedThisFrame() == true;
        if (!leftPressed && !rightPressed)
            return;

        Transform soundPosition = leftPressed ? leftFlipper : null;
        if (soundPosition == null && rightPressed)
            soundPosition = rightFlipper;

        if (soundPosition != null)
            AudioSource.PlayClipAtPoint(flipperSound, soundPosition.position, .8f);
    }

    private FlipperMotion CreateFlipperMotion(
        Transform flipper,
        Transform otherFlipper,
        bool useMinX,
        float fallbackDirection)
    {
        if (flipper == null)
            return null;

        BoxCollider colliderPalette = flipper.GetComponentInChildren<BoxCollider>();

        if (colliderPalette == null)
        {
            Debug.LogWarning($"Ajoutez un BoxCollider à la palette {flipper.name}.", flipper);
            return null;
        }

        Transform repere = colliderPalette.transform;

        // Mesurer la longueur des axes X et Z en tenant compte de l'échelle.
        Vector3 demiAxeX = repere.TransformVector(
            Vector3.right * colliderPalette.size.x * 0.5f);

        Vector3 demiAxeZ = repere.TransformVector(
            Vector3.forward * colliderPalette.size.z * 0.5f);

        Vector3 demiLongueur = demiAxeX.sqrMagnitude >= demiAxeZ.sqrMagnitude
            ? demiAxeX
            : demiAxeZ;

        Vector3 centre = repere.TransformPoint(colliderPalette.center);
        Vector3 pointeA = centre - demiLongueur;
        Vector3 pointeB = centre + demiLongueur;

        Vector3 pivotPosition = useMinX ? pointeA : pointeB;
        Vector3 tipPosition = useMinX ? pointeB : pointeA;

        if (otherFlipper != null)
        {
            float distanceA = (pointeA - otherFlipper.position).sqrMagnitude;
            float distanceB = (pointeB - otherFlipper.position).sqrMagnitude;
            bool usePointA = distanceA >= distanceB;
            pivotPosition = usePointA ? pointeA : pointeB;
            tipPosition = usePointA ? pointeB : pointeA;
        }

        Vector3 axis = GetRotationAxis();
        Vector3 arm = Vector3.ProjectOnPlane(tipPosition - pivotPosition, axis);
        Vector3 towardOther = otherFlipper != null
            ? Vector3.ProjectOnPlane(otherFlipper.position - pivotPosition, axis)
            : Vector3.zero;
        float directionDot = Vector3.Dot(Vector3.Cross(axis, arm), towardOther);
        float swingDirection = Mathf.Abs(directionDot) > 0.0001f
            ? Mathf.Sign(directionDot)
            : fallbackDirection;

        Rigidbody body = flipper.GetComponent<Rigidbody>();
        if (body == null)
            body = flipper.gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        return new FlipperMotion
        {
            body = body,
            pivotPosition = pivotPosition,
            initialPosition = flipper.position,
            initialRotation = flipper.rotation,
            swingDirection = swingDirection
        };
    }

    private Vector3 GetRotationAxis()
    {
        Vector3 axis = table != null ? table.up : Vector3.up;
        if (axis.sqrMagnitude < 0.0001f)
            return Vector3.up;
        return axis.normalized;
    }

    private void MoveFlipper(FlipperMotion motion, bool pressed)
    {
        if (motion == null)
            return;

        float angle = pressed ? motion.swingDirection * activeAngle : 0f;
        if (invertDirection) angle = -angle;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, GetRotationAxis())
            * motion.initialRotation;
        float speed = returnSpeed;

        if (pressed)
        {
            speed = rotationSpeed;
        }

        Quaternion nextRotation = Quaternion.RotateTowards(
            motion.body.rotation,
            targetRotation,
            speed * Time.fixedDeltaTime
        );

        Quaternion rotationFromRest = nextRotation * Quaternion.Inverse(motion.initialRotation);
        Vector3 nextPosition = motion.pivotPosition
            + rotationFromRest * (motion.initialPosition - motion.pivotPosition);
        motion.body.MovePosition(nextPosition);
        motion.body.MoveRotation(nextRotation);
    }
}
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

    private Rigidbody leftPivot;
    private Rigidbody rightPivot;

    private Quaternion leftInitialRotation;
    private Quaternion rightInitialRotation;

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
        leftPivot = CreatePivotAtEnd(leftFlipper, true);
        rightPivot = CreatePivotAtEnd(rightFlipper, false);

        if (leftPivot != null)
            leftInitialRotation = leftPivot.rotation;

        if (rightPivot != null)
            rightInitialRotation = rightPivot.rotation;
    }

    private void FixedUpdate()
    {
        bool leftPressed = leftFlipAction?.action?.IsPressed() == true;
        bool rightPressed = rightFlipAction?.action?.IsPressed() == true;

        MoveFlipper(leftPivot, leftInitialRotation, -activeAngle, leftPressed);
        MoveFlipper(rightPivot, rightInitialRotation, activeAngle, rightPressed);
    }

    private Rigidbody CreatePivotAtEnd(Transform flipper, bool useMinX)
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

        Transform autrePalette = useMinX ? rightFlipper : leftFlipper;
        Vector3 pivotPosition = useMinX ? pointeA : pointeB;

        // Choisir l'extrémité la plus éloignée de l'autre palette.
        if (autrePalette != null)
        {
            float distanceA = (pointeA - autrePalette.position).sqrMagnitude;
            float distanceB = (pointeB - autrePalette.position).sqrMagnitude;
            pivotPosition = distanceA >= distanceB ? pointeA : pointeB;
        }

        Transform pivot = new GameObject(flipper.name + "_Pivot").transform;
        pivot.SetParent(flipper.parent);
        pivot.SetPositionAndRotation(pivotPosition, flipper.rotation);
        flipper.SetParent(pivot, true);

        Rigidbody body = pivot.gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        return body;
    }

    private void MoveFlipper(
        Rigidbody pivot,
        Quaternion initialRotation,
        float angle,
        bool pressed)
    {
        if (pivot == null)
            return;

        // Axe vertical de la table (en espace monde)
        Vector3 axis = table != null ? table.up : Vector3.up;

        if (invertDirection)
            angle = -angle;

        Quaternion targetRotation = initialRotation;
        float speed = returnSpeed;

        if (pressed)
        {
            // Rotation autour de l'axe de la table, indépendante de l'orientation du modèle
            targetRotation = Quaternion.AngleAxis(angle, axis) * initialRotation;
            speed = rotationSpeed;
        }

        Quaternion nextRotation = Quaternion.RotateTowards(
            pivot.rotation,
            targetRotation,
            speed * Time.fixedDeltaTime
        );

        pivot.MoveRotation(nextRotation);
    }


}
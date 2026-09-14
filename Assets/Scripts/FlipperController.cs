using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform leftFlipper;
    public Transform rightFlipper;

    public float rotationSpeed = 500f;
    public float activeAngle = 45f;

    [Header("Input")]
    public InputActionReference leftFlipAction;
    public InputActionReference rightFlipAction;

    private Transform leftPivot;
    private Transform rightPivot;

    private Quaternion leftInitialRotation;
    private Quaternion rightInitialRotation;

    void OnEnable()
    {
        if (leftFlipAction != null)
            leftFlipAction.action.Enable();

        if (rightFlipAction != null)
            rightFlipAction.action.Enable();
    }

    void OnDisable()
    {
        if (leftFlipAction != null)
            leftFlipAction.action.Disable();

        if (rightFlipAction != null)
            rightFlipAction.action.Disable();
    }

    void Start()
    {
        // Pivots aux extrémités EXTÉRIEURES
        // Gauche = min X
        // Droite = max X
        leftPivot = CreatePivotAtEnd(leftFlipper, true);
        rightPivot = CreatePivotAtEnd(rightFlipper, false);

        if (leftPivot != null)
            leftInitialRotation = leftPivot.localRotation;

        if (rightPivot != null)
            rightInitialRotation = rightPivot.localRotation;
    }

    void Update()
    {
        // Flipper gauche
        bool leftPressed = leftFlipAction != null && leftFlipAction.action.IsPressed();

        if (leftPressed)
        {
            RotateFlipper(
                leftPivot,
                leftInitialRotation,
                -activeAngle
            );
        }
        else
        {
            ReturnToInitialRotation(
                leftPivot,
                leftInitialRotation
            );
        }

        // Flipper droit
        bool rightPressed = rightFlipAction != null && rightFlipAction.action.IsPressed();

        if (rightPressed)
        {
            RotateFlipper(
                rightPivot,
                rightInitialRotation,
                activeAngle
            );
        }
        else
        {
            ReturnToInitialRotation(
                rightPivot,
                rightInitialRotation
            );
        }
    }

    Transform CreatePivotAtEnd(Transform flipper, bool useMinX)
    {
        if (flipper == null)
            return null;

        // Criar o pivot
        GameObject pivotObject = new GameObject(flipper.name + "_Pivot");
        Transform pivot = pivotObject.transform;

        // Salvar posição e rotação originais
        Vector3 originalPosition = flipper.position;
        Quaternion originalRotation = flipper.rotation;

        MeshFilter meshFilter = flipper.GetComponent<MeshFilter>();

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            // Bounds em ESPAÇO LOCAL do mesh (não sofre distorção por rotação)
            Bounds localBounds = meshFilter.sharedMesh.bounds;

            float xLocal = useMinX ? localBounds.min.x : localBounds.max.x;

            // Centro da extremidade: X na ponta, Y e Z no centro (em local space)
            Vector3 localPoint = new Vector3(
                xLocal,
                localBounds.center.y,
                localBounds.center.z
            );

            // Converte o ponto local para posição real no mundo,
            // respeitando a rotação e escala atuais do flipper
            pivot.position = flipper.TransformPoint(localPoint);
        }
        else
        {
            // Fallback: usa Renderer.bounds (menos preciso se houver rotação)
            Renderer renderer = flipper.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                float xPosition = useMinX ? bounds.min.x : bounds.max.x;

                pivot.position = new Vector3(
                    xPosition,
                    bounds.center.y,
                    bounds.center.z
                );
            }
            else
            {
                pivot.position = originalPosition;
            }
        }

        // Mesma rotação que o flipper
        pivot.rotation = originalRotation;

        // Manter o mesmo parent
        pivot.SetParent(flipper.parent);

        // Colocar o flipper sob o pivot
        flipper.SetParent(pivot);

        // Conservar exatamente a posição e rotação iniciais
        flipper.position = originalPosition;
        flipper.rotation = originalRotation;

        return pivot;
    }

    void RotateFlipper(
        Transform pivot,
        Quaternion initialRotation,
        float angle
    )
    {
        if (pivot == null)
            return;

        Quaternion targetRotation =
            initialRotation *
            Quaternion.Euler(0f, angle, 0f);

        pivot.localRotation = Quaternion.RotateTowards(
            pivot.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void ReturnToInitialRotation(
        Transform pivot,
        Quaternion initialRotation
    )
    {
        if (pivot == null)
            return;

        pivot.localRotation = Quaternion.RotateTowards(
            pivot.localRotation,
            initialRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
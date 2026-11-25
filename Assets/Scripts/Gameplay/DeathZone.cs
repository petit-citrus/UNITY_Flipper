using UnityEngine;

/// <summary>
/// Zona (trigger) por baixo da mesa. Avisa o LifeManager quando a bola entra.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DeathZone : MonoBehaviour
{
    [SerializeField] private LifeManager lifeManager;
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private Transform table;

    public void Configure(LifeManager manager, Transform board = null)
    {
        lifeManager = manager;
        table = board;
        debugLogs = false;
        GetComponent<Collider>().isTrigger = true;
    }

    public static bool IsDrainPosition(float boardZ) { return boardZ <= -15.55f; }

    private void Start()
    {
        if (table != null) return;
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                if (item.name == "BaseLvl_1") table = item;
    }

    // Chamado ao adicionar o componente: já deixa o collider como Trigger.
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // Desenha a zona a vermelho no Scene view.
    private void OnDrawGizmos()
    {
        Collider zone = GetComponent<Collider>();
        if (zone == null)
            return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawCube(zone.bounds.center, zone.bounds.size);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled || table == null || other.attachedRigidbody == null) return;
        Vector3 local = Quaternion.Inverse(table.rotation) * (other.attachedRigidbody.worldCenterOfMass - table.position);
        if (!IsDrainPosition(local.z)) return;
        if (debugLogs)
            Debug.Log("DeathZone : '" + other.name + "' entrou (tag " + other.tag + ").", this);

        if (lifeManager == null)
        {
            Debug.LogWarning("DeathZone : LifeManager não atribuído!", this);
            return;
        }

        lifeManager.HandleBallLost(other);
    }
}

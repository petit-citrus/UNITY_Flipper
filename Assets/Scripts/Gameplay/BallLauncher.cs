using UnityEngine;
using UnityEngine.InputSystem;

public class BallLauncher : MonoBehaviour
{
    [Header("Références")]
    public Rigidbody balle;
    public Transform pointDeLancement;

    [Header("Commande")]
    public InputActionReference actionLancer;

    [Header("Réglages du lancement")]
    [Min(0f)] public float impulsion = 12f;
    [Min(0.01f)] public float rayonDeLancement = 0.5f;

    private InputAction commandeLancer;
    private bool commandeActiveeParLeLanceur;

    private void OnEnable()
    {
        commandeLancer = actionLancer != null ? actionLancer.action : null;

        if (commandeLancer != null && !commandeLancer.enabled)
        {
            commandeLancer.Enable();
            commandeActiveeParLeLanceur = true;
        }
    }

    private void OnDisable()
    {
        if (commandeActiveeParLeLanceur)
            commandeLancer.Disable();

        commandeActiveeParLeLanceur = false;
    }

    private void Update()
    {
        if (commandeLancer == null || !commandeLancer.WasPressedThisFrame())
            return;

        if (balle == null || pointDeLancement == null)
            return;

        // Vérifier que la balle est en position de départ.
        Vector3 ecart = balle.worldCenterOfMass - pointDeLancement.position;
        if (ecart.sqrMagnitude > rayonDeLancement * rayonDeLancement)
            return;

        // Lancer la balle dès l'appui, dans le sens de l'axe Z local.
        balle.AddForce(pointDeLancement.forward * impulsion, ForceMode.Impulse);
    }
}
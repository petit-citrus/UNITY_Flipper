using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Couloir de lancement manuel ou relance automatique en cours de partie, toujours avec la même balle.</summary>
public class BallLauncher : MonoBehaviour
{
    public Rigidbody balle;
    public Transform pointDeLancement;
    public InputActionReference actionLancer;
    [Min(0f)] public float impulsion = 20f;
    [Min(.01f)] public float rayonDeLancement = .65f;
    public bool automatic;
    [Min(.1f)] public float cooldown = .6f;
    [SerializeField] private AudioClip launchSound;
    private InputAction commandeLancer;
    private bool commandeActiveeParLeLanceur;
    private GameManager game;
    private float nextLaunchTime;

    private void OnEnable()
    {
        commandeLancer = actionLancer != null ? actionLancer.action : null;
        if (commandeLancer != null && !commandeLancer.enabled)
        { commandeLancer.Enable(); commandeActiveeParLeLanceur = true; }
    }

    private void Start()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            if (game == null) game = root.GetComponentInChildren<GameManager>(true);
    }

    private void OnDisable()
    {
        if (commandeActiveeParLeLanceur && commandeLancer != null) commandeLancer.Disable();
        commandeActiveeParLeLanceur = false;
    }

    public static float ForwardImpulse(float currentSpeed, float configuredImpulse, float mass, bool auto)
    {
        return auto ? System.Math.Max(0, configuredImpulse - currentSpeed) * mass : configuredImpulse;
    }

    private void Update()
    {
        if (!automatic && commandeLancer != null && commandeLancer.WasPressedThisFrame()) TryLaunch();
    }

    private void FixedUpdate()
    {
        if (automatic) TryLaunch();
    }

    private void TryLaunch()
    {
        if (balle == null || pointDeLancement == null || balle.isKinematic
            || (game != null && !game.IsPlaying) || Time.time < nextLaunchTime) return;
        Vector3 offset = balle.worldCenterOfMass - pointDeLancement.position;
        if (offset.sqrMagnitude > rayonDeLancement * rayonDeLancement) return;
        float forwardSpeed = Vector3.Dot(balle.linearVelocity, pointDeLancement.forward);
        float force = ForwardImpulse(forwardSpeed, impulsion, balle.mass, automatic);
        nextLaunchTime = Time.time + cooldown;
        if (force > 0)
        {
            balle.AddForce(pointDeLancement.forward * force, ForceMode.Impulse);
            if (!automatic && launchSound != null)
                AudioSource.PlayClipAtPoint(launchSound, pointDeLancement.position, .8f);
            if (ScoreManager.Instance != null) ScoreManager.Instance.BeginBallInPlay(balle);
        }
    }
}

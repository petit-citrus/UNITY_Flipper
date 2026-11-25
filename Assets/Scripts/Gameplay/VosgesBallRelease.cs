using System.Collections.Generic;
using UnityEngine;

/// <summary>A gentle recovery only for a launched ball stalled against a boundary.</summary>
[RequireComponent(typeof(Rigidbody))]
public class VosgesBallRelease : MonoBehaviour
{
    [SerializeField] private Transform table;
    [SerializeField] private GameManager game;
    [SerializeField] private LifeManager lives;
    private Rigidbody ball;
    private Vector3 spawn, contactNormal;
    private Vector3 lastSafePosition;
    private Collider ballCollider;
    private readonly HashSet<Collider> wallContacts = new HashSet<Collider>();
    private bool launched;
    private float stalled;

    public void Configure(Transform board, GameManager manager, LifeManager lifeManager = null)
    { table = board; game = manager; lives = lifeManager; }

    private void Start()
    {
        ball = GetComponent<Rigidbody>();
        spawn = ball.position;
        lastSafePosition = spawn;
        ballCollider = GetComponent<Collider>();
        ball.maxAngularVelocity = 50;
    }

    private void OnCollisionEnter(Collision collision) { Track(collision); }
    private void OnCollisionStay(Collision collision) { Track(collision); }
    private void OnCollisionExit(Collision collision) { wallContacts.Remove(collision.collider); }

    private void Track(Collision collision)
    {
        if (table == null) return;
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (Mathf.Abs(Vector3.Dot(normal, table.up)) >= .65f) continue;
            wallContacts.Add(collision.collider);
            contactNormal = Vector3.ProjectOnPlane(normal, table.up).normalized;
            return;
        }
        wallContacts.Remove(collision.collider);
    }

    private void FixedUpdate()
    {
        if (table == null || game == null || ball == null) return;
        if (!game.IsPlaying || ball.isKinematic)
        {
            stalled = 0;
            if (game.State == GameState.Ready || game.State == GameState.GameOver) launched = false;
            return;
        }
        if (ball.collisionDetectionMode != CollisionDetectionMode.ContinuousDynamic)
            ball.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Vector3 local = Quaternion.Inverse(table.rotation) * (ball.position - table.position);
        if (local.z < -15.6f && lives != null && ballCollider != null)
        {
            lives.HandleBallLost(ballCollider);
            stalled = 0;
            return;
        }
        // An exceptional physics escape must not leave the ball lost outside the view.
        // Normal drains above still go through the existing life/respawn system.
        if (local.y < -.45f || local.y > 1.35f || local.x < -6.9f || local.x > 7.0f || local.z > 7.75f)
        {
            ball.position = lastSafePosition;
            ball.linearVelocity = -table.forward;
            ball.angularVelocity = Vector3.zero;
            ball.WakeUp();
            wallContacts.Clear();
            stalled = 0;
            return;
        }
        if (local.y > .15f && local.y < .8f && local.x > -5.85f && local.x < 5.95f
            && local.z > -14.9f && local.z < 6.45f)
            lastSafePosition = ball.position;
        float speed = Vector3.ProjectOnPlane(ball.linearVelocity, table.up).magnitude;
        float distanceFromSpawn = Vector3.Distance(ball.position, spawn);
        if (distanceFromSpawn < .65f && speed < .2f)
        {
            launched = false;
            stalled = 0;
            wallContacts.Clear();
            return;
        }
        if (distanceFromSpawn > 1 && speed > .75f) launched = true;
        bool boundary = local.x < -5.25f || local.x > 3.8f || local.z > 5.5f;
        if (!launched || !boundary || wallContacts.Count == 0 || speed > .16f)
        { stalled = 0; return; }
        stalled += Time.fixedDeltaTime;
        if (stalled < 1.6f) return;
        Vector3 direction = Vector3.ProjectOnPlane(Physics.gravity, table.up).normalized * 1.25f + contactNormal;
        if (direction.sqrMagnitude < .01f) direction = -table.forward;
        ball.WakeUp();
        ball.AddForce(direction.normalized * .85f, ForceMode.Impulse);
        stalled = 0;
    }

    private void OnDisable() { wallContacts.Clear(); stalled = 0; }
}

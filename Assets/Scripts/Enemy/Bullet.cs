using UnityEngine;

// This script controls enemy bullets
// It makes the bullet move, hit the player, hit walls, and delete itself
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour {
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float maxTravelDistance = 20f;
    [SerializeField] private float deleteTime = 5f;

    [Header("Hitbox")]
    [SerializeField] private float hitboxRadius = 0.35f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask solidMask;

    // This stores the Rigidbody so the bullet can move with physics
    private Rigidbody rb;

    // This stores where the bullet started
    private Vector3 startPosition;

    // This stores which direction the bullet should fly
    private Vector3 moveDirection;

    // This prevents the bullet from hitting the player more than once
    private bool hasHitPlayer;

    void Awake() {
        // Gets the Rigidbody on this bullet
        rb = GetComponent<Rigidbody>();

        // Bullets should not fall down from gravity
        rb.useGravity = false;

        // Bullets should not spin around when moving
        rb.freezeRotation = true;

        // Helps the bullet not pass through objects when moving fast
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start() {
        // Saves where the bullet started
        startPosition = transform.position;

        // Deletes the bullet after a few seconds no matter what
        Destroy(gameObject, deleteTime);
    }

    void Update() {
        // Checks if the bullet hit the player
        CheckPlayerHitbox();

        // Checks if the bullet went too far
        CheckMaxDistance();

        // Checks if the bullet is about to hit a wall or solid object
        CheckSolidAhead();
    }

    void FixedUpdate() {
        // Moves the bullet forward using Rigidbody velocity
        rb.velocity = moveDirection * speed;
    }

    public void SetDirection(Vector3 direction) {
        // Saves the direction and normalizes it so the length is 1
        moveDirection = direction.normalized;

        // Makes sure the direction is big enough to rotate toward
        if (moveDirection.sqrMagnitude > 0.001f) {
            // Rotates the bullet so it faces the direction it is flying
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    public void SetDamage(int newDamage) {
        // Sets how much damage this bullet does
        damage = newDamage;
    }

    private void CheckPlayerHitbox() {
        // If the bullet already hit the player, do nothing
        if (hasHitPlayer) {
            return;
        }

        // Makes a small invisible sphere around the bullet
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            hitboxRadius,
            playerMask,
            QueryTriggerInteraction.Ignore
        );

        // Goes through everything inside the bullet hitbox
        foreach (Collider hit in hits) {
            // Tries to find PlayerReceiveDamage on the object or its parent
            PlayerReceiveDamage playerDamage = hit.GetComponentInParent<PlayerReceiveDamage>();

            // If the player damage script was found, damage the player
            if (playerDamage != null) {
                // Mark that the bullet already hit
                hasHitPlayer = true;

                // Damage the player
                playerDamage.Hit(damage);

                // Destroy the bullet after hitting
                Destroy(gameObject);

                // Stop the function so it does not keep checking
                return;
            }
        }
    }

    private void CheckMaxDistance() {
        // Checks how far the bullet has traveled from where it started
        if (Vector3.Distance(startPosition, transform.position) >= maxTravelDistance) {
            // Deletes the bullet if it went too far
            Destroy(gameObject);
        }
    }

    private void CheckSolidAhead() {
        // If the bullet has no direction, do nothing
        if (moveDirection.sqrMagnitude < 0.001f) {
            return;
        }

        // Checks a small distance ahead based on bullet speed
        float checkDistance = speed * Time.deltaTime;

        // SphereCast checks ahead so the bullet does not skip through walls
        if (Physics.SphereCast(
            transform.position,
            hitboxRadius,
            moveDirection,
            out RaycastHit hit,
            checkDistance,
            solidMask,
            QueryTriggerInteraction.Ignore)) {
            // Destroy the bullet when it hits a wall or solid object
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected() {
        // Makes the gizmo red
        Gizmos.color = Color.red;

        // Draws the bullet hitbox in the Scene view
        Gizmos.DrawWireSphere(transform.position, hitboxRadius);
    }
}
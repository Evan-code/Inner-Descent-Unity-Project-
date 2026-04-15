using UnityEngine;

// This script controls an enemy projectile.
// It moves forward in one direction, ignores gravity,
// damages the player on contact,
// and destroys itself if it travels too far or hits something solid.
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float maxTravelDistance = 20f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Vector3 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Make sure gravity does not affect the bullet
        rb.useGravity = false;

        // Freeze bullet rotation so physics does not make it spin weirdly
        rb.freezeRotation = true;

        // Better collision detection for fast projectiles
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start()
    {
        // Save where the bullet started so we can delete it later
        startPosition = transform.position;
    }

    void Update()
    {
        // Delete the bullet if it has traveled too far
        float distanceTravelled = Vector3.Distance(startPosition, transform.position);

        if (distanceTravelled >= maxTravelDistance)
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        // Move the bullet in a straight line every physics frame
        rb.velocity = moveDirection * speed;
    }

    // This is called by EnemyShoot right after the bullet is created
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;

        // Face the direction the bullet is traveling
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    // Trigger collision version
    void OnTriggerEnter(Collider other)
    {
        TryHitObject(other);
    }

    // Non-trigger collision version
    void OnCollisionEnter(Collision collision)
    {
        TryHitObject(collision.collider);
    }

    void TryHitObject(Collider other)
    {
        // Ignore other triggers so bullets do not vanish on helper colliders
        if (other.isTrigger)
            return;

        // Check if the bullet hit the player
        PlayerReceiveDamage playerDamage = other.GetComponentInParent<PlayerReceiveDamage>();

        if (playerDamage != null)
        {
            playerDamage.Hit(damage);
            Destroy(gameObject);
            return;
        }

        // If it hits anything solid that is not the player, destroy it
        Destroy(gameObject);
    }
}
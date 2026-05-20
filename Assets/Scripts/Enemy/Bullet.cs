using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
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

    private Rigidbody rb;
    private Vector3 startPosition;
    private Vector3 moveDirection;
    private bool hasHitPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, deleteTime);
    }

    void Update()
    {
        CheckPlayerHitbox();
        CheckMaxDistance();
        CheckSolidAhead();
    }

    void FixedUpdate()
    {
        rb.velocity = moveDirection * speed;
    }

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;

        if (moveDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(moveDirection);
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    void CheckPlayerHitbox()
    {
        if (hasHitPlayer)
            return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            hitboxRadius,
            playerMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            PlayerReceiveDamage playerDamage = hit.GetComponentInParent<PlayerReceiveDamage>();

            if (playerDamage != null)
            {
                hasHitPlayer = true;
                playerDamage.Hit(damage);
                Destroy(gameObject);
                return;
            }
        }
    }

    void CheckMaxDistance()
    {
        if (Vector3.Distance(startPosition, transform.position) >= maxTravelDistance)
            Destroy(gameObject);
    }

    void CheckSolidAhead()
    {
        if (moveDirection.sqrMagnitude < 0.001f)
            return;

        float checkDistance = speed * Time.deltaTime;

        if (Physics.SphereCast(
            transform.position,
            hitboxRadius,
            moveDirection,
            out RaycastHit hit,
            checkDistance,
            solidMask,
            QueryTriggerInteraction.Ignore))
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitboxRadius);
    }
}
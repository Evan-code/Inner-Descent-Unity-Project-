using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalSpeed = 5f;

    [Header("Wall Collision")]
    [SerializeField] private float wallSkin = 0.05f;

    public bool IsDashing { get; set; }

    private Transform cam;
    private Animator anim;
    private Rigidbody rb;
    private PlayerCombat combat;

    void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<PlayerCombat>();

        if (anim != null)
        {
            anim.applyRootMotion = false;
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void FixedUpdate()
    {
        if (rb == null || cam == null)
            return;

        if (IsDashing)
        {
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
            }

            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector2 rawInput = new Vector2(x, z);

        if (rawInput.sqrMagnitude < 0.0001f)
        {
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
            }

            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", 1f);
        }

        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;

        Vector3 moveDirection = (camRight * x + camForward * z).normalized;

        float currentSpeed = moveSpeed;

        if (x != 0 && z == 0)
        {
            currentSpeed = moveSpeed;
        }
        else if (x == 0 && z != 0)
        {
            currentSpeed = verticalSpeed;
        }
        else if (x != 0 && z != 0)
        {
            currentSpeed = (moveSpeed + verticalSpeed) / 2f;
        }

        if (combat != null && combat.IsAttackSlowed)
        {
            currentSpeed *= combat.AttackMoveMultiplier;
        }

        Vector3 moveAmount = moveDirection * currentSpeed * Time.fixedDeltaTime;

        MoveSafely(moveAmount);

        if (moveDirection.sqrMagnitude > 0.0001f && (combat == null || !combat.IsAttacking))
        {
            transform.forward = moveDirection;
        }
    }

    void MoveSafely(Vector3 moveAmount)
    {
        Vector3 horizontalMove = new Vector3(moveAmount.x, 0f, 0f);
        Vector3 verticalMove = new Vector3(0f, 0f, moveAmount.z);

        TryMove(horizontalMove);
        TryMove(verticalMove);
    }

    void TryMove(Vector3 moveAmount)
    {
        float distance = moveAmount.magnitude;

        if (distance <= 0.0001f)
            return;

        Vector3 direction = moveAmount.normalized;

        if (rb.SweepTest(direction, out RaycastHit hit, distance + wallSkin, QueryTriggerInteraction.Ignore))
        {
            float safeDistance = Mathf.Max(hit.distance - wallSkin, 0f);
            rb.MovePosition(rb.position + direction * safeDistance);
        }
        else
        {
            rb.MovePosition(rb.position + moveAmount);
        }
    }

    public void SetMovementSpeeds(float newMoveSpeed, float newVerticalSpeed)
    {
        moveSpeed = newMoveSpeed;
        verticalSpeed = newVerticalSpeed;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetVerticalSpeed()
    {
        return verticalSpeed;
    }
}
using UnityEngine;

// This script handles the player's normal movement.
// It moves the player relative to the camera, plays movement animation,
// and slows the player during attacks.
// IMPORTANT:
// This script uses Rigidbody movement, so your dash script should ALSO
// use Rigidbody movement to avoid jitter/glitchy motion.
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float moveSpeed = 5f;      // Speed for left/right movement
    [SerializeField] private float verticalSpeed = 5f;  // Speed for forward/back movement

    // This property is used by the dash script.
    // When true, normal movement stops so dash can take over cleanly.
    public bool IsDashing { get; set; }

    // References to other important components
    private Transform cam;          // Main camera transform (used for camera-relative movement)
    private Animator anim;          // Animator for movement animation
    private Rigidbody rb;           // Rigidbody for physics-based movement
    private PlayerCombat combat;    // Combat script so movement can slow during attacks

    void Start()
    {
        // Get the main camera transform if it exists
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        // Get needed components from this GameObject
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<PlayerCombat>();

        // Turn off root motion so animation does not physically move the player
        if (anim != null)
        {
            anim.applyRootMotion = false;
        }
    }

    void FixedUpdate()
    {
        // Safety check:
        // If Rigidbody or camera is missing, do nothing
        if (rb == null || cam == null)
            return;

        // If the player is currently dashing,
        // stop normal movement so the dash script fully controls motion
        if (IsDashing)
        {
            // Set movement animation speed to 0 while dashing
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
            }

            // Stop leftover horizontal movement while keeping gravity
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        // Read keyboard input
        float x = Input.GetAxisRaw("Horizontal"); // A/D or left/right
        float z = Input.GetAxisRaw("Vertical");   // W/S or up/down

        // Store the raw 2D input
        Vector2 rawInput = new Vector2(x, z);

        // If no movement keys are being pressed, stop movement animation and horizontal motion
        if (rawInput.sqrMagnitude < 0.0001f)
        {
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
            }

            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        // If player is moving, set movement animation speed to 1
        if (anim != null)
        {
            anim.SetFloat("Speed", 1f);
        }

        // Get the camera's forward direction
        Vector3 camForward = cam.forward;

        // Remove vertical tilt so movement stays flat on the ground
        camForward.y = 0f;
        camForward.Normalize();

        // Get the camera's right direction
        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;

        // Build movement direction based on input and camera orientation
        Vector3 moveDirection = (camRight * x + camForward * z).normalized;

        // Choose speed depending on input direction
        float currentSpeed = moveSpeed;

        if (x != 0 && z == 0)
        {
            // Pure horizontal movement
            currentSpeed = moveSpeed;
        }
        else if (x == 0 && z != 0)
        {
            // Pure vertical movement
            currentSpeed = verticalSpeed;
        }
        else if (x != 0 && z != 0)
        {
            // Diagonal movement uses the average of both speeds
            currentSpeed = (moveSpeed + verticalSpeed) / 2f;
        }

        // If the player is attacking, reduce movement speed
        if (combat != null && combat.IsAttackSlowed)
        {
            currentSpeed *= combat.AttackMoveMultiplier;
        }

        // Create the final movement vector
        Vector3 moveVector = moveDirection * currentSpeed;

        // Calculate the player's next position using Rigidbody
        Vector3 newPos = rb.position + moveVector * Time.fixedDeltaTime;

        // Move using physics-friendly Rigidbody motion
        rb.MovePosition(newPos);

        // Rotate the player to face the direction they are moving
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            transform.forward = moveDirection;
        }
    }
    
        // Lets other scripts set the player's movement speeds
    public void SetMovementSpeeds(float newMoveSpeed, float newVerticalSpeed)
    {
        moveSpeed = newMoveSpeed;
        verticalSpeed = newVerticalSpeed;
    }

    // Lets other scripts read the current movement speeds
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetVerticalSpeed()
    {
        return verticalSpeed;
    }
}


using UnityEngine;

// This script controls the player's normal movement
// It moves based on camera direction and avoids sliding through walls weirdly
public class PlayerMovement : MonoBehaviour {
    [Header("Movement Speeds")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalSpeed = 5f;

    [Header("Smooth Rotation")]
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Wall Collision")]
    [SerializeField] private float wallSkin = 0.05f;

    // PlayerDash changes this so normal movement stops during dash
    public bool IsDashing { get; set; }

    // Stores the camera transform
    private Transform cam;

    // Stores the Animator
    private Animator anim;

    // Stores the Rigidbody
    private Rigidbody rb;

    // Stores the combat script so movement can slow while attacking
    private PlayerCombat combat;

    void Start() {
        // Gets the main camera transform
        if (Camera.main != null) {
            cam = Camera.main.transform;
        }

        // Gets the Animator on the player
        anim = GetComponent<Animator>();

        // Gets the Rigidbody on the player
        rb = GetComponent<Rigidbody>();

        // Gets the PlayerCombat script on the player
        combat = GetComponent<PlayerCombat>();

        // Turns off root motion because movement is handled by code
        if (anim != null) {
            anim.applyRootMotion = false;
        }

        // Sets up Rigidbody settings
        if (rb != null) {
            // Stops player from falling over
            rb.freezeRotation = true;

            // Helps prevent going through walls
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Makes movement look smoother
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void FixedUpdate() {
        // If no Rigidbody or camera exists, movement cannot work
        if (rb == null || cam == null) {
            return;
        }

        // If player is dashing, normal movement should stop
        if (IsDashing) {
            // Set animation speed to 0
            if (anim != null) {
                anim.SetFloat("Speed", 0f);
            }

            // Stop horizontal movement but keep vertical velocity
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        // Gets left/right input
        float x = Input.GetAxisRaw("Horizontal");

        // Gets forward/back input
        float z = Input.GetAxisRaw("Vertical");

        // Stores input as a Vector2 so we can check if anything is being pressed
        Vector2 rawInput = new Vector2(x, z);

        // If the player is not pressing movement keys
        if (rawInput.sqrMagnitude < 0.0001f) {
            // Tell Animator the player is not moving
            if (anim != null) {
                anim.SetFloat("Speed", 0f);
            }

            // Stop horizontal velocity
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        // Tell Animator the player is moving
        if (anim != null) {
            anim.SetFloat("Speed", 1f);
        }

        // Gets movement direction based on the camera
        Vector3 moveDirection = GetCameraBasedMoveDirection(x, z);

        // Gets speed based on horizontal, vertical, or diagonal movement
        float currentSpeed = GetCurrentSpeed(x, z);

        // If attacking, slow the player down
        if (combat != null && combat.IsAttackSlowed) {
            currentSpeed *= combat.AttackMoveMultiplier;
        }

        // Calculates movement amount for this physics frame
        Vector3 moveAmount = moveDirection * currentSpeed * Time.fixedDeltaTime;

        // Moves the player while checking walls
        MoveSafely(moveAmount);

        // Rotates the player toward movement direction
        SmoothlyRotateToward(moveDirection);
    }

    private Vector3 GetCameraBasedMoveDirection(float x, float z) {
        // Gets the camera forward direction
        Vector3 camForward = cam.forward;

        // Removes up/down tilt
        camForward.y = 0f;

        // Makes the direction length 1
        camForward.Normalize();

        // Gets the camera right direction
        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;

        // Combines camera directions with input
        return (camRight * x + camForward * z).normalized;
    }

    private float GetCurrentSpeed(float x, float z) {
        // If moving only sideways, use moveSpeed
        if (x != 0 && z == 0) {
            return moveSpeed;
        }

        // If moving only forward/back, use verticalSpeed
        if (x == 0 && z != 0) {
            return verticalSpeed;
        }

        // If moving diagonal, use an average of both speeds
        return (moveSpeed + verticalSpeed) / 2f;
    }

    private void SmoothlyRotateToward(Vector3 moveDirection) {
        // If direction is basically zero, do not rotate
        if (moveDirection.sqrMagnitude < 0.0001f) {
            return;
        }

        // If attacking, combat script rotates toward the mouse instead
        if (combat != null && combat.IsAttacking) {
            return;
        }

        // Creates the rotation we want the player to face
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        // Smooths the rotation instead of snapping
        Quaternion smoothRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        // Applies the rotation using Rigidbody
        rb.MoveRotation(smoothRotation);
    }

    private void MoveSafely(Vector3 moveAmount) {
        // Splits X movement from Z movement so wall movement feels better
        Vector3 horizontalMove = new Vector3(moveAmount.x, 0f, 0f);

        // Stores Z movement separately
        Vector3 verticalMove = new Vector3(0f, 0f, moveAmount.z);

        // Tries moving left/right
        TryMove(horizontalMove);

        // Tries moving forward/back
        TryMove(verticalMove);
    }

    private void TryMove(Vector3 moveAmount) {
        // Gets how far the player is trying to move
        float distance = moveAmount.magnitude;

        // If movement is tiny, do nothing
        if (distance <= 0.0001f) {
            return;
        }

        // Gets movement direction
        Vector3 direction = moveAmount.normalized;

        // SweepTest checks if movement would hit a wall
        if (rb.SweepTest(direction, out RaycastHit hit, distance + wallSkin, QueryTriggerInteraction.Ignore)) {
            // Calculates safe distance before touching wall
            float safeDistance = Mathf.Max(hit.distance - wallSkin, 0f);

            // Moves only to the safe distance
            rb.MovePosition(rb.position + direction * safeDistance);
        } else {
            // If no wall is hit, move normally
            rb.MovePosition(rb.position + moveAmount);
        }
    }

    public void SetMovementSpeeds(float newMoveSpeed, float newVerticalSpeed) {
        // Sets sideways speed
        moveSpeed = newMoveSpeed;

        // Sets forward/back speed
        verticalSpeed = newVerticalSpeed;
    }

    public float GetMoveSpeed() {
        // Returns current sideways speed
        return moveSpeed;
    }

    public float GetVerticalSpeed() {
        // Returns current forward/back speed
        return verticalSpeed;
    }
}
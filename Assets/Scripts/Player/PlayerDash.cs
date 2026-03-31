using System.Collections;
using UnityEngine;

// This script handles the player's dash.
// It uses Rigidbody movement instead of transform.Translate()
// so it works more smoothly with PlayerMovement.
// Dash direction is locked at the start of the dash.
public class PlayerDash : MonoBehaviour
{
    // References to other components
    private PlayerMovement moveScript; // Lets us tell normal movement to stop during dash
    private Transform cam;             // Main camera transform for camera-relative dash direction
    private Rigidbody rb;              // Rigidbody for smooth physics-based dash movement

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;              // Base dash speed
    [SerializeField] private float dashDuration = 0.15f;         // How long the dash lasts
    [SerializeField] private float dashCooldown = 1f;            // Delay before player can dash again
    [SerializeField] private float horizontalDashMultiplier = 1f; // Dash speed when moving left/right
    [SerializeField] private float verticalDashMultiplier = 1.5f; // Dash speed when moving forward/back

    [Header("Visuals")]
    [SerializeField] private Animator animator;      // Plays dash animation
    [SerializeField] private ParticleSystem dashVFX; // Dash particle effect

    private bool canDash = true; // Tracks whether dash is currently available

    void Start()
    {
        // Get references from this GameObject
        moveScript = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();

        // Get main camera if it exists
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }

    void Update()
    {
        // If player presses Left Shift and dash is ready, start dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        // Safety check:
        // If Rigidbody or camera is missing, do nothing
        if (rb == null || cam == null)
            yield break;

        // Turn off dash availability immediately so player cannot spam it
        canDash = false;

        // Tell the movement script to stop normal movement during dash
        if (moveScript != null)
        {
            moveScript.IsDashing = true;
        }

        // Play dash animation if animator exists
        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }

        // Play dash particle effect if assigned
        if (dashVFX != null)
        {
            dashVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            dashVFX.Play();
        }

        // Get flattened camera forward direction
        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        // Get flattened camera right direction
        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;

        // Read current movement input at the start of the dash
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Build dash direction from input and camera directions
        Vector3 moveDirection = camRight * x + camForward * z;

        // If there is no input, dash in the direction the player is already facing
        if (moveDirection.sqrMagnitude < 0.0001f)
        {
            moveDirection = transform.forward;
        }
        else
        {
            moveDirection.Normalize();
        }

        // Lock dash direction so it does not change during the dash
        Vector3 lockedDashDirection = moveDirection.normalized;

        // Instantly face the dash direction
        transform.forward = lockedDashDirection;

        // Decide speed multiplier based on input direction
        float speedMultiplier = 1f;

        if (x != 0 && z == 0)
        {
            // Pure left/right dash
            speedMultiplier = horizontalDashMultiplier;
        }
        else if (x == 0 && z != 0)
        {
            // Pure forward/back dash
            speedMultiplier = verticalDashMultiplier;
        }
        else if (x != 0 && z != 0)
        {
            // Diagonal dash uses a balanced value
            speedMultiplier = (horizontalDashMultiplier + verticalDashMultiplier) / 2.3f;
        }

        // If no movement keys were pressed,
        // choose multiplier based on the player's facing direction relative to camera
        if (x == 0 && z == 0)
        {
            float dotVertical = Vector3.Dot(lockedDashDirection, camForward);
            float dotHorizontal = Vector3.Dot(lockedDashDirection, camRight);

            if (Mathf.Abs(dotVertical) > Mathf.Abs(dotHorizontal))
            {
                speedMultiplier = verticalDashMultiplier + 0.2f;
            }
            else if (Mathf.Abs(dotHorizontal) > Mathf.Abs(dotVertical))
            {
                speedMultiplier = horizontalDashMultiplier + 0.1f;
            }
            else
            {
                speedMultiplier = (horizontalDashMultiplier + verticalDashMultiplier) / 2.3f;
            }
        }

        // Dash timer
        float timer = 0f;

        // Move for the dash duration
        while (timer < dashDuration)
        {
            // Increase timer every frame
            timer += Time.deltaTime;

            // Calculate the next Rigidbody position
            Vector3 newPos = rb.position + lockedDashDirection * dashSpeed * speedMultiplier * Time.deltaTime;

            // Move using Rigidbody so dash works smoothly with physics and normal movement
            rb.MovePosition(newPos);

            // Keep player facing the dash direction the whole time
            transform.forward = lockedDashDirection;

            // Wait one frame
            yield return null;
        }

        // Tell movement script dash is over
        if (moveScript != null)
        {
            moveScript.IsDashing = false;
        }

        // Wait for cooldown before allowing another dash
        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

        // Lets other scripts change dash cooldown
    public void SetDashCooldown(float newCooldown)
    {
        dashCooldown = newCooldown;
    }

    // Lets other scripts read dash cooldown
    public float GetDashCooldown()
    {
        return dashCooldown;
    }
}
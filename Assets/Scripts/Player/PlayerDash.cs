using System.Collections;
using UnityEngine;

// This script controls the player's dash
// It moves the player quickly, gives dash invincibility, and has a cooldown
public class PlayerDash : MonoBehaviour {
    // This stores the PlayerMovement script
    private PlayerMovement moveScript;

    // This stores the PlayerCombat script so dash can reset combos
    private PlayerCombat combatScript;

    // This stores the damage script so dash can turn invincibility on and off
    private PlayerReceiveDamage receiveDamageScript;

    // This stores the camera transform so dash can move based on camera direction
    private Transform cam;

    // Rigidbody is used to move the player with physics
    private Rigidbody rb;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float horizontalDashMultiplier = 1f;
    [SerializeField] private float verticalDashMultiplier = 1.5f;

    [Header("Wall Collision")]
    [SerializeField] private float wallSkin = 0.05f;

    [Header("Visuals")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem dashVFX;

    // This tells if the player is allowed to dash right now
    private bool canDash = true;

    void Start() {
        // Gets the movement script from the player
        moveScript = GetComponent<PlayerMovement>();

        // Gets the combat script from the player
        combatScript = GetComponent<PlayerCombat>();

        // Gets the damage receiving script from the player
        receiveDamageScript = GetComponent<PlayerReceiveDamage>();

        // Gets the Rigidbody from the player
        rb = GetComponent<Rigidbody>();

        // Gets the main camera if it exists
        if (Camera.main != null) {
            cam = Camera.main.transform;
        }

        // Sets up Rigidbody settings if the Rigidbody exists
        if (rb != null) {
            // Stops the player from tipping over
            rb.freezeRotation = true;

            // Helps stop the player from passing through walls
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Makes movement look smoother
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void Update() {
        // If player presses Left Shift and dash is ready, start dashing
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash) {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash() {
        // If there is no Rigidbody or camera, stop the dash
        if (rb == null || cam == null) {
            yield break;
        }

        // Player cannot dash again until cooldown finishes
        canDash = false;

        // Reset combo when dash starts
        if (combatScript != null) {
            combatScript.ResetComboFromDash();
        }

        // Make player invincible during the dash
        if (receiveDamageScript != null) {
            receiveDamageScript.SetDashInvincible(true);
        }

        // Tell movement script that dash is controlling movement right now
        if (moveScript != null) {
            moveScript.IsDashing = true;
        }

        // Play dash animation if Animator exists
        if (animator != null) {
            animator.SetTrigger("Dash");
        }

        // Play dash VFX if assigned
        if (dashVFX != null) {
            dashVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            dashVFX.Play();
        }

        // Get the direction the player should dash
        Vector3 lockedDashDirection = GetDashDirection();

        // Face the player in the dash direction
        transform.forward = lockedDashDirection;

        // Get the speed multiplier based on dash direction
        float speedMultiplier = GetDashSpeedMultiplier(lockedDashDirection);

        // Timer for how long dash lasts
        float timer = 0f;

        // Keep moving while the dash timer is active
        while (timer < dashDuration) {
            // Add time to the dash timer
            timer += Time.deltaTime;

            // Calculates how far the player should move this frame
            Vector3 moveAmount = lockedDashDirection * dashSpeed * speedMultiplier * Time.deltaTime;

            // Moves the player, but checks for walls
            bool hitWall = TryDashMove(moveAmount);

            // Keep facing dash direction during dash
            transform.forward = lockedDashDirection;

            // If the player hit a wall, stop the dash early
            if (hitWall) {
                break;
            }

            // Wait until next frame
            yield return null;
        }

        // Stops leftover horizontal velocity after dash
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);

        // Tells PlayerMovement that dash is finished
        if (moveScript != null) {
            moveScript.IsDashing = false;
        }

        // Turns off dash invincibility
        if (receiveDamageScript != null) {
            receiveDamageScript.SetDashInvincible(false);
        }

        // Wait for dash cooldown
        yield return new WaitForSeconds(dashCooldown);

        // Dash is ready again
        canDash = true;
    }

    private Vector3 GetDashDirection() {
        // Gets the camera forward direction
        Vector3 camForward = cam.forward;

        // Removes camera up/down tilt
        camForward.y = 0f;

        // Makes the direction length equal 1
        camForward.Normalize();

        // Gets the right direction from the camera
        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;

        // Gets horizontal input
        float x = Input.GetAxisRaw("Horizontal");

        // Gets vertical input
        float z = Input.GetAxisRaw("Vertical");

        // Combines input with camera directions
        Vector3 moveDirection = camRight * x + camForward * z;

        // If the player is not pressing a movement key, dash forward
        if (moveDirection.sqrMagnitude < 0.0001f) {
            moveDirection = transform.forward;
        } else {
            // Otherwise normalize the input direction
            moveDirection.Normalize();
        }

        // Return the dash direction
        return moveDirection.normalized;
    }

    private float GetDashSpeedMultiplier(Vector3 lockedDashDirection) {
        // Gets camera forward direction
        Vector3 camForward = cam.forward;

        // Removes up/down tilt
        camForward.y = 0f;

        // Normalizes camera forward
        camForward.Normalize();

        // Gets camera right direction
        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;

        // Gets horizontal input
        float x = Input.GetAxisRaw("Horizontal");

        // Gets vertical input
        float z = Input.GetAxisRaw("Vertical");

        // If only horizontal input is pressed, use horizontal multiplier
        if (x != 0 && z == 0) {
            return horizontalDashMultiplier;
        }

        // If only vertical input is pressed, use vertical multiplier
        if (x == 0 && z != 0) {
            return verticalDashMultiplier;
        }

        // If both are pressed, use a mixed multiplier
        if (x != 0 && z != 0) {
            return (horizontalDashMultiplier + verticalDashMultiplier) / 2.3f;
        }

        // Checks how much the dash direction lines up with camera forward
        float dotVertical = Vector3.Dot(lockedDashDirection, camForward);

        // Checks how much the dash direction lines up with camera right
        float dotHorizontal = Vector3.Dot(lockedDashDirection, camRight);

        // If more vertical, use vertical multiplier
        if (Mathf.Abs(dotVertical) > Mathf.Abs(dotHorizontal)) {
            return verticalDashMultiplier + 0.2f;
        }

        // If more horizontal, use horizontal multiplier
        if (Mathf.Abs(dotHorizontal) > Mathf.Abs(dotVertical)) {
            return horizontalDashMultiplier + 0.1f;
        }

        // Backup mixed multiplier
        return (horizontalDashMultiplier + verticalDashMultiplier) / 2.3f;
    }

    private bool TryDashMove(Vector3 moveAmount) {
        // Gets how far the player is trying to move
        float distance = moveAmount.magnitude;

        // If movement is basically zero, do nothing
        if (distance <= 0.0001f) {
            return false;
        }

        // Gets the direction of the movement
        Vector3 direction = moveAmount.normalized;

        // SweepTest checks if the Rigidbody would hit something before moving
        if (rb.SweepTest(direction, out RaycastHit hit, distance + wallSkin, QueryTriggerInteraction.Ignore)) {
            // Calculates a safe distance so the player does not go inside the wall
            float safeDistance = Mathf.Max(hit.distance - wallSkin, 0f);

            // Moves player only to the safe point
            rb.MovePosition(rb.position + direction * safeDistance);

            // Return true because a wall was hit
            return true;
        }

        // If no wall was hit, move normally
        rb.MovePosition(rb.position + moveAmount);

        // Return false because no wall was hit
        return false;
    }

    public void SetDashCooldown(float newCooldown) {
        // Sets the dash cooldown
        dashCooldown = newCooldown;
    }

    public float GetDashCooldown() {
        // Returns the current dash cooldown
        return dashCooldown;
    }
}
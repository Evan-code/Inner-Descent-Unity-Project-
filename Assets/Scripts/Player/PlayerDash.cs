using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    private PlayerMovement moveScript;
    private Transform cam;
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

    private bool canDash = true;

    void Start()
    {
        moveScript = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();

        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        if (rb == null || cam == null)
            yield break;

        canDash = false;

        if (moveScript != null)
        {
            moveScript.IsDashing = true;
        }

        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }

        if (dashVFX != null)
        {
            dashVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            dashVFX.Play();
        }

        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = camRight * x + camForward * z;

        if (moveDirection.sqrMagnitude < 0.0001f)
        {
            moveDirection = transform.forward;
        }
        else
        {
            moveDirection.Normalize();
        }

        Vector3 lockedDashDirection = moveDirection.normalized;
        transform.forward = lockedDashDirection;

        float speedMultiplier = 1f;

        if (x != 0 && z == 0)
        {
            speedMultiplier = horizontalDashMultiplier;
        }
        else if (x == 0 && z != 0)
        {
            speedMultiplier = verticalDashMultiplier;
        }
        else if (x != 0 && z != 0)
        {
            speedMultiplier = (horizontalDashMultiplier + verticalDashMultiplier) / 2.3f;
        }

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

        float timer = 0f;

        while (timer < dashDuration)
        {
            timer += Time.deltaTime;

            Vector3 moveAmount = lockedDashDirection * dashSpeed * speedMultiplier * Time.deltaTime;

            bool hitWall = TryDashMove(moveAmount);

            transform.forward = lockedDashDirection;

            if (hitWall)
                break;

            yield return null;
        }

        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);

        if (moveScript != null)
        {
            moveScript.IsDashing = false;
        }

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    bool TryDashMove(Vector3 moveAmount)
    {
        float distance = moveAmount.magnitude;

        if (distance <= 0.0001f)
            return false;

        Vector3 direction = moveAmount.normalized;

        if (rb.SweepTest(direction, out RaycastHit hit, distance + wallSkin, QueryTriggerInteraction.Ignore))
        {
            float safeDistance = Mathf.Max(hit.distance - wallSkin, 0f);
            rb.MovePosition(rb.position + direction * safeDistance);
            return true;
        }

        rb.MovePosition(rb.position + moveAmount);
        return false;
    }

    public void SetDashCooldown(float newCooldown)
    {
        dashCooldown = newCooldown;
    }

    public float GetDashCooldown()
    {
        return dashCooldown;
    }
}
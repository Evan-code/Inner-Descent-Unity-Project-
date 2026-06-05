using System.Collections;
using UnityEngine;

// This script handles death after Health reaches 0
// It plays a death animation, turns off other scripts, waits, and destroys the object
public class DieOnZero : MonoBehaviour {
    // This stores the Health script on this object
    private Health health;

    [Header("Animation")]
    public Animator animator;

    [Header("Death Settings")]
    [SerializeField] private float destroyDelay = 1.5f;

    // This prevents the death routine from starting more than once
    private bool isDying = false;

    void Start() {
        // Gets the Health script on this object
        health = GetComponent<Health>();

        // If Health exists, listen for when it dies
        if (health != null) {
            health.OnDied += Die;
        }
    }

    void OnDestroy() {
        // Stop listening when this object is destroyed
        if (health != null) {
            health.OnDied -= Die;
        }
    }

    private void Die() {
        // If already dying, do not start again
        if (isDying) {
            return;
        }

        // Mark that this object is dying
        isDying = true;

        // Start the death routine
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine() {
        // If this is the player, stop screen shake so the death screen is clean
        if (CompareTag("Player") && ScreenShake.Instance != null) {
            ScreenShake.Instance.StopShakeForever();
        }

        // Play the death animation if Animator exists
        if (animator != null) {
            animator.SetTrigger("Die");
        }

        // Get every MonoBehaviour script on this object
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();

        // Loop through each script
        foreach (MonoBehaviour script in scripts) {
            // Do not turn off this script because it is running the death routine
            if (script != this) {
                // Disable the script so enemies stop moving or attacking after death
                script.enabled = false;
            }
        }

        // Wait so the death animation can play
        yield return new WaitForSeconds(destroyDelay);

        // Destroy the object after the delay
        Destroy(gameObject);
    }
}
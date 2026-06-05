using System.Collections;
using UnityEngine;

// This script shakes the camera
// PlayerReceiveDamage calls this when the player gets hit
public class ScreenShake : MonoBehaviour {
    // Static Instance lets other scripts easily call ScreenShake.Instance
    public static ScreenShake Instance;

    // Stores the current shake coroutine
    private Coroutine shakeCoroutine;

    // This can permanently stop shaking after death
    private bool shakeDisabled = false;

    void Awake() {
        // Save this script as the current screen shake instance
        Instance = this;
    }

    public void Shake(float duration, float strength) {
        // If shaking is disabled, do nothing
        if (shakeDisabled) {
            return;
        }

        // If a shake is already happening, stop it
        if (shakeCoroutine != null) {
            StopCoroutine(shakeCoroutine);
        }

        // Start a new shake
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    public void StopShakeForever() {
        // Prevents any future screen shake
        shakeDisabled = true;

        // If a shake is currently running, stop it
        if (shakeCoroutine != null) {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
    }

    private IEnumerator ShakeRoutine(float duration, float strength) {
        // Timer for how long the shake has been going
        float elapsed = 0f;

        // Keep shaking while there is time left
        while (elapsed < duration) {
            // Stop immediately if shaking gets disabled
            if (shakeDisabled) {
                yield break;
            }

            // Pick a random offset for the camera
            Vector3 offset = new Vector3(
                Random.Range(-strength, strength),
                Random.Range(-strength, strength),
                0f
            );

            // Move the camera by the offset
            transform.position += offset;

            // Wait one frame
            yield return null;

            // Move the camera back so it does not drift away
            transform.position -= offset;

            // Add time to the shake timer
            elapsed += Time.deltaTime;
        }

        // Clear the coroutine reference when done
        shakeCoroutine = null;
    }
}
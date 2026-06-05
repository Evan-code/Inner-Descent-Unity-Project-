using System.Collections;
using UnityEngine;

// This script handles the player getting hit
// It takes damage, flashes the player, gives invincibility, and shakes the screen
public class PlayerReceiveDamage : MonoBehaviour {
    // Stores the Health script
    private Health health;

    // Stores all renderers so the player can flash
    private Renderer[] renderers;

    [Header("Invincibility")]
    [SerializeField] private float invulnerabilityDuration = 1f;

    [Header("Flashing")]
    [SerializeField] private float initialFlashDuration = 0.08f;
    [SerializeField] private float slowFlashInterval = 0.25f;
    [SerializeField] private Color flashColor = new Color(0.65f, 0.65f, 0.65f, 1f);

    [Header("Screen Shake")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.25f;

    // True when the player has normal hit invincibility
    private bool isInvulnerable = false;

    // True only while dashing
    private bool dashInvincible = false;

    // Saves the original material colors
    private Color[] originalColors;

    void Awake() {
        // Gets the Health script on the player
        health = GetComponent<Health>();

        // Gets all renderers on the player and child objects
        renderers = GetComponentsInChildren<Renderer>();

        // Makes an array with one color slot for each renderer
        originalColors = new Color[renderers.Length];

        // Loops through all renderers
        for (int i = 0; i < renderers.Length; i++) {
            // Gets this renderer's material
            Material mat = renderers[i].material;

            // If using URP/Lit shader, color is usually _BaseColor
            if (mat.HasProperty("_BaseColor")) {
                originalColors[i] = mat.GetColor("_BaseColor");
            }
            // If using older shader, color is usually _Color
            else if (mat.HasProperty("_Color")) {
                originalColors[i] = mat.GetColor("_Color");
            }
            // Backup color if the material does not have either property
            else {
                originalColors[i] = Color.white;
            }
        }
    }

    public void Hit(int damage) {
        // If dashing, ignore the hit
        if (dashInvincible) {
            return;
        }

        // If already invulnerable or missing Health, ignore the hit
        if (isInvulnerable || health == null) {
            return;
        }

        // Damage the player
        health.TakeDamage(damage);

        // Only do shake and invincibility if the player survived
        if (health.currentHP > 0) {
            // Shake the screen if ScreenShake exists
            if (ScreenShake.Instance != null) {
                ScreenShake.Instance.Shake(shakeDuration, shakeStrength);
            }

            // Start the invincibility flashing
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    public void SetDashInvincible(bool value) {
        // PlayerDash uses this to turn dash invincibility on or off
        dashInvincible = value;
    }

    private IEnumerator InvulnerabilityCoroutine() {
        // Player is now invulnerable
        isInvulnerable = true;

        // Timer tracks how long invincibility has lasted
        float timer = 0f;

        // Flash once quickly right after getting hit
        SetPlayerColor(flashColor);

        // Wait for the first quick flash
        yield return new WaitForSeconds(initialFlashDuration);

        // Restore normal color
        RestoreOriginalColors();

        // Add the first flash time to timer
        timer += initialFlashDuration;

        // Keep blinking while invincibility is still active
        while (timer < invulnerabilityDuration) {
            // Set player to flash color
            SetPlayerColor(flashColor);

            // Wait while flashed
            yield return new WaitForSeconds(slowFlashInterval);

            // Restore normal color
            RestoreOriginalColors();

            // Wait while normal color
            yield return new WaitForSeconds(slowFlashInterval);

            // Add both wait times to timer
            timer += slowFlashInterval * 2f;
        }

        // Make sure color is normal at the end
        RestoreOriginalColors();

        // Player can now be hit again
        isInvulnerable = false;
    }

    private void SetPlayerColor(Color color) {
        // Loops through all renderers
        foreach (Renderer renderer in renderers) {
            // If renderer is missing, skip it
            if (renderer == null) {
                continue;
            }

            // Gets the material on this renderer
            Material mat = renderer.material;

            // Changes color for URP/Lit shader
            if (mat.HasProperty("_BaseColor")) {
                mat.SetColor("_BaseColor", color);
            }
            // Changes color for older shaders
            else if (mat.HasProperty("_Color")) {
                mat.SetColor("_Color", color);
            }
        }
    }

    private void RestoreOriginalColors() {
        // Loops through all renderers using an index
        for (int i = 0; i < renderers.Length; i++) {
            // If renderer is missing, skip it
            if (renderers[i] == null) {
                continue;
            }

            // Gets the material on this renderer
            Material mat = renderers[i].material;

            // Restores URP/Lit color
            if (mat.HasProperty("_BaseColor")) {
                mat.SetColor("_BaseColor", originalColors[i]);
            }
            // Restores older shader color
            else if (mat.HasProperty("_Color")) {
                mat.SetColor("_Color", originalColors[i]);
            }
        }
    }
}
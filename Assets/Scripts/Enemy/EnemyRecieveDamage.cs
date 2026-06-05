using System.Collections;
using UnityEngine;

// This script handles enemies getting hit
// It damages their Health, flashes them, and spawns floating damage numbers
public class EnemyReceiveDamage : MonoBehaviour {
    // This stores the enemy Health script
    private Health health;

    // This stores every Renderer on the enemy
    private Renderer[] renderers;

    [Header("Hit Flash")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = new Color32(236, 146, 146, 255);

    [Header("Damage Number")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 1.8f, 0f);
    [SerializeField] private float randomSpawnRadius = 0.5f;

    // This stores the enemy's original colors
    private Color[] originalColors;

    // This stores the flash coroutine so it can restart if hit again
    private Coroutine flashRoutine;

    void Awake() {
        // Gets the Health script on the enemy
        health = GetComponent<Health>();

        // Warn if Health is missing
        if (health == null) {
            Debug.LogError("EnemyReceiveDamage could not find a Health component on " + gameObject.name);
        }

        // Gets every Renderer on this enemy and its children
        renderers = GetComponentsInChildren<Renderer>();

        // Warn if there are no renderers to flash
        if (renderers.Length == 0) {
            Debug.LogWarning("EnemyReceiveDamage found no Renderers on " + gameObject.name);
        }

        // Make the original color array the same size as the renderers array
        originalColors = new Color[renderers.Length];

        // Loop through each renderer
        for (int i = 0; i < renderers.Length; i++) {
            // Gets the material from this renderer
            Material mat = renderers[i].material;

            // If material uses _BaseColor, save that color
            if (mat.HasProperty("_BaseColor")) {
                originalColors[i] = mat.GetColor("_BaseColor");
            }
            // If material uses _Color, save that color
            else if (mat.HasProperty("_Color")) {
                originalColors[i] = mat.GetColor("_Color");
            }
            // Backup color if neither property exists
            else {
                originalColors[i] = Color.white;
            }
        }
    }

    public void Hit(int damage) {
        // If Health exists, damage the enemy
        if (health != null) {
            health.TakeDamage(damage);
        }

        // Spawn a floating damage number
        SpawnDamageNumber(damage);

        // If already flashing, stop the old flash
        if (flashRoutine != null) {
            StopCoroutine(flashRoutine);
        }

        // Start a new flash
        flashRoutine = StartCoroutine(FlashCoroutine());
    }

    private void SpawnDamageNumber(int damage) {
        // If there is no prefab, do nothing
        if (damageNumberPrefab == null) {
            return;
        }

        // Creates a small random offset so numbers do not stack exactly
        Vector3 randomOffset = new Vector3(
            Random.Range(-randomSpawnRadius, randomSpawnRadius),
            Random.Range(-randomSpawnRadius, randomSpawnRadius),
            Random.Range(-randomSpawnRadius, randomSpawnRadius)
        );

        // Calculates where the number should appear
        Vector3 spawnPosition = transform.position + damageNumberOffset + randomOffset;

        // Spawns the damage number prefab
        GameObject numberObject = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);

        // Gets the DamageNumber script on the spawned object
        DamageNumber damageNumber = numberObject.GetComponent<DamageNumber>();

        // If the script exists, set the number text
        if (damageNumber != null) {
            damageNumber.SetDamage(damage);
        }
    }

    private IEnumerator FlashCoroutine() {
        // Loop through every renderer
        for (int i = 0; i < renderers.Length; i++) {
            // Gets the material on this renderer
            Material mat = renderers[i].material;

            // Set flash color for URP/Lit materials
            if (mat.HasProperty("_BaseColor")) {
                mat.SetColor("_BaseColor", flashColor);
            }
            // Set flash color for older materials
            else if (mat.HasProperty("_Color")) {
                mat.SetColor("_Color", flashColor);
            }
        }

        // Wait for the flash duration
        yield return new WaitForSeconds(flashDuration);

        // Loop through every renderer again
        for (int i = 0; i < renderers.Length; i++) {
            // Gets the material on this renderer
            Material mat = renderers[i].material;

            // Restore the original _BaseColor
            if (mat.HasProperty("_BaseColor")) {
                mat.SetColor("_BaseColor", originalColors[i]);
            }
            // Restore the original _Color
            else if (mat.HasProperty("_Color")) {
                mat.SetColor("_Color", originalColors[i]);
            }
        }

        // Clear the flash routine since it is done
        flashRoutine = null;
    }
}
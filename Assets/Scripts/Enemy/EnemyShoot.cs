using UnityEngine;

// This script lets a ranged enemy shoot bullets
// RangedEnemyAI calls Shoot when it is time to attack
public class EnemyShoot : MonoBehaviour {
    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Optional Shoot Point")]
    [SerializeField] private Transform shootPoint;

    [Header("Projectile Settings")]
    [SerializeField] private float spawnForwardOffset = 0.3f;

    // This stores the player's transform so the enemy can aim
    private Transform player;

    void Start() {
        // Looks for the player using the Player tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        // If player is found, save their transform
        if (playerObj != null) {
            player = playerObj.transform;
        } else {
            // Warn if no player was found
            Debug.LogError("EnemyShoot: No object with tag 'Player' found!");
        }
    }

    public void Shoot() {
        // If player is missing, do not shoot
        if (player == null) {
            Debug.LogWarning("EnemyShoot: Player is missing.");
            return;
        }

        // If projectile prefab is missing, do not shoot
        if (projectilePrefab == null) {
            Debug.LogWarning("EnemyShoot: projectilePrefab is not assigned.");
            return;
        }

        // This stores where the bullet should spawn from
        Vector3 origin;

        // If a shoot point is assigned, use that position
        if (shootPoint != null) {
            origin = shootPoint.position;
        } else {
            // If no shoot point exists, shoot from slightly above the enemy
            origin = transform.position + Vector3.up * 1.2f;
        }

        // Aim toward the player's body instead of their feet
        Vector3 direction = (player.position + Vector3.up * 1f - origin).normalized;

        // Spawn the bullet slightly forward so it does not appear inside the enemy
        Vector3 spawnPosition = origin + direction * spawnForwardOffset;

        // Create the bullet in the scene
        GameObject projectileObject = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        // Get the Bullet script from the spawned projectile
        Bullet bullet = projectileObject.GetComponent<Bullet>();

        // If the Bullet script exists, give it a direction
        if (bullet != null) {
            bullet.SetDirection(direction);
        } else {
            // Warn if the projectile does not have the Bullet script
            Debug.LogWarning("EnemyShoot: Spawned projectile is missing the Bullet script.");
        }
    }
}
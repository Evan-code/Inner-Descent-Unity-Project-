using UnityEngine;

// This script makes an enemy shoot toward the player automatically.
// The bullet spawns slightly in front of the enemy using an offset.
public class EnemyShoot : MonoBehaviour
{
    [Header("References")]
    private Transform player; // Found automatically using tag

    [SerializeField] private GameObject projectilePrefab;

    [Header("Shooting Settings")]
    [SerializeField] private float shootCooldown = 2f;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Spawn Offset")]
    [SerializeField] private float spawnOffset = 1f; // How far in front of enemy the bullet spawns

    private float nextShootTime = 0f;

    void Start()
    {
        // Find player using tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("EnemyShoot: No object with tag 'Player' found!");
        }
    }

    void Update()
    {
        if (player == null)
            return;

        if (Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootCooldown;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("EnemyShoot: projectilePrefab is not assigned.");
            return;
        }

        // Direction from enemy to player
        Vector3 direction = (player.position - transform.position).normalized;

        // Offset spawn position forward so bullet doesn't spawn inside enemy
        Vector3 spawnPosition = transform.position + direction * spawnOffset;

        // Spawn projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Rotate projectile to face direction
        projectile.transform.forward = direction;

        // Move projectile using Rigidbody
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }
        else
        {
            Debug.LogWarning("EnemyShoot: Projectile has no Rigidbody.");
        }
    }
}
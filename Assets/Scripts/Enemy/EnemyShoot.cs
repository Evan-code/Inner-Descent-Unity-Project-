using UnityEngine;

// This script creates and launches projectiles toward the player.
// The projectile spawns from a shoot point so it does not appear at the enemy's feet.
public class EnemyShoot : MonoBehaviour
{
    [Header("References")]
    private Transform player;

    [SerializeField] private GameObject projectilePrefab;

    [Header("Optional Shoot Point")]
    [SerializeField] private Transform shootPoint;

    [Header("Projectile Settings")]
    [SerializeField] private float spawnForwardOffset = 0.3f;

    void Start()
    {
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

    public void Shoot()
    {
        if (player == null)
        {
            Debug.LogWarning("EnemyShoot: Player is missing.");
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("EnemyShoot: projectilePrefab is not assigned.");
            return;
        }

        // Use shoot point if assigned. Otherwise fall back to enemy position slightly above center.
        Vector3 origin;

        if (shootPoint != null)
        {
            origin = shootPoint.position;
        }
        else
        {
            origin = transform.position + Vector3.up * 1.2f;
        }

        Vector3 direction = (player.position + Vector3.up * 1f - origin).normalized;
        Vector3 spawnPosition = origin + direction * spawnForwardOffset;

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        Bullet bullet = projectileObject.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.SetDirection(direction);
        }
        else
        {
            Debug.LogWarning("EnemyShoot: Spawned projectile is missing the Bullet script.");
        }
    }
}
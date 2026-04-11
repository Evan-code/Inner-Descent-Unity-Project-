using UnityEngine;

// This script handles only projectile firing.
// EnemyAI should decide WHEN to call Shoot().
public class EnemyShoot : MonoBehaviour
{
    [Header("References")]
    private Transform player;

    [SerializeField] private GameObject projectilePrefab;

    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Spawn Offset")]
    [SerializeField] private float spawnOffset = 1f;

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

        Vector3 direction = (player.position - transform.position).normalized;

        Vector3 spawnPosition = transform.position + direction * spawnOffset;

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));

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
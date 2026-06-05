using System.Collections.Generic;
using UnityEngine;

// This script handles spawning enemies and a boss in a room
// The room only counts as cleared when all normal enemies AND the boss are defeated
public class RoomManager : MonoBehaviour {
    [Header("Enemy Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Enemy Spawn Settings")]
    public int enemiesToSpawn = 3;

    [Header("Boss Spawn Point")]
    public Transform bossSpawnPoint;

    [Header("Boss Prefabs")]
    public GameObject[] bossPrefabs;

    [Header("Boss Settings")]
    public bool spawnBoss = true;

    // This list stores all normal enemies that are still alive
    private List<GameObject> aliveEnemies = new List<GameObject>();

    // This stores the boss that spawned in the room
    private GameObject aliveBoss;

    // This prevents the room clear event from happening more than once
    private bool roomCleared = false;

    // This makes sure enemies actually spawned before the room can clear
    private bool hasSpawnedEnemies = false;

    // This makes sure the boss actually spawned before the room can clear
    private bool hasSpawnedBoss = false;

    // Other scripts can listen to this when the room is cleared
    public System.Action OnRoomCleared;

    void Start() {
        // Spawn normal enemies when the room starts
        SpawnEnemies();

        // Spawn the boss when the room starts
        SpawnBoss();
    }

    void Update() {
        // Remove normal enemies from the list if they were destroyed
        aliveEnemies.RemoveAll(enemy => enemy == null);

        // Checks if all normal enemies are gone
        bool enemiesDefeated = hasSpawnedEnemies && aliveEnemies.Count == 0;

        // Checks if the boss is gone
        bool bossDefeated = hasSpawnedBoss && aliveBoss == null;

        // If boss spawning is turned off, then boss is automatically counted as defeated
        if (!spawnBoss) {
            bossDefeated = true;
        }

        // Only clear the room when enemies and boss are both defeated
        if (!roomCleared && enemiesDefeated && bossDefeated) {
            // Mark the room as cleared so this only happens once
            roomCleared = true;

            // Print a message so we know the room cleared
            Debug.Log("Room Cleared! Enemies and boss defeated.");

            // Tell other scripts the room was cleared
            OnRoomCleared?.Invoke();
        }
    }

    public void SpawnEnemies() {
        // Reset room clear status
        roomCleared = false;

        // Reset enemy spawned status
        hasSpawnedEnemies = false;

        // Clear the old enemy list
        aliveEnemies.Clear();

        // Make sure enemy prefabs are assigned
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) {
            Debug.LogWarning("RoomManager: No enemy prefabs assigned!");
            return;
        }

        // Make sure spawn points are assigned
        if (spawnPoints == null || spawnPoints.Length == 0) {
            Debug.LogWarning("RoomManager: No enemy spawn points assigned!");
            return;
        }

        // Make sure the number of enemies is above 0
        if (enemiesToSpawn <= 0) {
            Debug.LogWarning("RoomManager: enemiesToSpawn is 0!");
            return;
        }

        // Spawn the amount of enemies set in the Inspector
        for (int i = 0; i < enemiesToSpawn; i++) {
            // Pick a random spawn point
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Pick a random enemy prefab
            GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // Spawn the enemy at the chosen spawn point
            GameObject enemy = Instantiate(randomEnemyPrefab, spawn.position, spawn.rotation);

            // Add this enemy to the alive enemies list
            aliveEnemies.Add(enemy);
        }

        // If at least one enemy spawned, mark enemies as spawned
        if (aliveEnemies.Count > 0) {
            hasSpawnedEnemies = true;
        }
    }

    public void SpawnBoss() {
        // Clear old boss reference before spawning a new one
        aliveBoss = null;

        // Reset boss spawned status
        hasSpawnedBoss = false;

        // If boss spawning is turned off, do nothing
        if (!spawnBoss) {
            return;
        }

        // Make sure boss prefabs are assigned
        if (bossPrefabs == null || bossPrefabs.Length == 0) {
            Debug.LogWarning("RoomManager: No boss prefabs assigned!");
            return;
        }

        // Make sure a boss spawn point is assigned
        if (bossSpawnPoint == null) {
            Debug.LogWarning("RoomManager: No boss spawn point assigned!");
            return;
        }

        // Pick a random boss prefab from the boss prefab list
        GameObject randomBossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Length)];

        // Spawn the boss at the boss spawn point
        aliveBoss = Instantiate(randomBossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);

        // Mark that the boss spawned
        hasSpawnedBoss = true;
    }
}
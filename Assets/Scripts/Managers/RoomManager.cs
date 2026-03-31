using System.Collections.Generic;
using UnityEngine;

// This script handles spawning enemies and detecting when the room is cleared.
// It now supports MULTIPLE enemy types.
public class RoomManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Enemy Prefabs (Multiple Types)")]
    public GameObject[] enemyPrefabs; // <-- MULTIPLE enemies now

    [Header("Spawn Settings")]
    public int enemiesToSpawn = 3;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool roomCleared = false;
    private bool hasSpawnedEnemies = false;

    public System.Action OnRoomCleared;

    void Start()
    {
        SpawnEnemies();
    }

    void Update()
    {
        // Remove destroyed enemies
        aliveEnemies.RemoveAll(enemy => enemy == null);

        // Only trigger clear AFTER enemies were spawned
        if (hasSpawnedEnemies && !roomCleared && aliveEnemies.Count == 0)
        {
            roomCleared = true;
            Debug.Log("Room Cleared!");
            OnRoomCleared?.Invoke();
        }
    }

    public void SpawnEnemies()
    {
        // Reset state
        roomCleared = false;
        hasSpawnedEnemies = false;
        aliveEnemies.Clear();

        // Safety checks
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("RoomManager: No enemy prefabs assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("RoomManager: No spawn points assigned!");
            return;
        }

        if (enemiesToSpawn <= 0)
        {
            Debug.LogWarning("RoomManager: enemiesToSpawn is 0!");
            return;
        }

        // Spawn enemies
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Pick a random enemy type
            GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            GameObject enemy = Instantiate(randomEnemyPrefab, spawn.position, spawn.rotation);
            aliveEnemies.Add(enemy);
        }

        if (aliveEnemies.Count > 0)
        {
            hasSpawnedEnemies = true;
        }
    }
}
using UnityEngine;

public class RandomRoomSpawner : MonoBehaviour
{
    [Header("Room Prefabs")]
    public GameObject[] roomPrefabs;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    private GameObject currentRoom;

    void Start()
    {
        SpawnRandomRoom();
    }

    public void SpawnRandomRoom()
    {
        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            Debug.LogWarning("No room prefabs assigned!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("No spawn point assigned!");
            return;
        }

        if (currentRoom != null)
        {
            Destroy(currentRoom);
        }

        int randomIndex = Random.Range(0, roomPrefabs.Length);
        currentRoom = Instantiate(roomPrefabs[randomIndex], spawnPoint.position, spawnPoint.rotation);
    }
}
using UnityEngine;

public class PlayerEnemyCollisionIgnorer : MonoBehaviour
{
    [Header("Layers")]
    public string playerLayerName = "Player";
    public string enemyLayerName = "Enemy";

    void Awake()
    {
        int playerLayer = LayerMask.NameToLayer(playerLayerName);
        int enemyLayer = LayerMask.NameToLayer(enemyLayerName);

        if (playerLayer == -1)
        {
            Debug.LogError("Player layer does not exist. Create a layer called Player.");
            return;
        }

        if (enemyLayer == -1)
        {
            Debug.LogError("Enemy layer does not exist. Create a layer called Enemy.");
            return;
        }

        // Makes Player and Enemy pass through each other physically.
        // This does NOT stop OverlapSphere attacks from detecting enemies.
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
    }
}

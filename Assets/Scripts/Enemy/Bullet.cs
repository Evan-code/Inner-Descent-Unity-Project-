using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter(Collider other)
    {
        PlayerReceiveDamage playerDamage = other.GetComponent<PlayerReceiveDamage>();

        if (playerDamage != null)
        {
            playerDamage.Hit(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}

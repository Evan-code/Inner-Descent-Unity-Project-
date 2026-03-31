using System;
using UnityEngine;

// Handles health, damage, and death
public class Health : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 10;
    public int currentHP;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = true;

    public event Action OnDied;

    private bool isDead = false;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHP -= amount;

        if (currentHP <= 0)
        {
            currentHP = 0;
            isDead = true;

            OnDied?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }
}
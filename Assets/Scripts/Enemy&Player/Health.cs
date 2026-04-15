using System;
using UnityEngine;

// This script stores health for any object that can take damage.
// It supports:
// - taking damage
// - healing
// - death
// - notifying UI when health changes
public class Health : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 10;
    public int currentHP;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = true;

    // Called when this object dies
    public event Action OnDied;

    // Called whenever health changes
    public event Action<int, int> OnHealthChanged;

    private bool isDead = false;

    void Awake()
    {
        currentHP = maxHP;

        // Tell any listeners what our starting health is
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHP -= amount;

        if (currentHP < 0)
            currentHP = 0;

        // Notify UI and health bars
        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
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

        currentHP += amount;

        if (currentHP > maxHP)
            currentHP = maxHP;

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    // Lets other scripts force a UI refresh if needed
    public void RefreshHealthUI()
    {
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
}
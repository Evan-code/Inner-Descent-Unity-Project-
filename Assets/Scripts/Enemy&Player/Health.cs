using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 10;
    public int currentHP;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = true;

    public event Action OnDied;
    public event Action<int, int> OnHealthChanged;

    private bool isDead = false;

    void Awake()
    {
        currentHP = maxHP;
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void SetHealth(int newCurrentHP, int newMaxHP)
    {
        maxHP = newMaxHP;
        currentHP = Mathf.Clamp(newCurrentHP, 1, maxHP);
        isDead = false;

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;

        if (currentHP < 0)
            currentHP = 0;

        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            isDead = true;
            OnDied?.Invoke();

            if (destroyOnDeath)
                Destroy(gameObject);
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHP += amount;

        if (currentHP > maxHP)
            currentHP = maxHP;

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void RefreshHealthUI()
    {
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
}
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
    }

    public void SetHealth(int newCurrentHP, int newMaxHP)
    {
        maxHP = newMaxHP;

        currentHP = Mathf.Clamp(newCurrentHP, 1, maxHP);

        isDead = false;

        PlayerRunData.SaveHealth(currentHP);

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        OnHealthChanged?.Invoke(currentHP, maxHP);

        // Save health ONLY if still alive
        if (currentHP > 0)
        {
            PlayerRunData.SaveHealth(currentHP);
        }

        // 🔥 DEATH TRIGGER
        if (currentHP <= 0 && !isDead)
        {
            isDead = true;

            OnDied?.Invoke(); // ← THIS triggers death screen

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        PlayerRunData.SaveHealth(currentHP);

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void RefreshHealthUI()
    {
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
}
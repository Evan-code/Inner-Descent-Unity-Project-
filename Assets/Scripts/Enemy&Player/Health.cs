using System;
using UnityEngine;

// This is the main health script for players and enemies
// It handles current health, max health, healing, damage, death, and UI updates
public class Health : MonoBehaviour {
    [Header("Health")]
    public int maxHP = 10;
    public int currentHP;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = true;

    // Other scripts can listen to this when the object dies
    public event Action OnDied;

    // Other scripts can listen to this when health changes
    // It sends currentHP and maxHP
    public event Action<int, int> OnHealthChanged;

    // This stops damage/death from happening more than once
    private bool isDead = false;

    void Awake() {
        // Start at full health
        currentHP = maxHP;

        // Tell UI or other listeners what the starting health is
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void SetHealth(int newCurrentHP, int newMaxHP) {
        // Set the max health first
        maxHP = newMaxHP;

        // Makes sure maxHP is at least 1 so health math does not break
        if (maxHP < 1) {
            maxHP = 1;
        }

        // Clamp current health so it stays between 1 and maxHP
        currentHP = Mathf.Clamp(newCurrentHP, 1, maxHP);

        // Since health was set above 0, this object is not dead
        isDead = false;

        // Tell UI that health changed
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int amount) {
        // If already dead, ignore damage
        if (isDead) {
            return;
        }

        // If damage amount is 0 or less, ignore it
        if (amount <= 0) {
            return;
        }

        // Subtract damage from current health
        currentHP -= amount;

        // Make sure health does not go below 0
        if (currentHP < 0) {
            currentHP = 0;
        }

        // Tell UI that health changed
        OnHealthChanged?.Invoke(currentHP, maxHP);

        // If health is 0 or less, die
        if (currentHP <= 0) {
            // Mark as dead so death only happens once
            isDead = true;

            // Tell other scripts this object died
            OnDied?.Invoke();

            // If destroyOnDeath is true and there is no DieOnZero script, destroy right away
            // If DieOnZero exists, that script handles the death animation and destroy delay
            if (destroyOnDeath && GetComponent<DieOnZero>() == null) {
                Destroy(gameObject);
            }
        }
    }

    public void Heal(int amount) {
        // If dead, do not heal
        if (isDead) {
            return;
        }

        // If healing amount is 0 or less, do nothing
        if (amount <= 0) {
            return;
        }

        // Add healing to current health
        currentHP += amount;

        // Make sure health does not go above maxHP
        if (currentHP > maxHP) {
            currentHP = maxHP;
        }

        // Tell UI that health changed
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void RefreshHealthUI() {
        // Manually tells health bars to update using current values
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
}
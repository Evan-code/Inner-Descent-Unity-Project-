using UnityEngine;

// Stores player health and upgrades between scenes.
public static class PlayerRunData
{
    public static int bonusDamage = 0;

    public static float moveSpeedMultiplier = 1f;
    public static float dashCooldownMultiplier = 1f;
    public static float attackCooldownMultiplier = 1f;

    public static int savedHealth = 0;
    public static bool hasSavedHealth = false;

    public static void SaveHealth(int currentHealth)
    {
        savedHealth = currentHealth;
        hasSavedHealth = true;

        Debug.Log("Saved health: " + savedHealth);
    }

    public static void ResetRun()
    {
        bonusDamage = 0;

        moveSpeedMultiplier = 1f;
        dashCooldownMultiplier = 1f;
        attackCooldownMultiplier = 1f;

        savedHealth = 0;
        hasSavedHealth = false;

        Debug.Log("Run reset. Health and upgrades cleared.");
    }
}
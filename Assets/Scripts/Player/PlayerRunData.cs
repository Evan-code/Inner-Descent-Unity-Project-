using UnityEngine;

// This script stores player run data between scenes
// Since it is static, it does not need to be attached to a GameObject
public static class PlayerRunData {
    // Extra damage added from upgrades
    public static int bonusDamage = 0;

    // Multiplier for player movement speed upgrades
    public static float moveSpeedMultiplier = 1f;

    // Multiplier for dash cooldown upgrades
    public static float dashCooldownMultiplier = 1f;

    // Multiplier for attack cooldown upgrades
    public static float attackCooldownMultiplier = 1f;

    // Stores the player's health between scenes
    public static int savedHealth = 0;

    // Tells if saved health actually exists yet
    public static bool hasSavedHealth = false;

    public static void SaveHealth(int currentHealth) {
        // Store the current player health
        savedHealth = currentHealth;

        // Mark that health has been saved
        hasSavedHealth = true;

        // Debug message to check if health saving works
        Debug.Log("Saved health: " + savedHealth);
    }

    public static void ResetRun() {
        // Reset damage upgrade
        bonusDamage = 0;

        // Reset movement speed upgrade
        moveSpeedMultiplier = 1f;

        // Reset dash cooldown upgrade
        dashCooldownMultiplier = 1f;

        // Reset attack cooldown upgrade
        attackCooldownMultiplier = 1f;

        // Clear saved health
        savedHealth = 0;

        // Mark that there is no saved health anymore
        hasSavedHealth = false;

        // Debug message to show run data was reset
        Debug.Log("Run reset. Health and upgrades cleared.");
    }
}
using System.Collections;
using UnityEngine;

// This script applies saved upgrades and saved health to the player
// Put this on the player prefab or player object in every combat scene
public class PlayerUpgradeApplier : MonoBehaviour {
    private IEnumerator Start() {
        // Wait one frame so Health, UI, and other scripts can start first
        yield return new WaitForEndOfFrame();

        // Apply saved damage upgrade
        ApplyDamageUpgrade();

        // Apply saved movement speed upgrade
        ApplyMoveSpeedUpgrade();

        // Apply saved dash cooldown upgrade
        ApplyDashCooldownUpgrade();

        // Apply saved attack cooldown upgrade
        ApplyAttackCooldownUpgrade();

        // Load saved health after upgrades are applied
        LoadSavedHealth();
    }

    private void ApplyDamageUpgrade() {
        // Get the player combat script
        PlayerCombat combat = GetComponent<PlayerCombat>();

        // If there is no combat script, do nothing
        if (combat == null) {
            return;
        }

        // Add saved bonus damage to current damage
        combat.SetDamage(combat.GetDamage() + PlayerRunData.bonusDamage);
    }

    private void ApplyMoveSpeedUpgrade() {
        // Get the player movement script
        PlayerMovement movement = GetComponent<PlayerMovement>();

        // If there is no movement script, do nothing
        if (movement == null) {
            return;
        }

        // Multiply the current sideways speed by the saved upgrade multiplier
        float newMoveSpeed = movement.GetMoveSpeed() * PlayerRunData.moveSpeedMultiplier;

        // Multiply the current forward/back speed by the saved upgrade multiplier
        float newVerticalSpeed = movement.GetVerticalSpeed() * PlayerRunData.moveSpeedMultiplier;

        // Apply the new movement speeds
        movement.SetMovementSpeeds(newMoveSpeed, newVerticalSpeed);
    }

    private void ApplyDashCooldownUpgrade() {
        // Get the player dash script
        PlayerDash dash = GetComponent<PlayerDash>();

        // If there is no dash script, do nothing
        if (dash == null) {
            return;
        }

        // Multiply dash cooldown by saved multiplier
        // A lower multiplier makes the cooldown shorter
        dash.SetDashCooldown(dash.GetDashCooldown() * PlayerRunData.dashCooldownMultiplier);
    }

    private void ApplyAttackCooldownUpgrade() {
        // Get the player combat script
        PlayerCombat combat = GetComponent<PlayerCombat>();

        // If there is no combat script, do nothing
        if (combat == null) {
            return;
        }

        // Multiply attack cooldown by saved multiplier
        // A lower multiplier makes attacking faster
        combat.SetAttackCooldown(combat.GetAttackCooldown() * PlayerRunData.attackCooldownMultiplier);
    }

    private void LoadSavedHealth() {
        // Get the health script from the player
        Health health = GetComponent<Health>();

        // If there is no health script, do nothing
        if (health == null) {
            return;
        }

        // If saved health exists, load it
        if (PlayerRunData.hasSavedHealth) {
            // Use SetHealth instead of directly changing currentHP
            // This is important because SetHealth also updates the health UI
            health.SetHealth(PlayerRunData.savedHealth, health.maxHP);

            // Debug message to show loaded health
            Debug.Log("Loaded saved health: " + health.currentHP);
        } else {
            // If no saved health exists, just refresh the health UI
            health.RefreshHealthUI();
        }
    }
}
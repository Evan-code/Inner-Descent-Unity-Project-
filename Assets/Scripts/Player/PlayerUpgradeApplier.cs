using UnityEngine;

// This script reads the saved run upgrades
// and applies them to the player every time a combat scene loads.
public class PlayerUpgradeApplier : MonoBehaviour
{
    private Health health;
    private PlayerCombat combat;
    private PlayerMovement movement;
    private PlayerDash dash;

    // These store the player's original/base stats from the Inspector
    private int baseMaxHP;
    private int baseDamage;
    private float baseMoveSpeed;
    private float baseVerticalSpeed;
    private float baseDashCooldown;
    private float baseAttackCooldown;

    void Awake()
    {
        // Get references to player scripts
        health = GetComponent<Health>();
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();
        dash = GetComponent<PlayerDash>();

        // Save original values so upgrades stack from base values, not from already changed ones
        if (health != null)
            baseMaxHP = health.maxHP;
        else
            Debug.LogWarning("PlayerUpgradeApplier: Health component missing.");

        if (combat != null)
        {
            baseDamage = combat.GetDamage();
            baseAttackCooldown = combat.GetAttackCooldown();
        }
        else
        {
            Debug.LogWarning("PlayerUpgradeApplier: PlayerCombat component missing.");
        }

        if (movement != null)
        {
            baseMoveSpeed = movement.GetMoveSpeed();
            baseVerticalSpeed = movement.GetVerticalSpeed();
        }
        else
        {
            Debug.LogWarning("PlayerUpgradeApplier: PlayerMovement component missing.");
        }

        if (dash != null)
            baseDashCooldown = dash.GetDashCooldown();
        else
            Debug.LogWarning("PlayerUpgradeApplier: PlayerDash component missing.");
    }

    void Start()
    {
        ApplyUpgrades();
    }

    void ApplyUpgrades()
    {
        // Apply max health
        if (health != null)
        {
            health.maxHP = baseMaxHP + PlayerRunData.bonusMaxHP;
            health.currentHP = health.maxHP; // Heal to full when entering new combat scene
        }

        // Apply damage and attack cooldown
        if (combat != null)
        {
            combat.SetDamage(baseDamage + PlayerRunData.bonusDamage);
            combat.SetAttackCooldown(baseAttackCooldown * PlayerRunData.attackCooldownMultiplier);
        }

        // Apply movement speed
        if (movement != null)
        {
            float newMove = baseMoveSpeed * PlayerRunData.moveSpeedMultiplier;
            float newVertical = baseVerticalSpeed * PlayerRunData.moveSpeedMultiplier;
            movement.SetMovementSpeeds(newMove, newVertical);
        }

        // Apply dash cooldown
        if (dash != null)
        {
            dash.SetDashCooldown(baseDashCooldown * PlayerRunData.dashCooldownMultiplier);
        }

        Debug.Log(
            "Applied Upgrades To Player -> " +
            "maxHP: " + (health != null ? health.maxHP.ToString() : "missing") +
            ", damage: " + (combat != null ? combat.GetDamage().ToString() : "missing") +
            ", moveSpeed: " + (movement != null ? movement.GetMoveSpeed().ToString() : "missing") +
            ", verticalSpeed: " + (movement != null ? movement.GetVerticalSpeed().ToString() : "missing") +
            ", dashCooldown: " + (dash != null ? dash.GetDashCooldown().ToString() : "missing") +
            ", attackCooldown: " + (combat != null ? combat.GetAttackCooldown().ToString() : "missing")
        );
    }
}
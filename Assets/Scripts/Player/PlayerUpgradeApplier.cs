using UnityEngine;

public class PlayerUpgradeApplier : MonoBehaviour
{
    private Health health;
    private PlayerCombat combat;
    private PlayerMovement movement;
    private PlayerDash dash;

    private int baseMaxHP;
    private int baseDamage;
    private float baseMoveSpeed;
    private float baseVerticalSpeed;
    private float baseDashCooldown;
    private float baseAttackCooldown;

    void Awake()
    {
        health = GetComponent<Health>();
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();
        dash = GetComponent<PlayerDash>();

        if (health != null)
        {
            baseMaxHP = health.maxHP;
        }

        if (combat != null)
        {
            baseDamage = combat.GetDamage();
            baseAttackCooldown = combat.GetAttackCooldown();
        }

        if (movement != null)
        {
            baseMoveSpeed = movement.GetMoveSpeed();
            baseVerticalSpeed = movement.GetVerticalSpeed();
        }

        if (dash != null)
        {
            baseDashCooldown = dash.GetDashCooldown();
        }
    }

    void Start()
    {
        ApplyUpgrades();
    }

    void ApplyUpgrades()
    {
        if (health != null)
        {
            int newMaxHP = baseMaxHP + PlayerRunData.bonusMaxHP;
            int newCurrentHP;

            if (PlayerRunData.hasSavedHealth)
            {
                newCurrentHP = Mathf.Clamp(PlayerRunData.savedCurrentHP, 1, newMaxHP);
            }
            else
            {
                newCurrentHP = newMaxHP;
            }

            health.SetHealth(newCurrentHP, newMaxHP);

            PlayerRunData.SaveHealth(health.currentHP);
        }

        if (combat != null)
        {
            combat.SetDamage(baseDamage + PlayerRunData.bonusDamage);
            combat.SetAttackCooldown(baseAttackCooldown * PlayerRunData.attackCooldownMultiplier);
        }

        if (movement != null)
        {
            movement.SetMovementSpeeds(
                baseMoveSpeed * PlayerRunData.moveSpeedMultiplier,
                baseVerticalSpeed * PlayerRunData.moveSpeedMultiplier
            );
        }

        if (dash != null)
        {
            dash.SetDashCooldown(baseDashCooldown * PlayerRunData.dashCooldownMultiplier);
        }
    }
}
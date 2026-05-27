using System.Collections;
using System.Reflection;
using UnityEngine;

public class PlayerUpgradeApplier : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        ApplyDamageUpgrade();
        ApplyMoveSpeedUpgrade();
        ApplyDashCooldownUpgrade();
        ApplyAttackCooldownUpgrade();
        LoadSavedHealth();
    }

    void ApplyDamageUpgrade()
    {
        PlayerCombat combat = GetComponent<PlayerCombat>();

        if (combat == null)
            return;

        combat.SetDamage(combat.GetDamage() + PlayerRunData.bonusDamage);
    }

    void ApplyMoveSpeedUpgrade()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();

        if (movement == null)
            return;

        FieldInfo moveSpeedField = typeof(PlayerMovement).GetField(
            "moveSpeed",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        if (moveSpeedField == null)
        {
            Debug.LogWarning("Could not find moveSpeed variable in PlayerMovement.");
            return;
        }

        float currentMoveSpeed = (float)moveSpeedField.GetValue(movement);
        moveSpeedField.SetValue(movement, currentMoveSpeed * PlayerRunData.moveSpeedMultiplier);
    }

    void ApplyDashCooldownUpgrade()
    {
        PlayerDash dash = GetComponent<PlayerDash>();

        if (dash == null)
            return;

        dash.SetDashCooldown(dash.GetDashCooldown() * PlayerRunData.dashCooldownMultiplier);
    }

    void ApplyAttackCooldownUpgrade()
    {
        PlayerCombat combat = GetComponent<PlayerCombat>();

        if (combat == null)
            return;

        combat.SetAttackCooldown(combat.GetAttackCooldown() * PlayerRunData.attackCooldownMultiplier);
    }

    void LoadSavedHealth()
    {
        Health health = GetComponent<Health>();

        if (health == null)
            return;

        if (PlayerRunData.hasSavedHealth)
        {
            health.currentHP = PlayerRunData.savedHealth;

            if (health.currentHP > health.maxHP)
            {
                health.currentHP = health.maxHP;
            }

            if (health.currentHP < 1)
            {
                health.currentHP = 1;
            }

            Debug.Log("Loaded saved health: " + health.currentHP);
        }
    }
}
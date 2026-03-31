using UnityEngine;

// Stores upgrade values between scenes during one run.
public static class PlayerRunData
{
    public static int bonusMaxHP = 0;
    public static int bonusDamage = 0;

    public static float moveSpeedMultiplier = 1f;
    public static float dashCooldownMultiplier = 1f;
    public static float attackCooldownMultiplier = 1f;

    public static void ResetRun()
    {
        bonusMaxHP = 0;
        bonusDamage = 0;
        moveSpeedMultiplier = 1f;
        dashCooldownMultiplier = 1f;
        attackCooldownMultiplier = 1f;
    }
}
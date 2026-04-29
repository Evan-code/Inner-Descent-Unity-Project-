using UnityEngine;

public static class PlayerRunData
{
    public static int bonusMaxHP = 0;
    public static int bonusDamage = 0;

    public static float moveSpeedMultiplier = 1f;
    public static float dashCooldownMultiplier = 1f;
    public static float attackCooldownMultiplier = 1f;

    public static bool hasSavedHealth = false;
    public static int savedCurrentHP = 0;

    public static void SaveHealth(int currentHP)
    {
        if (currentHP > 0)
        {
            savedCurrentHP = currentHP;
            hasSavedHealth = true;
        }
    }

    public static void ResetRun()
    {
        bonusMaxHP = 0;
        bonusDamage = 0;

        moveSpeedMultiplier = 1f;
        dashCooldownMultiplier = 1f;
        attackCooldownMultiplier = 1f;

        hasSavedHealth = false;
        savedCurrentHP = 0;
    }
}
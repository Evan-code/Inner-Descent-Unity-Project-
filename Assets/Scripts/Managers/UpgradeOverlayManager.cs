using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// This script shows an upgrade menu over the current level.
// It darkens the background, pauses the game,
// and lets the player choose 1 of 3 upgrades.
public class UpgradeOverlayManager : MonoBehaviour
{
    [Header("Overlay UI")]
    [SerializeField] private GameObject upgradeOverlay;   // Parent object that holds dark background + panel
    [SerializeField] private Button[] upgradeButtons;     // 3 upgrade buttons
    [SerializeField] private TMP_Text[] buttonTexts;      // Text for the 3 buttons

    [Header("Scene Flow")]
    [SerializeField] private string nextSceneName = "Combat02";

    private bool overlayShowing = false;

    private class UpgradeOption
    {
        public string title;
        public UpgradeType type;
        public float amount;

        public UpgradeOption(string title, UpgradeType type, float amount)
        {
            this.title = title;
            this.type = type;
            this.amount = amount;
        }
    }

    private enum UpgradeType
    {
        MaxHealth,
        Damage,
        MoveSpeed,
        DashCooldown,
        AttackCooldown
    }

    private List<UpgradeOption> allUpgrades = new List<UpgradeOption>();

    void Start()
    {
        if (upgradeOverlay != null)
        {
            upgradeOverlay.SetActive(false);
        }
        else
        {
            Debug.LogWarning("UpgradeOverlayManager: upgradeOverlay is not assigned.");
        }

        CreateUpgradePool();
    }

    void CreateUpgradePool()
    {
        allUpgrades.Clear();

        allUpgrades.Add(new UpgradeOption("+2 Max Health", UpgradeType.MaxHealth, 2));
        allUpgrades.Add(new UpgradeOption("+1 Damage", UpgradeType.Damage, 1));
        allUpgrades.Add(new UpgradeOption("+15% Move Speed", UpgradeType.MoveSpeed, 1.15f));
        allUpgrades.Add(new UpgradeOption("10% Shorter Dash Cooldown", UpgradeType.DashCooldown, 0.90f));
        allUpgrades.Add(new UpgradeOption("10% Shorter Attack Cooldown", UpgradeType.AttackCooldown, 0.90f));
    }

    // Call this when the room is cleared
    public void ShowUpgradeOverlay()
    {
        if (overlayShowing)
            return;

        overlayShowing = true;

        if (upgradeOverlay != null)
        {
            upgradeOverlay.SetActive(true);
        }

        // Pause gameplay
        Time.timeScale = 0f;

        Debug.Log("Upgrade overlay shown.");

        ShowThreeRandomUpgrades();
    }

    void ShowThreeRandomUpgrades()
    {
        if (upgradeButtons == null || buttonTexts == null)
        {
            Debug.LogWarning("UpgradeOverlayManager: upgradeButtons or buttonTexts is null.");
            return;
        }

        List<UpgradeOption> available = new List<UpgradeOption>(allUpgrades);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (available.Count == 0)
                break;

            int randomIndex = Random.Range(0, available.Count);
            UpgradeOption chosenUpgrade = available[randomIndex];
            available.RemoveAt(randomIndex);

            if (i < buttonTexts.Length && buttonTexts[i] != null)
            {
                buttonTexts[i].text = chosenUpgrade.title;
            }

            if (upgradeButtons[i] != null)
            {
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => SelectUpgrade(chosenUpgrade));
            }
        }
    }

    void SelectUpgrade(UpgradeOption chosenUpgrade)
    {
        ApplyUpgrade(chosenUpgrade);

        Debug.Log("Player picked upgrade: " + chosenUpgrade.title);
        Debug.Log(
            "Run Data Now -> " +
            "bonusMaxHP: " + PlayerRunData.bonusMaxHP +
            ", bonusDamage: " + PlayerRunData.bonusDamage +
            ", moveSpeedMultiplier: " + PlayerRunData.moveSpeedMultiplier +
            ", dashCooldownMultiplier: " + PlayerRunData.dashCooldownMultiplier +
            ", attackCooldownMultiplier: " + PlayerRunData.attackCooldownMultiplier
        );

        // Unpause before changing scene
        Time.timeScale = 1f;

        SceneManager.LoadScene(nextSceneName);
    }

    void ApplyUpgrade(UpgradeOption upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.MaxHealth:
                PlayerRunData.bonusMaxHP += (int)upgrade.amount;
                break;

            case UpgradeType.Damage:
                PlayerRunData.bonusDamage += (int)upgrade.amount;
                break;

            case UpgradeType.MoveSpeed:
                PlayerRunData.moveSpeedMultiplier *= upgrade.amount;
                break;

            case UpgradeType.DashCooldown:
                PlayerRunData.dashCooldownMultiplier *= upgrade.amount;
                break;

            case UpgradeType.AttackCooldown:
                PlayerRunData.attackCooldownMultiplier *= upgrade.amount;
                break;
        }
    }
}
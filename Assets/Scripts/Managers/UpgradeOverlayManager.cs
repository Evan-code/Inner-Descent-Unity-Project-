using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This script controls the upgrade screen after clearing a room
// It shows three random upgrades, applies the chosen one, saves health, and loads the next scene
public class UpgradeOverlayManager : MonoBehaviour {
    [Header("Overlay UI")]
    [SerializeField] private GameObject upgradeOverlay;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TMP_Text[] buttonTexts;

    [Header("Scene Flow")]
    [SerializeField] private string nextSceneName = "Combat02";

    // This stops the upgrade overlay from opening more than once
    private bool overlayShowing = false;

    // This class stores info for one upgrade option
    private class UpgradeOption {
        // Text shown on the button
        public string title;

        // What kind of upgrade this is
        public UpgradeType type;

        // How strong the upgrade is
        public float amount;

        // Constructor sets up the upgrade option
        public UpgradeOption(string title, UpgradeType type, float amount) {
            this.title = title;
            this.type = type;
            this.amount = amount;
        }
    }

    // These are the possible upgrade categories
    private enum UpgradeType {
        Heal,
        Damage,
        MoveSpeed,
        DashCooldown,
        AttackCooldown
    }

    // This stores every possible upgrade
    private List<UpgradeOption> allUpgrades = new List<UpgradeOption>();

    void Start() {
        // Hide the upgrade overlay at the start
        if (upgradeOverlay != null) {
            upgradeOverlay.SetActive(false);
        }

        // Create the list of possible upgrades
        CreateUpgradePool();
    }

    private void CreateUpgradePool() {
        // Clear the list first so upgrades do not duplicate
        allUpgrades.Clear();

        // Adds a heal upgrade
        allUpgrades.Add(new UpgradeOption("+10 Health", UpgradeType.Heal, 10));

        // Adds a damage upgrade
        allUpgrades.Add(new UpgradeOption("+2 Damage", UpgradeType.Damage, 2));

        // Adds a movement speed upgrade
        allUpgrades.Add(new UpgradeOption("+15% Move Speed", UpgradeType.MoveSpeed, 1.15f));

        // Adds a shorter dash cooldown upgrade
        allUpgrades.Add(new UpgradeOption("25% Shorter Dash Cooldown", UpgradeType.DashCooldown, 0.75f));

        // Adds a shorter attack cooldown upgrade
        allUpgrades.Add(new UpgradeOption("25% Shorter Attack Cooldown", UpgradeType.AttackCooldown, 0.75f));
    }

    public void ShowUpgradeOverlay() {
        // If overlay is already showing, do nothing
        if (overlayShowing) {
            return;
        }

        // Mark overlay as showing
        overlayShowing = true;

        // Turn on the upgrade overlay UI
        if (upgradeOverlay != null) {
            upgradeOverlay.SetActive(true);
        }

        // Pause the game while choosing an upgrade
        Time.timeScale = 0f;

        // Pick and show three random upgrades
        ShowThreeRandomUpgrades();
    }

    private void ShowThreeRandomUpgrades() {
        // Make a copy of all upgrades so we can remove chosen ones
        List<UpgradeOption> available = new List<UpgradeOption>(allUpgrades);

        // Loop through the upgrade buttons
        for (int i = 0; i < upgradeButtons.Length; i++) {
            // If no upgrades are left, stop
            if (available.Count == 0) {
                break;
            }

            // Pick a random upgrade index
            int randomIndex = Random.Range(0, available.Count);

            // Get the upgrade at that random index
            UpgradeOption chosenUpgrade = available[randomIndex];

            // Remove it so it cannot be picked twice
            available.RemoveAt(randomIndex);

            // If the button text exists, change it to the upgrade title
            if (i < buttonTexts.Length && buttonTexts[i] != null) {
                buttonTexts[i].text = chosenUpgrade.title;
            }

            // If this button exists, set up what it does when clicked
            if (upgradeButtons[i] != null) {
                // Remove old button actions
                upgradeButtons[i].onClick.RemoveAllListeners();

                // Add the new button action
                upgradeButtons[i].onClick.AddListener(() => SelectUpgrade(chosenUpgrade));
            }
        }
    }

    private void SelectUpgrade(UpgradeOption chosenUpgrade) {
        // Apply the upgrade the player clicked
        ApplyUpgrade(chosenUpgrade);

        // Save health after the upgrade
        SavePlayerHealth();

        // Unpause the game
        Time.timeScale = 1f;

        // Hide the overlay
        if (upgradeOverlay != null) {
            upgradeOverlay.SetActive(false);
        }

        // Load the next scene with fade if transition manager exists
        if (SceneTransitionManager.Instance != null) {
            SceneTransitionManager.Instance.LoadSceneWithFade(nextSceneName);
        } else {
            // Warn if there is no scene transition manager
            Debug.LogError("No SceneTransitionManager found.");
        }
    }

    private void ApplyUpgrade(UpgradeOption upgrade) {
        // Checks which upgrade type was chosen
        switch (upgrade.type) {
            case UpgradeType.Heal:
                // Heal the player right away
                HealPlayer((int)upgrade.amount);
                break;

            case UpgradeType.Damage:
                // Add bonus damage to run data
                PlayerRunData.bonusDamage += (int)upgrade.amount;
                break;

            case UpgradeType.MoveSpeed:
                // Multiply movement speed upgrade value
                PlayerRunData.moveSpeedMultiplier *= upgrade.amount;
                break;

            case UpgradeType.DashCooldown:
                // Multiply dash cooldown upgrade value
                PlayerRunData.dashCooldownMultiplier *= upgrade.amount;
                break;

            case UpgradeType.AttackCooldown:
                // Multiply attack cooldown upgrade value
                PlayerRunData.attackCooldownMultiplier *= upgrade.amount;
                break;
        }
    }

    private void HealPlayer(int healAmount) {
        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // If there is no player, do nothing
        if (player == null) {
            return;
        }

        // Get the player's Health script
        Health health = player.GetComponent<Health>();

        // If there is no Health script, do nothing
        if (health == null) {
            return;
        }

        // Use Heal so the health bar also updates
        health.Heal(healAmount);
    }

    private void SavePlayerHealth() {
        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // If there is no player, do nothing
        if (player == null) {
            return;
        }

        // Get the player's Health script
        Health health = player.GetComponent<Health>();

        // If there is no Health script, do nothing
        if (health == null) {
            return;
        }

        // Only save health if player is alive
        if (health.currentHP > 0) {
            PlayerRunData.SaveHealth(health.currentHP);
        }
    }
}
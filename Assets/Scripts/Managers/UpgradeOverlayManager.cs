// These are "using" statements that let us use built-in Unity and C# features
using System.Collections.Generic; // Lets us use lists (like arrays that can grow/shrink)
using UnityEngine;                // Core Unity functionality
using UnityEngine.SceneManagement; // Lets us switch between scenes
using UnityEngine.UI;             // Lets us use UI elements like buttons
using TMPro;                      // TextMeshPro (better text system in Unity)

// This script controls an upgrade menu that appears after a level/room is cleared
public class UpgradeOverlayManager : MonoBehaviour
{
    // These variables will show up in the Unity Inspector so you can assign them
    [Header("Overlay UI")]

    // The full overlay (dark background + upgrade panel)
    [SerializeField] private GameObject upgradeOverlay;

    // The 3 buttons the player clicks to choose upgrades
    [SerializeField] private Button[] upgradeButtons;

    // The text labels shown on each button
    [SerializeField] private TMP_Text[] buttonTexts;

    [Header("Scene Flow")]

    // The name of the next scene to load after picking an upgrade
    [SerializeField] private string nextSceneName = "Combat02";

    // Keeps track of whether the upgrade screen is currently open
    private bool overlayShowing = false;

    // This class represents ONE upgrade option (like "+2 health")
    private class UpgradeOption
    {
        public string title;      // What the player sees on the button
        public UpgradeType type; // What kind of upgrade it is
        public float amount;     // How strong the upgrade is

        // Constructor (this runs when we create a new UpgradeOption)
        public UpgradeOption(string title, UpgradeType type, float amount)
        {
            this.title = title;
            this.type = type;
            this.amount = amount;
        }
    }

    // This enum (short for "enumeration") defines ALL possible upgrade types
    private enum UpgradeType
    {
        MaxHealth,
        Damage,
        MoveSpeed,
        DashCooldown,
        AttackCooldown
    }

    // This list stores ALL possible upgrades
    private List<UpgradeOption> allUpgrades = new List<UpgradeOption>();

    // Start() runs once when the game begins
    void Start()
    {
        // Make sure the upgrade overlay starts OFF (hidden)
        if (upgradeOverlay != null)
        {
            upgradeOverlay.SetActive(false);
        }
        else
        {
            Debug.LogWarning("UpgradeOverlayManager: upgradeOverlay is not assigned.");
        }

        // Create all upgrade options
        CreateUpgradePool();
    }

    // This fills the list with all possible upgrades
    void CreateUpgradePool()
    {
        // Clear the list in case it's already filled
        allUpgrades.Clear();

        // Add different upgrade options to the list
        allUpgrades.Add(new UpgradeOption("+2 Max Health", UpgradeType.MaxHealth, 2));
        allUpgrades.Add(new UpgradeOption("+1 Damage", UpgradeType.Damage, 1));
        allUpgrades.Add(new UpgradeOption("+15% Move Speed", UpgradeType.MoveSpeed, 1.15f));
        allUpgrades.Add(new UpgradeOption("10% Shorter Dash Cooldown", UpgradeType.DashCooldown, 0.90f));
        allUpgrades.Add(new UpgradeOption("10% Shorter Attack Cooldown", UpgradeType.AttackCooldown, 0.90f));
    }

    // Call this function when the player clears a room/level
    public void ShowUpgradeOverlay()
    {
        // If it's already showing, do nothing
        if (overlayShowing)
            return;

        overlayShowing = true;

        // Turn on the overlay (make it visible)
        if (upgradeOverlay != null)
        {
            upgradeOverlay.SetActive(true);
        }

        // Pause the game (time stops)
        Time.timeScale = 0f;

        Debug.Log("Upgrade overlay shown.");

        // Show 3 random upgrades
        ShowThreeRandomUpgrades();
    }

    // Picks and displays 3 random upgrades
    void ShowThreeRandomUpgrades()
    {
        // Safety check in case something wasn't assigned
        if (upgradeButtons == null || buttonTexts == null)
        {
            Debug.LogWarning("UpgradeOverlayManager: upgradeButtons or buttonTexts is null.");
            return;
        }

        // Make a copy of all upgrades so we don't repeat selections
        List<UpgradeOption> available = new List<UpgradeOption>(allUpgrades);

        // Loop through each button
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            // If we run out of upgrades, stop
            if (available.Count == 0)
                break;

            // Pick a random upgrade from the list
            int randomIndex = Random.Range(0, available.Count);
            UpgradeOption chosenUpgrade = available[randomIndex];

            // Remove it so we don't pick it again
            available.RemoveAt(randomIndex);

            // Set the button text (what the player sees)
            if (i < buttonTexts.Length && buttonTexts[i] != null)
            {
                buttonTexts[i].text = chosenUpgrade.title;
            }

            // Set what happens when the button is clicked
            if (upgradeButtons[i] != null)
            {
                // Remove any old click events
                upgradeButtons[i].onClick.RemoveAllListeners();

                // Add a new click event that selects this upgrade
                upgradeButtons[i].onClick.AddListener(() => SelectUpgrade(chosenUpgrade));
            }
        }
    }

    // This runs when the player clicks an upgrade
    void SelectUpgrade(UpgradeOption chosenUpgrade)
    {
        // Apply the upgrade to the player
        ApplyUpgrade(chosenUpgrade);

        // Debug logs to show what's happening (good for testing)
        Debug.Log("Player picked upgrade: " + chosenUpgrade.title);
        Debug.Log(
            "Run Data Now -> " +
            "bonusMaxHP: " + PlayerRunData.bonusMaxHP +
            ", bonusDamage: " + PlayerRunData.bonusDamage +
            ", moveSpeedMultiplier: " + PlayerRunData.moveSpeedMultiplier +
            ", dashCooldownMultiplier: " + PlayerRunData.dashCooldownMultiplier +
            ", attackCooldownMultiplier: " + PlayerRunData.attackCooldownMultiplier
        );

        // Unpause the game before switching scenes
        Time.timeScale = 1f;

        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }

    // This actually changes the player's stats
    void ApplyUpgrade(UpgradeOption upgrade)
    {
        // Switch checks what type of upgrade it is
        switch (upgrade.type)
        {
            case UpgradeType.MaxHealth:
                // Increase max health
                PlayerRunData.bonusMaxHP += (int)upgrade.amount;
                break;

            case UpgradeType.Damage:
                // Increase damage
                PlayerRunData.bonusDamage += (int)upgrade.amount;
                break;

            case UpgradeType.MoveSpeed:
                // Multiply movement speed
                PlayerRunData.moveSpeedMultiplier *= upgrade.amount;
                break;

            case UpgradeType.DashCooldown:
                // Reduce dash cooldown
                PlayerRunData.dashCooldownMultiplier *= upgrade.amount;
                break;

            case UpgradeType.AttackCooldown:
                // Reduce attack cooldown
                PlayerRunData.attackCooldownMultiplier *= upgrade.amount;
                break;
        }
    }
}
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This script updates the player's health UI
// It controls the main health bar, white damage bar, and health text
public class PlayerHealthUI : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image whiteDamageImage;
    [SerializeField] private TMP_Text healthText;

    [Header("White Damage Bar")]
    [SerializeField] private float waitBeforeShrink = 0.3f;
    [SerializeField] private float shrinkSpeed = 1.5f;

    // Stores the white bar coroutine so we can restart it
    private Coroutine damageBarRoutine;

    void Start() {
        // If Health was not dragged in, try finding the player by tag
        if (playerHealth == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            // If player exists, get its Health script
            if (player != null) {
                playerHealth = player.GetComponent<Health>();
            }
        }

        // If Health was found, listen to health changes
        if (playerHealth != null) {
            // This makes UpdateUI run every time health changes
            playerHealth.OnHealthChanged += UpdateUI;

            // Updates UI right away with the current health
            UpdateUI(playerHealth.currentHP, playerHealth.maxHP);

            // Extra refresh in case saved health loaded at scene start
            playerHealth.RefreshHealthUI();
        } else {
            // Warning if the player health was not found
            Debug.LogWarning("PlayerHealthUI: Could not find player Health.");
        }
    }

    void OnDestroy() {
        // Stop listening when this UI object is destroyed
        if (playerHealth != null) {
            playerHealth.OnHealthChanged -= UpdateUI;
        }
    }

    private void UpdateUI(int currentHP, int maxHP) {
        // Prevents divide by zero errors
        if (maxHP <= 0) {
            return;
        }

        // Turns health into a 0 to 1 fill amount
        float newFillAmount = (float)currentHP / maxHP;

        // Updates the main health bar
        if (fillImage != null) {
            fillImage.fillAmount = newFillAmount;
        }

        // Updates the health number text
        if (healthText != null) {
            healthText.text = currentHP + " / " + maxHP;
        }

        // Updates the white damage bar if assigned
        if (whiteDamageImage != null) {
            // If healing happened, white bar should jump up too
            if (whiteDamageImage.fillAmount < newFillAmount) {
                whiteDamageImage.fillAmount = newFillAmount;
            }

            // Stop old white bar shrink if one is running
            if (damageBarRoutine != null) {
                StopCoroutine(damageBarRoutine);
            }

            // Start shrinking the white bar toward the new health amount
            damageBarRoutine = StartCoroutine(ShrinkWhiteBar(newFillAmount));
        }
    }

    private IEnumerator ShrinkWhiteBar(float targetAmount) {
        // Wait before the white bar starts shrinking
        yield return new WaitForSeconds(waitBeforeShrink);

        // Keep shrinking while the white bar is above the target amount
        while (whiteDamageImage != null && whiteDamageImage.fillAmount > targetAmount) {
            // Move the white bar closer to the target amount
            whiteDamageImage.fillAmount = Mathf.MoveTowards(
                whiteDamageImage.fillAmount,
                targetAmount,
                shrinkSpeed * Time.deltaTime
            );

            // Wait until next frame
            yield return null;
        }

        // Make sure it lands exactly on the target
        if (whiteDamageImage != null) {
            whiteDamageImage.fillAmount = targetAmount;
        }
    }
}
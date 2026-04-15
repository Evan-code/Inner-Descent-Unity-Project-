using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This script updates the player's screen health bar and health text.
// Example display: 50 / 100
public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text healthText;

    void Start()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateUI;
            UpdateUI(playerHealth.currentHP, playerHealth.maxHP);
        }
        else
        {
            Debug.LogWarning("PlayerHealthUI: Could not find player Health.");
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateUI;
        }
    }

    void UpdateUI(int currentHP, int maxHP)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = (float)currentHP / maxHP;
        }

        if (healthText != null)
        {
            healthText.text = currentHP + " / " + maxHP;
        }
    }
}

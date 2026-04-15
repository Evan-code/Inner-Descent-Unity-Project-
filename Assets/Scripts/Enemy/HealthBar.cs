using UnityEngine;
using UnityEngine.UI;

// This script updates an enemy health bar UI.
// It listens to the enemy's Health script and changes the bar fill amount.
public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health targetHealth;
    [SerializeField] private Image fillImage;

    [Header("Look At Camera")]
    [SerializeField] private bool faceCamera = true;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (targetHealth == null)
        {
            // Try to find Health on the parent enemy automatically
            targetHealth = GetComponentInParent<Health>();
        }

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += UpdateBar;
            UpdateBar(targetHealth.currentHP, targetHealth.maxHP);
        }
        else
        {
            Debug.LogWarning("EnemyHealthBar: No Health found.");
        }
    }

    void LateUpdate()
    {
        // Make the bar face the camera
        if (faceCamera && mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }

    void OnDestroy()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= UpdateBar;
        }
    }

    void UpdateBar(int currentHP, int maxHP)
    {
        if (fillImage == null)
            return;

        fillImage.fillAmount = (float)currentHP / maxHP;
    }
}
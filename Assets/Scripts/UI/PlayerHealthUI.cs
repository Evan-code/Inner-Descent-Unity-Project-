using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image whiteDamageImage;
    [SerializeField] private TMP_Text healthText;

    [Header("White Damage Bar")]
    [SerializeField] private float waitBeforeShrink = 0.3f;
    [SerializeField] private float shrinkSpeed = 1.5f;

    private Coroutine damageBarRoutine;

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
        float newFillAmount = (float)currentHP / maxHP;

        if (fillImage != null)
        {
            fillImage.fillAmount = newFillAmount;
        }

        if (healthText != null)
        {
            healthText.text = currentHP + " / " + maxHP;
        }

        if (whiteDamageImage != null)
        {
            if (whiteDamageImage.fillAmount < newFillAmount)
            {
                whiteDamageImage.fillAmount = newFillAmount;
            }

            if (damageBarRoutine != null)
            {
                StopCoroutine(damageBarRoutine);
            }

            damageBarRoutine = StartCoroutine(ShrinkWhiteBar(newFillAmount));
        }
    }

    IEnumerator ShrinkWhiteBar(float targetAmount)
    {
        yield return new WaitForSeconds(waitBeforeShrink);

        while (whiteDamageImage.fillAmount > targetAmount)
        {
            whiteDamageImage.fillAmount = Mathf.MoveTowards(
                whiteDamageImage.fillAmount,
                targetAmount,
                shrinkSpeed * Time.deltaTime
            );

            yield return null;
        }

        whiteDamageImage.fillAmount = targetAmount;
    }
}
using System.Collections;
using UnityEngine;

// Lets the player take damage, flash, and become temporarily invincible
public class PlayerReceiveDamage : MonoBehaviour
{
    private Health health;
    private Renderer[] renderers;

    [SerializeField] private float invulnerabilityDuration = 1f;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = new Color32(236, 146, 146, 255);

    private bool isInvulnerable = false;
    private Color[] originalColors;

    void Awake()
    {
        health = GetComponent<Health>();
        renderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    public void Hit(int damage)
    {
        if (isInvulnerable || health == null)
            return;

        health.TakeDamage(damage);

        if (health.currentHP > 0)
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material.color = flashColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material.color = originalColors[i];
            }
        }

        float remainingTime = Mathf.Max(0f, invulnerabilityDuration - flashDuration);
        yield return new WaitForSeconds(remainingTime);

        isInvulnerable = false;
    }
}
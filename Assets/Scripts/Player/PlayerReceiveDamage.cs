using System.Collections;
using UnityEngine;

// This script lets the player take damage,
// flash when hit,
// and become briefly invincible.
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
            Material mat = renderers[i].material;

            if (mat.HasProperty("_BaseColor"))
                originalColors[i] = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color"))
                originalColors[i] = mat.GetColor("_Color");
            else
                originalColors[i] = Color.white;
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

        // Flash to hit color
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            Material mat = renderers[i].material;

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", flashColor);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", flashColor);
        }

        yield return new WaitForSeconds(flashDuration);

        // Return to original color
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            Material mat = renderers[i].material;

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", originalColors[i]);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", originalColors[i]);
        }

        float remainingTime = Mathf.Max(0f, invulnerabilityDuration - flashDuration);
        yield return new WaitForSeconds(remainingTime);

        isInvulnerable = false;
    }
}
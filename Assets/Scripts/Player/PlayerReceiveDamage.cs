using System.Collections;
using UnityEngine;

public class PlayerReceiveDamage : MonoBehaviour
{
    private Health health;
    private Renderer[] renderers;

    [Header("Invincibility")]
    [SerializeField] private float invulnerabilityDuration = 1f;

    [Header("Flashing")]
    [SerializeField] private float initialFlashDuration = 0.08f;
    [SerializeField] private float slowFlashInterval = 0.25f;
    [SerializeField] private Color flashColor = new Color(0.65f, 0.65f, 0.65f, 1f);

    [Header("Screen Shake")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.25f;

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
            {
                originalColors[i] = mat.GetColor("_BaseColor");
            }
            else if (mat.HasProperty("_Color"))
            {
                originalColors[i] = mat.GetColor("_Color");
            }
            else
            {
                originalColors[i] = Color.white;
            }
        }
    }

    public void Hit(int damage)
    {
        if (isInvulnerable || health == null)
        {
            return;
        }

        health.TakeDamage(damage);

        if (ScreenShake.Instance != null)
        {
            ScreenShake.Instance.Shake(shakeDuration, shakeStrength);
        }

        if (health.currentHP > 0)
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        float timer = 0f;

        SetPlayerColor(flashColor);
        yield return new WaitForSeconds(initialFlashDuration);
        RestoreOriginalColors();

        timer += initialFlashDuration;

        while (timer < invulnerabilityDuration)
        {
            SetPlayerColor(flashColor);
            yield return new WaitForSeconds(slowFlashInterval);

            RestoreOriginalColors();
            yield return new WaitForSeconds(slowFlashInterval);

            timer += slowFlashInterval * 2f;
        }

        RestoreOriginalColors();
        isInvulnerable = false;
    }

    private void SetPlayerColor(Color color)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            Material mat = renderer.material;

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }
        }
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material mat = renderers[i].material;

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", originalColors[i]);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", originalColors[i]);
            }
        }
    }
}
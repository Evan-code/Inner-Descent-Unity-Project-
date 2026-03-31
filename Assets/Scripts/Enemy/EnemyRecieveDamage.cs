using System.Collections;
using UnityEngine;

// This script lets an enemy take damage and flash when hit
public class EnemyReceiveDamage : MonoBehaviour
{
    private Health health;
    private Renderer[] renderers;

    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = new Color32(236, 146, 146, 255);

    private Color[] originalColors;
    private Coroutine flashRoutine;

    void Awake()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError("EnemyRecieveDamage could not find a Health component on " + gameObject.name);
        }

        renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("EnemyRecieveDamage found no Renderers on " + gameObject.name);
        }

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].material;

            if (mat.HasProperty("_BaseColor"))
                originalColors[i] = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color"))
                originalColors[i] = mat.GetColor("_Color");
            else
                originalColors[i] = Color.white; // fallback
        }
    }

    public void Hit(int damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].material;

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", flashColor);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", flashColor);
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].material;

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", originalColors[i]);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", originalColors[i]);
        }

        flashRoutine = null;
    }
}

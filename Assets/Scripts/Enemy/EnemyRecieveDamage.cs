using System.Collections;
using UnityEngine;

// This script lets an enemy take damage,
// flash when hit,
// and spawn floating damage numbers.
public class EnemyReceiveDamage : MonoBehaviour
{
    private Health health;
    private Renderer[] renderers;

    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = new Color32(236, 146, 146, 255);

    [Header("Damage Number")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 1.8f, 0f);
    [SerializeField] private float randomSpawnRadius = 0.5f;

    private Color[] originalColors;
    private Coroutine flashRoutine;

    void Awake()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError("EnemyReceiveDamage could not find a Health component on " + gameObject.name);
        }

        renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("EnemyReceiveDamage found no Renderers on " + gameObject.name);
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
                originalColors[i] = Color.white;
        }
    }

    public void Hit(int damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        SpawnDamageNumber(damage);

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashCoroutine());
    }

    void SpawnDamageNumber(int damage)
    {
        if (damageNumberPrefab == null)
            return;

        Vector3 randomOffset = new Vector3(
            Random.Range(-randomSpawnRadius, randomSpawnRadius),
            Random.Range(-randomSpawnRadius, randomSpawnRadius),
            Random.Range(-randomSpawnRadius, randomSpawnRadius)
        );

        Vector3 spawnPosition = transform.position + damageNumberOffset + randomOffset;

        GameObject numberObject = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);

        DamageNumber damageNumber = numberObject.GetComponent<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.SetDamage(damage);
        }
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
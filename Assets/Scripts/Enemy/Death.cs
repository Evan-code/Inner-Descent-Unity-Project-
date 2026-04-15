using System.Collections;
using UnityEngine;

// This script plays a death animation before destroying the object.
public class DieOnZero : MonoBehaviour
{
    private Health health;
    public Animator animator;

    [SerializeField] private float destroyDelay = 1.5f;

    void Start()
    {
        health = GetComponent<Health>();

        if (health != null)
        {
            health.OnDied += Die;
        }
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDied -= Die;
        }
    }

    void Die()
    {
        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Disable other behavior so the enemy stops moving/attacking
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}


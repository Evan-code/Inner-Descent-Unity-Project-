using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance;

    [SerializeField] private float defaultDuration = 0.15f;
    [SerializeField] private float defaultStrength = 0.2f;

    private Coroutine shakeCoroutine;

    void Awake()
    {
        Instance = this;
    }

    public void Shake(float duration, float strength)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    IEnumerator ShakeRoutine(float duration, float strength)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 offset = new Vector3(
                Random.Range(-strength, strength),
                Random.Range(-strength, strength),
                0f
            );

            transform.position += offset;

            yield return null;

            transform.position -= offset;

            elapsed += Time.deltaTime;
        }
    }
}
using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance;

    private Coroutine shakeCoroutine;
    private bool shakeDisabled = false;

    void Awake()
    {
        Instance = this;
    }

    public void Shake(float duration, float strength)
    {
        // If shaking has been disabled, do nothing.
        if (shakeDisabled)
            return;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    public void StopShakeForever()
    {
        // Prevent any future shake calls.
        shakeDisabled = true;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
    }

    IEnumerator ShakeRoutine(float duration, float strength)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (shakeDisabled)
                yield break;

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

        shakeCoroutine = null;
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    private bool isTransitioning = false;

    private void Awake()
    {
        // If there is already a SceneTransitionManager, destroy this duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Save this manager so other scripts can use it
        Instance = this;

        // Keep this object alive when changing scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Start with the screen clear
        SetFadeAlpha(0f);
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (isTransitioning) return;

        StartCoroutine(FadeOutLoadSceneFadeIn(sceneName));
    }

    private IEnumerator FadeOutLoadSceneFadeIn(string sceneName)
    {
        isTransitioning = true;

        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f));

        // Load the new scene while the screen is black
        SceneManager.LoadScene(sceneName);

        // Wait one frame so the new scene appears behind the black screen
        yield return null;

        // Fade back in from black
        yield return StartCoroutine(Fade(1f, 0f));

        isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);

            SetFadeAlpha(alpha);

            yield return null;
        }

        SetFadeAlpha(endAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null) return;

        fadeImage.color = new Color(0f, 0f, 0f, alpha);
    }
}
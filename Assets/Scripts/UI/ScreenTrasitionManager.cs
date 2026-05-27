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

    [Header("Scene Names")]
    public string squareRoomSceneName = "SquareRoom";
    public string mainMenuSceneName = "MainMenu";

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetFadeAlpha(0f);
    }

    // Use this for normal scene changes while the player is alive.
    public void LoadSceneWithFade(string sceneName)
    {
        if (isTransitioning) return;

        Time.timeScale = 1f;

        SaveCurrentPlayerHealth();

        StartCoroutine(FadeOutLoadSceneFadeIn(sceneName));
    }

    // Use this for the death screen restart button.
    public void LoadSquareRoomAfterDeath()
    {
        if (isTransitioning) return;

        Time.timeScale = 1f;

        PlayerRunData.ResetRun();

        StartCoroutine(FadeOutLoadSceneFadeIn(squareRoomSceneName));
    }

    // Use this for the death screen main menu button.
    public void LoadMainMenuAfterDeath()
    {
        if (isTransitioning) return;

        Time.timeScale = 1f;

        PlayerRunData.ResetRun();

        StartCoroutine(FadeOutLoadSceneFadeIn(mainMenuSceneName));
    }

    // Use this for normal main menu buttons if needed.
    public void LoadMainMenu()
    {
        if (isTransitioning) return;

        Time.timeScale = 1f;

        StartCoroutine(FadeOutLoadSceneFadeIn(mainMenuSceneName));
    }

    private void SaveCurrentPlayerHealth()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("No Player found. Health was not saved.");
            return;
        }

        Health health = player.GetComponent<Health>();

        if (health == null)
        {
            Debug.LogWarning("Player has no Health component. Health was not saved.");
            return;
        }

        if (health.currentHP > 0)
        {
            PlayerRunData.SaveHealth(health.currentHP);
        }
    }

    private IEnumerator FadeOutLoadSceneFadeIn(string sceneName)
    {
        isTransitioning = true;

        Time.timeScale = 1f;

        yield return StartCoroutine(Fade(0f, 1f));

        SceneManager.LoadScene(sceneName);

        yield return null;

        yield return StartCoroutine(Fade(1f, 0f));

        isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

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
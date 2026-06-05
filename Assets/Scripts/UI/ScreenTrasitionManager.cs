using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// This script handles scene changes
// It also fades the screen and saves health before moving to the next room
public class SceneTransitionManager : MonoBehaviour {
    // Static Instance lets other scripts find this script easily
    public static SceneTransitionManager Instance;

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    [Header("Scene Names")]
    public string squareRoomSceneName = "Square Room";
    public string mainMenuSceneName = "MainMenu";

    // This stops the player from starting multiple scene transitions at once
    private bool isTransitioning = false;

    private void Awake() {
        // If another SceneTransitionManager already exists, destroy this one
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        // Save this script as the main Instance
        Instance = this;

        // Keep this object alive when switching scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        // Makes sure the fade starts invisible
        SetFadeAlpha(0f);
    }

    public void LoadSceneWithFade(string sceneName) {
        // If already changing scenes, do nothing
        if (isTransitioning) {
            return;
        }

        // Make sure the game is not paused before loading
        Time.timeScale = 1f;

        // Save current player health before leaving the room
        SaveCurrentPlayerHealth();

        // Start the fade out, scene load, fade in process
        StartCoroutine(FadeOutLoadSceneFadeIn(sceneName));
    }

    public void LoadSquareRoomAfterDeath() {
        // If already changing scenes, do nothing
        if (isTransitioning) {
            return;
        }

        // Unpause the game
        Time.timeScale = 1f;

        // Since the player died, reset all run upgrades and saved health
        PlayerRunData.ResetRun();

        // Load the first combat room
        StartCoroutine(FadeOutLoadSceneFadeIn(squareRoomSceneName));
    }

    public void LoadMainMenuAfterDeath() {
        // If already changing scenes, do nothing
        if (isTransitioning) {
            return;
        }

        // Unpause the game
        Time.timeScale = 1f;

        // Death resets the run
        PlayerRunData.ResetRun();

        // Load main menu
        StartCoroutine(FadeOutLoadSceneFadeIn(mainMenuSceneName));
    }

    public void LoadMainMenu() {
        // If already changing scenes, do nothing
        if (isTransitioning) {
            return;
        }

        // Make sure game is unpaused
        Time.timeScale = 1f;

        // Load main menu with fade
        StartCoroutine(FadeOutLoadSceneFadeIn(mainMenuSceneName));
    }

    private void SaveCurrentPlayerHealth() {
        // Find the player object by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // If there is no player, health cannot be saved
        if (player == null) {
            Debug.LogWarning("No Player found. Health was not saved.");
            return;
        }

        // Get the Health script from the player
        Health health = player.GetComponent<Health>();

        // If the player has no Health script, health cannot be saved
        if (health == null) {
            Debug.LogWarning("Player has no Health component. Health was not saved.");
            return;
        }

        // Only save health if player is alive
        if (health.currentHP > 0) {
            PlayerRunData.SaveHealth(health.currentHP);
        }
    }

    private IEnumerator FadeOutLoadSceneFadeIn(string sceneName) {
        // Mark that a transition is happening
        isTransitioning = true;

        // Make sure time is normal during the transition
        Time.timeScale = 1f;

        // Fade from clear to black
        yield return StartCoroutine(Fade(0f, 1f));

        // Load the requested scene
        SceneManager.LoadScene(sceneName);

        // Wait one frame so the new scene can create its objects
        yield return null;

        // Fade from black back to clear
        yield return StartCoroutine(Fade(1f, 0f));

        // Transition is done
        isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha) {
        // Timer starts at 0
        float timer = 0f;

        // Keep fading until timer reaches fade duration
        while (timer < fadeDuration) {
            // Use unscaled time so fading still works if the game was paused
            timer += Time.unscaledDeltaTime;

            // Calculates alpha between start and end
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);

            // Apply that alpha to the fade image
            SetFadeAlpha(alpha);

            // Wait until next frame
            yield return null;
        }

        // Make sure the fade ends exactly at the target alpha
        SetFadeAlpha(endAlpha);
    }

    private void SetFadeAlpha(float alpha) {
        // If there is no fade image, do nothing
        if (fadeImage == null) {
            return;
        }

        // Change the fade image color while keeping it black
        fadeImage.color = new Color(0f, 0f, 0f, alpha);
    }
}
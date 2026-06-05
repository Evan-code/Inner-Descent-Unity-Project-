using UnityEngine;
using UnityEngine.SceneManagement;

// This script controls the death screen
// It shows the death screen when the player dies and handles restart buttons
public class DeathScreenManager : MonoBehaviour {
    [Header("UI")]
    [SerializeField] private GameObject deathScreen;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string squareRoomSceneName = "Square Room";

    // This stores the player's Health script
    private Health playerHealth;

    void Start() {
        // Hide the death screen when the scene starts
        if (deathScreen != null) {
            deathScreen.SetActive(false);
        }

        // Find the player using the Player tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // If the player exists, get their Health script
        if (player != null) {
            playerHealth = player.GetComponent<Health>();

            // If the player has Health, listen for death
            if (playerHealth != null) {
                playerHealth.OnDied += ShowDeathScreen;
            }
        }
    }

    void OnDestroy() {
        // Stop listening when this object gets destroyed
        if (playerHealth != null) {
            playerHealth.OnDied -= ShowDeathScreen;
        }
    }

    private void ShowDeathScreen() {
        // Turn on the death screen UI
        if (deathScreen != null) {
            deathScreen.SetActive(true);
        }

        // Pause the game while the death screen is open
        Time.timeScale = 0f;
    }

    public void RestartRoom() {
        // If the scene transition manager exists, use it
        if (SceneTransitionManager.Instance != null) {
            // This resets the run and loads Square Room
            SceneTransitionManager.Instance.LoadSquareRoomAfterDeath();
        } else {
            // Backup if the transition manager is missing
            Time.timeScale = 1f;

            // Reset health and upgrades
            PlayerRunData.ResetRun();

            // Load Square Room directly
            SceneManager.LoadScene(squareRoomSceneName);
        }
    }

    public void StartOver() {
        // If the scene transition manager exists, use it
        if (SceneTransitionManager.Instance != null) {
            // This resets the run and loads main menu
            SceneTransitionManager.Instance.LoadMainMenuAfterDeath();
        } else {
            // Backup if the transition manager is missing
            Time.timeScale = 1f;

            // Reset health and upgrades
            PlayerRunData.ResetRun();

            // Load the main menu directly
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
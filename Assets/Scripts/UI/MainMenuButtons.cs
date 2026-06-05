using UnityEngine;

// This script is used for menu buttons
// Each public function can be connected to a UI button in the Inspector
public class MainMenuButtons : MonoBehaviour {
    [Header("Scene Names")]
    public string tutorialSceneName = "Tutorial";
    public string mainMenuSceneName = "MainMenu";
    public string squareRoomSceneName = "Square Room";

    public void StartButton() {
        // Checks if the scene transition manager exists
        if (SceneTransitionManager.Instance != null) {
            // Loads the tutorial scene with fade
            SceneTransitionManager.Instance.LoadSceneWithFade(tutorialSceneName);
        }
    }

    public void MainMenuButton() {
        // Checks if the scene transition manager exists
        if (SceneTransitionManager.Instance != null) {
            // Loads the main menu scene with fade
            SceneTransitionManager.Instance.LoadSceneWithFade(mainMenuSceneName);
        }
    }

    public void SquareRoomButton() {
        // Checks if the scene transition manager exists
        if (SceneTransitionManager.Instance != null) {
            // Loads the first combat room with fade
            SceneTransitionManager.Instance.LoadSceneWithFade(squareRoomSceneName);
        }
    }

    public void RestartAfterDeathButton() {
        // Checks if the scene transition manager exists
        if (SceneTransitionManager.Instance != null) {
            // Resets the run and loads Square Room
            SceneTransitionManager.Instance.LoadSquareRoomAfterDeath();
        }
    }

    public void MainMenuAfterDeathButton() {
        // Checks if the scene transition manager exists
        if (SceneTransitionManager.Instance != null) {
            // Resets the run and loads the main menu
            SceneTransitionManager.Instance.LoadMainMenuAfterDeath();
        }
    }

    public void ExitButton() {
        // Quits the game in a built version
        Application.Quit();

#if UNITY_EDITOR
        // Stops play mode when testing inside Unity editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
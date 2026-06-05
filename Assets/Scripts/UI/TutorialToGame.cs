using UnityEngine;

// This script lets the player press Space in the tutorial to start the game
public class TutorialToGame : MonoBehaviour {
    [Header("Next Scene")]
    public string nextSceneName = "Square Room";

    // This prevents the player from pressing Space multiple times and loading twice
    private bool hasPressed = false;

    void Update() {
        // If Space was already pressed, do nothing
        if (hasPressed) {
            return;
        }

        // Checks if the player pressed Space
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Mark that the player already pressed Space
            hasPressed = true;

            // If transition manager exists, load the next scene with fade
            if (SceneTransitionManager.Instance != null) {
                SceneTransitionManager.Instance.LoadSceneWithFade(nextSceneName);
            }
        }
    }
}
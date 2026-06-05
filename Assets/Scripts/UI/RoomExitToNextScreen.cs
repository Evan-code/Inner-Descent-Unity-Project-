using UnityEngine;

// This script loads the next scene
// It is useful for a button or room exit object
public class RoomExitToNextScene : MonoBehaviour {
    [Header("Next Scene")]
    public string nextSceneName;

    public void GoToNextScene() {
        // Checks if the SceneTransitionManager exists
        if (SceneTransitionManager.Instance != null) {
            // Loads the next scene with fade
            SceneTransitionManager.Instance.LoadSceneWithFade(nextSceneName);
        }
    }
}
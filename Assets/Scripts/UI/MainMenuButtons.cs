using UnityEngine;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Scene Names")]
    public string tutorialSceneName = "Tutorial";
    public string mainMenuSceneName = "MainMenu";
    public string squareRoomSceneName = "SquareRoom";

    public void StartButton()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(tutorialSceneName);
        }
    }

    public void MainMenuButton()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(mainMenuSceneName);
        }
    }

    public void SquareRoomButton()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(squareRoomSceneName);
        }
    }

    public void RestartAfterDeathButton()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSquareRoomAfterDeath();
        }
    }

    public void MainMenuAfterDeathButton()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadMainMenuAfterDeath();
        }
    }

    public void ExitButton()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}